using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OldDeer : Animal
{
    /*
     * [행동 리스트]
     * - 섭취 >> 배고픔
     * >> 배가 고플 때
     * >> 도망가야하는 상황이 아닐 때
     * 
     * - 정지 >> 배고픔, 시야거리
     * >> 그냥 딱히 막 배고프지도, 도망가야하지도 않을 경우 그냥 멈춰서 쉼
     * 
     * - 정찰 >> 배고픔, 이동 속도, 시야거리
     * >> 배고픈데 주변에 풀이 없는 경우
     * >> 주변에 무리가 없을 경우
     * 
     * - 이동 >> 배고픔, 위치
     * >> 무리와 특정 거리 이상 떨어져 있거나 특정 거리 이상 가까울 때(위치 조정)
     * >> 섭취 상태가 아닐 때
     * >> Boid
     * 
     * - 도망 >> 체력, 이동 속도, 시야거리, (+ 공격력)
     * >> 주변에 늑대가 보일때
     * >> 주변 무리가 도망 상태일때
     * >> Boid
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

    public enum DeerState
    {
        Idle,
        Eat,
        Search,
        Move,
        Run,
        Mate,
        Healing,
        Die,
        MAX,
    }

    [Header("State")]
    [SerializeField] private DeerState state;
    public DeerState State { get => state; private set => state = value; }

    [SerializeField] private float[] stateFactors = new float[(int)DeerState.MAX];
    private AnimalStateBehaviour[] stateBehaviours = 
        new AnimalStateBehaviour[(int)DeerState.MAX];

    [Header("Hunted")]
    [SerializeField] private int beingChased = 0;
    [SerializeField] private int meat = 10;

    public override void Init()
    {
        base.Init();

        state = DeerState.MAX;

        stateBehaviours[(int)DeerState.Eat] = new DeerEat();
        stateBehaviours[(int)DeerState.Idle] = new DeerIdle();
        stateBehaviours[(int)DeerState.Search] = new DeerSearch();
        stateBehaviours[(int)DeerState.Move] = new DeerMove();
        stateBehaviours[(int)DeerState.Run] = new DeerRun();
        stateBehaviours[(int)DeerState.Mate] = new DeerMate();
        stateBehaviours[(int)DeerState.Healing] = new DeerHealing();
        stateBehaviours[(int)DeerState.Die] = new DeerDie();

        foreach(var state in stateBehaviours)
        {
            state?.Init(this);
        }
    }

    private DeerState ChangeState()
    {
        if(BaseStatus.health <= 0 && BaseStatus.hunger >= BaseStatus.maxHunger)
        {
            return DeerState.Die;
        }

        int runningDeerCount = 0;//DeerList.Count(_ => _.State == DeerState.Run);
        int wolfCount = 0;
        foreach (var d in DeerList)
        {
            wolfCount += d.WolfList.Count;
        }
        if (WolfList.Count > 0 || wolfCount > 0)
            return DeerState.Run;

        if(CheckState(DeerState.Mate) == true)
            return DeerState.Mate;

        // 팩터 초기화
        for(int i = 0; i < (int)DeerState.MAX; ++i)
        {
            stateFactors[i] = -1f;
        }

        // 휴식
        stateFactors[(int)DeerState.Idle] = 
            CheckState(DeerState.Eat) ? 0f : BaseStatus.maxStamina - BaseStatus.stamina -
            BaseStatus.hungerCurve.Evaluate(BaseStatus.hunger / BaseStatus.maxHunger);

        bool isThereGrassToEat = false;
        foreach (var grass in GrassList)
        {
            if (grass.reservedBy == this || (grass.IsGrown == true && grass.reservedBy == null))
            {
                isThereGrassToEat = true;
                break;
            }
        }

        // 섭취 팩터: 풀이 없으면 정찰로 넘어가도록 유도
        stateFactors[(int)DeerState.Eat] = (isThereGrassToEat ? 1f : 0f)
            * BaseStatus.hungerCurve.Evaluate(BaseStatus.hunger / BaseStatus.maxHunger) * 
            BaseStatus.eatAdder;

        // 정찰 팩터
        bool noGrass = !isThereGrassToEat;
        bool noDeerNearby = DeerList.Count == 0;

        float hungerFactor = Mathf.Clamp01(BaseStatus.hunger / BaseStatus.maxHunger); // 배고프면 1에 가까움

        stateFactors[(int)DeerState.Search] = (noGrass || noDeerNearby ? 1f : 0f) * hungerFactor * BaseStatus.searchAdder;

        // 이동 팩터
        float distance = DeerList.Count > 0 ? (DeerList[0].transform.position - transform.position).magnitude : -1f;
        bool move = Random.Range(0, 10) < 3;
        stateFactors[(int)DeerState.Move] = (move == false || ((CheckState(DeerState.Eat) || distance < 0f)) ? 0f :
            Mathf.Abs(distance - BaseStatus.maxClusterDistance) * BaseStatus.moveAdder);

        // 번식 팩터
        bool canMate = CheckIfThisCanMate();
        if (LookingForMate != canMate && canMate == true )
        {
            // 지금 번식 가능해짐
            Mate();
        }
        LookingForMate = canMate; // 번식 가능여부
        //stateFactors[(int)DeerState.Mate] = 
        //    (isHealthy && isFull && hasGroup && safe && hasGrass ? 1f : 0f) * BaseStatus.mateAdder;

        // 회복 팩터
        bool enoughHunger = BaseStatus.hunger / BaseStatus.maxHunger >= 0.5f; // 배고픔이 50% 이상
        stateFactors[(int)DeerState.Healing] = (BaseStatus.maxHealth - BaseStatus.health) / BaseStatus.maxHealth *
            BaseStatus.heallingAdder;

        if((int) state < (int) DeerState.MAX)
            stateFactors[(int)state] += BaseStatus.hysteresisAdder;

        return (DeerState)GetTopFactor();
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
        // 풀 정리(내가 먹을 수 있는 것만 남김)
        int i = 0;
        while(i < GrassList.Count)
        {
            if(GrassList[i] == null || (GrassList[i].reservedBy != null && GrassList[i].reservedBy != gameObject))
            {
                GrassList.Remove(GrassList[i]);
            }
            else
            {
                ++i;
            }
        }
        // 풀 정렬 (가까운 순)
        GrassList.Sort((a, b) =>
        {
            if (a.IsGrown == true && b.IsGrown == false)
                return 1;
            else if (b.IsGrown == true && a.IsGrown == false)
                return -1;

            if ((a.transform.position - transform.position).sqrMagnitude <
            (b.transform.position - transform.position).sqrMagnitude)
            {
                return 1;
            }
            else if ((a.transform.position - transform.position).sqrMagnitude >
            (b.transform.position - transform.position).sqrMagnitude)
                return -1;
            else 
                return 0;
        });

        // 사슴 정렬
        i= 0;
        while (i < DeerList.Count)
        {
            if (DeerList[i] == null)
            {
                DeerList.Remove(DeerList[i]);
            }
            else
            {
                ++i;
            }
        }

        // 사슴 정렬 (가까운 순)
        DeerList.Sort((a, b) =>
        {
            if ((a.transform.position - transform.position).sqrMagnitude <
            (b.transform.position - transform.position).sqrMagnitude)
            {
                return 1;
            }
            else if((a.transform.position - transform.position).sqrMagnitude >
            (b.transform.position - transform.position).sqrMagnitude)
                return -1;
            return 0;
        });

        // 사슴 정렬
        i = 0;
        while (i < DeerList.Count)
        {
            if (DeerList[i] == null)
            {
                DeerList.Remove(DeerList[i]);
            }
            else
            {
                ++i;
            }
        }

        // 늑대 정렬 (가까운 순)
        WolfList.Sort((a, b) =>
        {
            if ((a.transform.position - transform.position).sqrMagnitude <
            (b.transform.position - transform.position).sqrMagnitude)
            {
                return 1;
            }
            else if ((a.transform.position - transform.position).sqrMagnitude >
            (b.transform.position - transform.position).sqrMagnitude)
                return -1;
            return 0;
        });

    }

    protected override void BehaviourCycle()
    {
        base.BehaviourCycle();

        BaseStatus.fear = Mathf.Clamp(BaseStatus.fear - BaseStatus.subfearBySec * Time.deltaTime, 0f, BaseStatus.maxFear);

        float subBySec = CheckState(DeerState.MAX) ? BaseStatus.subStaminaByWalkSec :
            stateBehaviours[(int)state].ReducedStamina();
        BaseStatus.stamina = Mathf.Clamp(BaseStatus.stamina - subBySec * Time.deltaTime, 0f, BaseStatus.maxStamina);

        if(CheckState(DeerState.MAX) == false)
            stateBehaviours[(int)state].OnBehaviourCycle();

        if(BaseStatus.health <= 0f && BaseStatus.hunger >= BaseStatus.maxHunger)
        {
            SelectStateAndBehave((int)DeerState.Die);
        }
    }

    public override void OnEnviromentChanged()
    {
        if (IsDied == true)
            return;

        // 환경 조건 정리
        UpdateEnviroment();

        SelectStateAndBehave();
    }

    protected override void SelectStateAndBehave(int _newState = -1)
    {
        if (IsDied == true)
            return;

        var newState = _newState >= 0 ? (DeerState)_newState : ChangeState();

        // 이미 그 행동을 진행 중
        if (CheckState(newState) == true)
        {
            stateReset?.Invoke();
            return;
        }

        //Debug.Log($"Current State: {state}, Change State: {newState}, id: {id}");
        state = newState;
        stateChanged?.Invoke();
        stateReset = null;

        currentBehaviour = stateBehaviours[(int)state].Update;
        stateReset = stateBehaviours[(int)state].OnReset;
        stateChanged = stateBehaviours[(int)state].OnExit;

        stateBehaviours[(int)state].OnEnter();
    }

    private bool CheckState(DeerState _checkState)
    {
        return state == _checkState;
    }

    public override void Mate(Animal _mateAnimal = null, bool isRequested = false)
    {
        Debug.Log($"{id} want to mate");
        if (_mateAnimal == null) // 이제 짝 찾기
        {
            mate = AnimalManager<OldDeer>.Instance.WantMate(this);
        }
        else
        {
            mate = _mateAnimal;
        }

        if (mate == null) // 지금은 짝이 없음
        {
            return;
        }

        if (isRequested == false)
        {
            mate.Mate(this, true);
        }
        SelectStateAndBehave((int)DeerState.Mate);
    }

    public override void MateOver(Animal _mateAnimal = null)
    {
        if (IsMating == false)
            return;

        IsMating = false;
        BaseStatus.urgeToMate -= BaseStatus.subUrgeToMateAfterMate;

        if (_mateAnimal == null) // 내가 번식을 종료함
        {
            mate.MateOver(this);
            GiveBirth(mate);
        }

        SelectStateAndBehave((int)DeerState.Idle);
    }

    public override bool CheckIfThisCanMate()
    {
        bool isHealthy = BaseStatus.health >= BaseStatus.maxHealth * 0.5f;
        bool isFull = BaseStatus.hunger / BaseStatus.maxHunger <= 0.3f;
        bool hasGroup = PackNumber >= 0;
        bool safe = CheckState(DeerState.Run) == false;
        bool hasGrass = GrassList.Count(g => g.IsGrown == true && g.reservedBy == null) > 0;
        bool urgeIsEnough = BaseStatus.urgeToMate >= BaseStatus.maxUrgeToMate * 0.5f;

        return isHealthy && isFull && hasGroup && safe && hasGrass && urgeIsEnough;
    }

    protected override void GiveBirth(Animal _other)
    {
        OldDeer baby = Instantiate(this);
        baby.BaseStatus.SetStatus(this.BaseStatus);
        baby.transform.position = (transform.position + _other.transform.position) / 2f;
    }

    public override void GetDamaged(float _value)
    {
        BaseStatus.health = Mathf.Clamp(BaseStatus.health - _value, 0, BaseStatus.maxHealth);
        Debug.Log($"Get Hurt {BaseStatus.health} ");
        OnEnviromentChanged();
    }

    public bool GetEaten()
    {
        meat = Mathf.Clamp(meat - 1, 0, meat);
        Debug.Log($"Get Hurt {meat} ");

        if (meat <= 0)
        {
            Destroy(gameObject);
        }

        return meat > 0;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        OldDeer otherDeer = null;
        if(IsThereComponent<OldDeer>(other, out otherDeer) == false)
        {
            return;
        }

        AnimalManager<OldDeer>.Instance.MeetPack(this, otherDeer);
        if (BaseStatus.coLoosePack != null)
        {
            StopCoroutine(BaseStatus.coLoosePack);
            BaseStatus.coLoosePack = null;
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        OldDeer otherDeer = null;
        if (IsThereComponent<OldDeer>(other, out otherDeer) == false)
        {
            return;
        }

        if(DeerList.Count <= 0)
        {
            // 주변에 무리가 보이지 않음
            BaseStatus.coLoosePack = StartCoroutine(CoLoosePack<OldDeer>());
        }
    }



    private void OnDestroy()
    {
    }
}
