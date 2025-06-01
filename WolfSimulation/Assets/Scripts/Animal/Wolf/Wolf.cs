using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
     * >> 배고픈데 주변에 풀이 없는 경우
     * >> 주변에 무리가 없을 경우
     * 
     * - 이동 >> 배고픔, 위치
     * >> 무리와 특정 거리 이상 떨어져 있거나 특정 거리 이상 가까울 때(위치 조정)
     * >> 섭취 상태가 아닐 때
     * >> Boid
     * 
     * - 추격 >> 체력, 이동 속도, 시야거리, (+ 공격력)
     * >> 주변에 늑대가 보일때
     * >> 주변 무리가 도망 상태일때
     * >> Boid
     * 
     * - 공격 >> 이동 속도, 시야거리, 
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
        Eat,
        Idle,
        Search,
        Move,
        Chase,
        Attack,
        Mate,
        Healing,
        Die,
        MAX,
    }

    [SerializeField]
    protected float moveAdder = 7f;

    [Header("State")]
    [SerializeField] private DeerState state;
    public DeerState State { get => state; private set => state = value; }

    [SerializeField] private float[] stateFactors = new float[(int)DeerState.MAX];

    [SerializeField] private Animator animator;

    protected override void Init()
    {
        base.Init();

        state = DeerState.MAX;
    }

    private DeerState ChangeState()
    {
        if (BaseStatus.health <= 0 && BaseStatus.hunger <= 0)
        {
            return DeerState.Die;
        }

        //float runPoint = wolfList.Count + deerList.Count(_ => _.State == DeerState.Run);
        //if (runPoint > 0)
        //    return DeerState.Run;

        // 팩터 초기화
        for (int i = 0; i < (int)DeerState.MAX; ++i)
        {
            stateFactors[i] = -1f;
        }

        // 휴식
        stateFactors[(int)DeerState.Idle] =
            CheckState(DeerState.Eat) ? 0f : BaseStatus.maxStamina - BaseStatus.stamina;

        // 섭취 팩터: 풀이 없으면 정찰로 넘어가도록 유도
        stateFactors[(int)DeerState.Eat] = (GrassList.Any(g => g.IsGrown == true && g.reservedBy == null) ? 1f : 0f)
            * BaseStatus.hungerCurve.Evaluate(BaseStatus.hunger / BaseStatus.maxHunger);

        // 정찰 팩터
        bool noGrass = GrassList.All(g => g.reservedBy != null);
        bool noDeerNearby = DeerList.Count == 0;

        float hungerFactor = Mathf.Clamp01(BaseStatus.hunger / BaseStatus.maxHunger); // 배고프면 1에 가까움

        stateFactors[(int)DeerState.Search] = (noGrass || noDeerNearby ? 1f : 0f) * hungerFactor * BaseStatus.searchAdder;

        // 이동 팩터
        float distance = DeerList.Count > 0 ? (DeerList[0].transform.position - transform.position).magnitude : -1f;
        stateFactors[(int)DeerState.Move] = (CheckState(DeerState.Eat) || distance < 0f ? 0f :
            Mathf.Abs(distance - BaseStatus.maxClusterDistance) * moveAdder);

        // 번식 팩터
        //bool isHealthy = BaseStatus.health >= BaseStatus.maxHealth * 0.9f;
        //bool isFull = BaseStatus.hunger / BaseStatus.maxHunger >= 0.8f;
        //bool hasGroup = deerList.Count > 0;
        //bool safe = !CheckState(DeerState.Run);
        //bool hasGrass = grassList.Count(g => g.reservedBy == null) >= 3;

        //stateFactors[(int)DeerState.Mate] =
        //    (isHealthy && isFull && hasGroup && safe && hasGrass ? 1f : 0f) * BaseStatus.mateAdder;

        // 회복 팩터
        bool enoughHunger = BaseStatus.hunger / BaseStatus.maxHunger >= 0.5f; // 배고픔이 50% 이상
        stateFactors[(int)DeerState.Healing] = (enoughHunger ? 1f : 0f) *
            Mathf.Clamp01((BaseStatus.maxHealth - BaseStatus.health) / BaseStatus.maxHealth) * BaseStatus.hungerToRestAdder;

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

    private void SortLists()
    {
        // 풀 정리(내가 먹을 수 있는 것만 남김)
        int i = 0;
        while (i < GrassList.Count)
        {
            if (GrassList[i] == null || (GrassList[i].reservedBy != null && GrassList[i].reservedBy != gameObject))
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
            else
                return -1;
        });

        // 사슴 정렬 (가까운 순)
        DeerList.Sort((a, b) =>
        {
            if ((a.transform.position - transform.position).sqrMagnitude <
            (b.transform.position - transform.position).sqrMagnitude)
            {
                return 1;
            }
            else
                return -1;
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

        // 스태미나
        if (CheckState(DeerState.Idle) == true)
            return;

        float subSBySec = BaseStatus.subStaminaByWalkSec;
        if (CheckState(DeerState.Chase) == true)
        {
            subSBySec = BaseStatus.subStaminaByRunSec;
        }

        BaseStatus.stamina = Mathf.Clamp(BaseStatus.stamina - subSBySec * Time.deltaTime, 0f, BaseStatus.maxStamina);
    }

    public override void OnEnviromentChanged()
    {
        // 환경 조건 정리
        SortLists();

        SelectStateAndBehave();
    }

    protected override void SelectStateAndBehave(int _newState = -1)
    {
        var newState = ChangeState();

        // 이미 그 행동을 진행 중
        if (CheckState(newState) == true)
        {
            stateReset?.Invoke();
            return;
        }

        Debug.Log($"Current State: {state}, Change State: {newState}");
        state = newState;
        stateChanged?.Invoke();
        stateReset = null;

        switch (newState)
        {
            case DeerState.Eat: OnEat(); break;
            case DeerState.Idle: OnIdle(); break;
            case DeerState.Search: OnSearch(); break;
            case DeerState.Move: OnMove(); break;
            case DeerState.Chase: OnRun(); break;
            case DeerState.Attack: break;
            case DeerState.Mate: break;
            case DeerState.Healing: break;
            case DeerState.Die: Die(); break;
        }

    }

    private bool CheckState(DeerState _checkState)
    {
        return state == _checkState;
    }

    #region Eat
    [SerializeField] private GameObject TG;
    private Grass targetGrass = null;
    public Grass TGrass
    {
        get => targetGrass;
        set
        {
            targetGrass = value;
            if (value == null)
            {
                print("널뜸");
            }
        }
    }
    private void OnEat()
    {
        OnEatReset();

        currentBehaviour = EatUpdate;
        stateReset = OnEatReset;
        stateChanged = OnEatExit;
    }

    private void OnEatReset()
    {
        if (TGrass != null)
            return;

        TGrass = GrassList.Find(_ => _.IsGrown && _.reservedBy == null);
        if (TGrass == null)
        {
            OnEnviromentChanged();
            return;
        }

        TGrass.reservedBy = gameObject;
        TG = targetGrass.gameObject;
    }

    private bool EatUpdate()
    {
        if (TGrass == null)
            return true;

        Vector3 subVec = TGrass.transform.position - transform.position;
        subVec.y = 0f;
        TurnToDesiredDir(subVec.normalized);
        rigid.MovePosition(transform.position + transform.forward * BaseStatus.moveSpeed * Time.deltaTime);

        if (subVec.sqrMagnitude <= 0.5f)
        {
            // todo: 임시 코드
            GrassList.Remove(TGrass);
            TGrass.OnMouseDown();
            BaseStatus.hunger = Mathf.Clamp(BaseStatus.hunger - BaseStatus.subHungerWhenEat, 0, BaseStatus.maxHunger);
            return true;
        }
        return false;
    }

    private void OnEatExit()
    {
        Debug.Log("Eat Exit");
        if (TGrass == null)
            return;

        TGrass.reservedBy = null;
        TGrass = null;
    }
    #endregion

    #region Idle
    private void OnIdle()
    {
        currentBehaviour = IdleUpdate;
        stateChanged = IdleExit;

        animator.speed = 0f;
    }

    private bool IdleUpdate()
    {
        BaseStatus.stamina = Mathf.Clamp(BaseStatus.stamina + BaseStatus.addStaminaBySec * Time.deltaTime,
            0, BaseStatus.maxStamina);
        return false;
    }

    private void IdleExit()
    {
        animator.speed = 1f;

        return;
    }

    #endregion

    #region Search

    private Vector3 searchDir = Vector3.zero;
    private enum SearchType
    {
        None,
        SearchPack,
        SearchGrass,
        SearchBoth,
        MAX
    }
    private SearchType searchType = SearchType.None;
    private void OnSearch()
    {
        currentBehaviour = SearchUpdate;
        stateReset = SearchReset;
        stateChanged = OnSearchExit;

        SearchReset();
    }

    private void SearchReset()
    {
        int grassCount = GrassList.Count;
        int deerCount = DeerList.Count;

        if (grassCount > 0 && deerCount == 0) // 풀은 있는데 무리가 없음
        {
            searchType = SearchType.SearchPack;
            MoveToGrass();
        }
        else if (deerCount > 0 && grassCount == 0) // 무리는 있는데 풀이 없음
        {
            searchType = SearchType.SearchGrass;
            FollowPack();
        }
        else // deerCount == 0 && grassCount == 0 >> 무리도 풀도 없음
        {
            searchType = SearchType.SearchBoth;
            SearchRandom();
        }
    }

    private float searchTime = 8f;
    private float searchElapsedTime = 0f;
    private bool SearchUpdate()
    {
        if (searchType == SearchType.SearchBoth)
        {
            searchElapsedTime += Time.deltaTime;
            if (searchElapsedTime >= searchTime)
            {
                // todo: 자연스럽게 하기 위해서 조금 멈췄다가 다시 이동하는 걸로 해야함
                searchElapsedTime -= searchTime;
                PickSearchDir();
            }
        }

        // 종료 조건 없음, 만약 풀이나 사슴을 찾았다면,
        // 그 쪽에서 환경 변화가 발생했다고 트리거감
        TurnToDesiredDir(searchDir);
        rigid.MovePosition(rigid.position + transform.forward * BaseStatus.moveSpeed * Time.deltaTime);

        return false;
    }

    private void OnSearchExit()
    {
        searchElapsedTime = 0f;
        searchDir = Vector3.zero;

        stateReset = null;

    }

    private void MoveToGrass()
    {
        // 무리가 없긴하지만, 일단 있는 자원인 풀 쪽으로 향하는 것이 생존상 유리

        SortLists();

        Grass targetG = GrassList[GrassList.Count / 2]; // 적당한 거리의 풀을 선택
        searchDir = (targetG.transform.position - transform.position).normalized;
        searchDir.y = 0f;
        searchDir = searchDir.normalized;
    }

    private void FollowPack()
    {
        // 무리가 향하는 방향 => 풀이 있을 가능성 높음
        Vector3 center = Vector3.zero;
        foreach (var deer in DeerList)
            center += deer.transform.position;
        center /= DeerList.Count;

        searchDir = (center - transform.position).normalized;
    }

    private void SearchRandom()
    {
        currentSearchWay = 0b10000; // 새로운 방향 초기화 어느 뱡향도 아님

        PickSearchDir();
    }

    private enum SearchWay : byte
    {
        Forward = 0b0100,
        Backward = 0b1000,
        Side_Left = 0b0000,     // Side_right와 동일, 비트 연산을 위함
        Side_Right = 0b1100,    // Side_left와 동일, 비트 연산을 위함

        Left = 0b0001,
        Right = 0b0010,
        Straight_Up = 0b0000,   // Straight_Down과 동일, 비트 연산을 위함
        Straight_Down = 0b0011, // Straight_Up과 동일, 비트 연산을 위함
    }
    private byte currentSearchWay = 0b10000;
    private void PickSearchDir()
    {
        byte newDir = 0b00;

        // 새로운 방향이 기존 방향과 동일하지 않고, 완전 반대 방향이 아닌 경우
        do
        {
            newDir = (byte)((Random.Range(0, 4) << 2) + Random.Range(0, 4));
        } while (((newDir == currentSearchWay) && (~newDir == currentSearchWay)));

        currentSearchWay = newDir;

        // 앞 뒤 방향 선택(로컬 기준)
        if ((currentSearchWay & (byte)SearchWay.Forward) == (byte)SearchWay.Forward)
        {
            searchDir = transform.forward;
        }
        else if ((currentSearchWay & (byte)SearchWay.Backward) == (byte)SearchWay.Backward)
        {
            searchDir = -transform.forward;
        }

        // 양옆 방향 선택(로컬 기준)
        if ((currentSearchWay & (byte)SearchWay.Left) == (byte)SearchWay.Left)
        {
            searchDir -= transform.right;
        }
        else if ((currentSearchWay & (byte)SearchWay.Right) == (byte)SearchWay.Right)
        {
            searchDir += transform.right;
        }
    }

    #endregion

    #region Move
    private void OnMove()
    {
        currentBehaviour = MoveUpdate;
        stateChanged = OnMoveExit;
    }

    private Vector3 moveTargetDir = Vector3.zero;
    private bool MoveUpdate()
    {
        SetMoveDir();

        TurnToDesiredDir(moveTargetDir);
        rigid.MovePosition(transform.position + transform.forward * BaseStatus.moveSpeed * Time.deltaTime);
        return false; // 어차피 해당 업데이트 이후 Behaviour Cycle에 따라 처리가 됨
    }

    private void OnMoveExit()
    {
        moveTargetDir = Vector3.zero;
    }

    private void SetMoveDir()
    {
        const int MaxCalDeerCount = 5;
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        int count = 0;

        foreach (var neighbor in DeerList)
        {
            Vector3 toNeighbor = neighbor.transform.position - transform.position;
            float distance = toNeighbor.magnitude;

            // Separation
            if (distance < BaseStatus.minClusterDistance)
            {
                separation -= toNeighbor.normalized / distance; // 가까울수록 더 강하게 밀어냄
            }

            // Alignment
            alignment += neighbor.transform.forward; // 진행 방향

            // Cohesion
            cohesion += neighbor.transform.position;

            count++;
            if (count >= MaxCalDeerCount)
            {
                break;
            }
        }

        if (count == 0)
        {
            moveTargetDir = transform.forward;
        }

        alignment /= count;
        cohesion = (cohesion / count - transform.position).normalized;

        // 각 요소에 가중치를 곱해서 합산
        float weightSeparation = 1.5f;
        float weightAlignment = 1.0f;
        float weightCohesion = 1.0f;

        moveTargetDir = (
            separation.normalized * weightSeparation +
            alignment.normalized * weightAlignment +
            cohesion.normalized * weightCohesion
        );
        moveTargetDir.y = 0f;
        moveTargetDir = moveTargetDir.normalized;

        return;
    }

    #endregion

    #region Run
    private void OnRun()
    {
        // todo: 여기서 도망칠 방향 처리(boid)

        currentBehaviour = RunUpdate;
        stateChanged = OnRunExit;
    }

    private bool RunUpdate()
    {
        rigid.MovePosition(transform.position + transform.forward * BaseStatus.runSpeed * Time.deltaTime);
        return false; // 환경이 변경되면 알아서 처리됨
    }

    private void OnRunExit()
    {

    }

    #endregion

    #region Mate
    private Deer mateDeer = null;
    private void OnMate()
    {
        mateElapsedTime = 0f;

        currentBehaviour = MateUpdate;
        stateChanged = OnMateExit;
    }

    private float mateTime = 2f;
    private float mateElapsedTime = 0f;
    private bool MateUpdate()
    {
        // todo: 생략
        return false;

        //if (mateDeer == null)
        //{
        //    // 주변에 번식이 가능한 사슴을 찾음
        //    mateDeer = AnimalManager<Deer>.Instance.WantMate(this);
        //    return false;
        //}
        //mateElapsedTime += Time.deltaTime;
        //if (mateElapsedTime > mateTime)
        //{
        //    return true;
        //}

        return false;
    }

    private void OnMateExit()
    {
        lookingForMate = false;
    }
    #endregion

    #region Die
    private void Die()
    {
        IsDied = true;

        // todo: 디버그 용
        Destroy(gameObject, 1f);
    }
    #endregion

    public override void GetDamaged(float _value)
    {
        BaseStatus.health = Mathf.Clamp(BaseStatus.health - _value, 0, BaseStatus.maxHealth);
        OnEnviromentChanged();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        //Deer otherDeer = null;
        //if (IsThereComponent<Deer>(other, out otherDeer) == false)
        //{
        //    return;
        //}

        //AnimalManager<Deer>.Instance.MeetPack(this, otherDeer);
        //if (BaseStatus.coLoosePack != null)
        //{
        //    StopCoroutine(BaseStatus.coLoosePack);
        //    BaseStatus.coLoosePack = null;
        //}
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        Deer otherDeer = null;
        if (IsThereComponent<Deer>(other, out otherDeer) == false)
        {
            return;
        }

        if (DeerList.Count <= 0)
        {
            // 주변에 무리가 보이지 않음
            BaseStatus.coLoosePack = StartCoroutine(CoLoosePack<Deer>());
        }
    }


    private void OnDestroy()
    {
    }


    private void TurnToDesiredDir(Vector3 _targetDir)
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            Quaternion.LookRotation(_targetDir),
            BaseStatus.maxTurnAngleBySec * Time.deltaTime);
    }

    protected override void GiveBirth(Animal _other)
    {
        throw new System.NotImplementedException();
    }
}
