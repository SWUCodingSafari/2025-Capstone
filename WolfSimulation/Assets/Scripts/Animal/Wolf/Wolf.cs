using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Deer;

public class Wolf : Animal
{/*
     * [행동 리스트]
     * - 섭취 >> 배고픔
     * >> 배가 고플 때
     * >> 도망가야하는 상황이 아닐 때
     * 
     * - 정지 >> 배고픔, 시야거리
     * >> 그냥 딱히 막 배고프지도, 도망가야하지도 않을 경우 그냥 멈춰서 쉼
     * 
     * - 정찰 >> 배고픔, 이동 속도, 시야거리
     * >> 현재 우리 무리가 통계적으로 배고픔 수치가 70% 이하이고 주변에 사슴이 없을 경우
     * >> 주변에 무리가 없을 경우
     * 
     * - 이동 >> 배고픔, 위치
     * >> 무리와 특정 거리 이상 떨어져 있거나 특정 거리 이상 가까울 때(위치 조정)
     * >> 섭취 상태가 아닐 때
     * >> Boid
     * 
     * - 추격 >> 체력, 이동 속도, 시야거리, (+ 공격력)
     * >> 주변에 사슴이 보일때
     * >> Boid
     * 
     * - 공격 >> 이동 속도, 시야거리
     * >> 타겟으로 삼은 사슴이 공격반경 안에 들어왔을 경우
     * 
     * - 번식 >> 체력, 배고픔
     * >> 체력과 배고픔이 충분 할때
     * >> 도망가야 하는 상황이 아닐 때
     * >> 주변에 풀이 많이 있을 때
     * 
     * - 아사 >> 체력, 배고픔
     * >> 체력이 0이고, 배고픔이 0인 상태일 때
     * >> *체력이 0이어도 배고픔이 남아있다면 휴식 상태로 전환한다.
     * 
     * - 휴식 >> 체력, 배고픔
     * >> 딱히 뭔가 할 필요 없을 때(배고픔이 특정 퍼센트 이하 + 무리 있음 + 풀 있음)
     * >> 체력이 최대 체력 이하고, 배고픔이 특정 퍼센트 이상일 때
     * >> 배고픔을 일정량 줄이고, 체력을 일정량 회복
     */

    public enum WolfState
    {
        Idle,
        Eat,
        Search,
        Move,
        Chase,
        Mate,
        Healing,
        Die,
        MAX,
    }

    [Header("State")]
    [SerializeField] private WolfState state;
    public WolfState State { get => state; private set => state = value; }

    [SerializeField] private float[] stateFactors = new float[(int)WolfState.MAX];
    private AnimalStateBehaviour[] stateBehaviours =
        new AnimalStateBehaviour[(int)WolfState.MAX];

    protected override void Init()
    {
        base.Init();

        state = WolfState.MAX;

        stateBehaviours[(int)WolfState.Idle] = new WolfIdle();
        stateBehaviours[(int)WolfState.Eat] = new WolfEat();
        stateBehaviours[(int)WolfState.Search] = new WolfSearch();
        stateBehaviours[(int)WolfState.Move] = new WolfMove();
        stateBehaviours[(int)WolfState.Chase] = new WolfChase();
        stateBehaviours[(int)WolfState.Mate] = new WolfMate();
        stateBehaviours[(int)WolfState.Healing] = new WolfHealing();
        stateBehaviours[(int)WolfState.Die] = new WolfDie();

        foreach (var state in stateBehaviours)
        {
            state?.Init(this);
        }
    }

    private WolfState ChangeState()
    {
        if (BaseStatus.health <= 0 && BaseStatus.hunger <= 0)
        {
            return WolfState.Die;
        }

        if (CheckState(WolfState.Chase) == true)
            return WolfState.Chase;

        // 팩터 초기화
        for (int i = 0; i < (int)WolfState.MAX; ++i)
        {
            stateFactors[i] = -1f;
        }

        // 휴식
        stateFactors[(int)DeerState.Idle] =
            CheckState(WolfState.Eat) ? 0f : BaseStatus.maxStamina - BaseStatus.stamina -
            BaseStatus.hungerCurve.Evaluate(BaseStatus.hunger / BaseStatus.maxHunger);

        // 주변에 사슴이 있는가?
        bool isThereDeer = DeerList.Any();
        bool isThereDeadDeer = DeerList.Any(_ => _.IsDied == true);
        bool hasTargetDeerInRange = DeerList.Any(d =>
            (d.transform.position - transform.position).sqrMagnitude <= Mathf.Pow((BaseStatus as WolfStatus).attackRange, 2));

        float hungerRatio = BaseStatus.hunger / BaseStatus.maxHunger;
        float staminaRatio = BaseStatus.stamina / BaseStatus.maxStamina;
        float healthRatio = BaseStatus.health / BaseStatus.maxHealth;

        // 섭취
        stateFactors[(int)WolfState.Eat] = (isThereDeadDeer ? 1f : 0f) * hungerRatio * BaseStatus.eatAdder;

        // 추격
        stateFactors[(int)WolfState.Chase] = (isThereDeer ? 1f : 0f) * staminaRatio * (BaseStatus as WolfStatus).chaseAdder;

        // 정찰 (무리 배고픔 평균)
        bool noWolfNearby = WolfList.Count == 0;
        bool packHungry = WolfList.Count == 0 || WolfList.Average(w => w.BaseStatus.hunger) / BaseStatus.maxHunger < 0.7f;
        stateFactors[(int)WolfState.Search] = (noWolfNearby || packHungry ? 1f : 0f) * hungerRatio * BaseStatus.searchAdder;

        // 이동 (무리와 거리 조정)
        bool isMoving = Random.Range(0, 10) < 8;
        float distToNearestWolf = WolfList.Count > 0 ? (WolfList[0].transform.position - transform.position).magnitude : -1f;
        stateFactors[(int)WolfState.Move] = (isMoving == false || (CheckState(WolfState.Eat) || distToNearestWolf < 0f) ? 0f :
            Mathf.Abs(distToNearestWolf - BaseStatus.maxClusterDistance) * BaseStatus.moveAdder);

        // 번식
        bool canMate = CheckIfThisCanMate();
        if (lookingForMate != canMate && canMate == true)
        {
            Mate();
        }
        lookingForMate = canMate;

        // 회복
        bool enoughHunger = hungerRatio >= 0.5f;
        stateFactors[(int)WolfState.Healing] = (BaseStatus.maxHealth - BaseStatus.health) / BaseStatus.maxHealth *
            BaseStatus.heallingAdder;

        // 정지 (휴식 아님. 그냥 딱히 안 굶주렸고 도망 안 가도 됨)
        bool idleCondition = !CheckState(WolfState.Eat) && !CheckState(WolfState.Chase);
        stateFactors[(int)WolfState.Idle] = idleCondition ?
            BaseStatus.maxStamina - BaseStatus.stamina - BaseStatus.hungerCurve.Evaluate(hungerRatio) : 0f;


        if ((int)state < (int)WolfState.MAX)
            stateFactors[(int)state] += BaseStatus.hysteresisAdder;

        return (WolfState)GetTopFactor();
    }

    private int GetTopFactor()
    {
        int index = 0;
        float max = stateFactors[0];

        for (int i = 1; i < stateFactors.Length; i++)
        {
            if (stateFactors[i] > max)
            {
                max = stateFactors[i];
                index = i;
            }
        }

        return index;
    }

    public override void UpdateEnviroment()
    {
        // 사슴 정렬 (가까운 순)
        DeerList.Sort((a, b) =>
        {
            float alength = (a.transform.position - transform.position).sqrMagnitude;
            float blength = (b.transform.position - transform.position).sqrMagnitude;

            if (alength < blength)
            {
                return 1;
            }
            else if(alength > blength)
            {
                return -1;
            }
            else
            {
                if (a.BaseStatus.stamina > b.BaseStatus.stamina)
                {
                    return 1;
                }
                else if (a.BaseStatus.stamina < b.BaseStatus.stamina)
                    return -1;
                else
                    return 0;
            }

        });

        // 늑대 정렬 (가까운 순)
        WolfList.Sort((a, b) =>
        {
            if ((a.transform.position - transform.position).sqrMagnitude <
            (b.transform.position - transform.position).sqrMagnitude)
            {
                return 1;
            }
            else
                return -1;
        });

    }

    public override void Update()
    {
        base.Update();
    }

    protected override void BehaviourCycle()
    {
        base.BehaviourCycle();


        float subBySec = CheckState(WolfState.MAX) ? BaseStatus.subStaminaByWalkSec :
            stateBehaviours[(int)state].ReducedStamina();
        BaseStatus.stamina = Mathf.Clamp(BaseStatus.stamina - subBySec * Time.deltaTime, 0f, BaseStatus.maxStamina);

        if(CheckState(WolfState.MAX) == false )
            stateBehaviours[(int)state].OnBehaviourCycle();
    }

    public override void OnEnviromentChanged()
    {
        // 환경 조건 정리
        UpdateEnviroment();

        SelectStateAndBehave();
    }

    protected override void SelectStateAndBehave(int _newState = -1)
    {
        var newState = _newState >= 0 ? (WolfState)_newState : ChangeState();

        // 이미 그 행동을 진행 중
        if (CheckState(newState) == true)
        {
            stateReset?.Invoke();
            return;
        }

        Debug.Log($"Current State: {state}, Change State: {newState}, id: {id}");
        state = newState;
        stateChanged?.Invoke();
        stateReset = null;

        currentBehaviour = stateBehaviours[(int)state].Update;
        stateReset = stateBehaviours[(int)state].OnReset;
        stateChanged = stateBehaviours[(int)state].OnExit;

        stateBehaviours[(int)state].OnEnter();
    }

    private bool CheckState(WolfState _checkState)
    {
        return state == _checkState;
    }

    public override void Mate(Animal _mateAnimal = null)
    {
        if (_mateAnimal == null) // 이제 짝 찾기
        {
            mate = AnimalManager<Wolf>.Instance.WantMate(this);
        }
        else
        {
            mate = _mateAnimal;
        }

        if (mate == null) // 지금은 짝이 없음
        {
            return;
        }

        mate.Mate(this);
        SelectStateAndBehave((int)WolfState.Mate);
    }

    public override void MateOver(Animal _mateAnimal = null)
    {
        IsMating = false;
        BaseStatus.urgeToMate -= BaseStatus.subUrgeToMateAfterMate;

        if (_mateAnimal == null) // 내가 번식을 종료함
        {
            mate.MateOver(this);
            GiveBirth(mate);
        }

        OnEnviromentChanged();
    }

    public override bool CheckIfThisCanMate()
    {
        bool isHealthy = BaseStatus.health >= BaseStatus.maxHealth * 0.5f;
        bool isFull = BaseStatus.hunger / BaseStatus.maxHunger >= 0.5f;
        bool hasGroup = PackNumber >= 0;
        bool urgeIsEnough = BaseStatus.urgeToMate >= BaseStatus.maxUrgeToMate;

        return isHealthy && isFull && hasGroup && urgeIsEnough;
    }

    protected override void GiveBirth(Animal _other)
    {
        Wolf baby = Instantiate(this);
        baby.BaseStatus = BaseStatus.GetNewStatus(_other.BaseStatus);
    }

    public void Attack(Deer _target)
    {
        _target.GetDamaged((BaseStatus as WolfStatus).damage);
    }

    public override void GetDamaged(float _value)
    {
        BaseStatus.health = Mathf.Clamp(BaseStatus.health - _value, 0, BaseStatus.maxHealth);
        OnEnviromentChanged();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        Wolf otherWolf = null;
        if (IsThereComponent<Wolf>(other, out otherWolf) == false)
        {
            return;
        }

        AnimalManager<Wolf>.Instance.MeetPack(this, otherWolf);
        if (BaseStatus.coLoosePack != null)
        {
            StopCoroutine(BaseStatus.coLoosePack);
            BaseStatus.coLoosePack = null;
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        Wolf otherDeer = null;
        if (IsThereComponent<Wolf>(other, out otherDeer) == false)
        {
            return;
        }

        if (DeerList.Count <= 0)
        {
            // 주변에 무리가 보이지 않음
            BaseStatus.coLoosePack = StartCoroutine(CoLoosePack<Wolf>());
        }
    }

    private void OnDestroy()
    {
    }
}
