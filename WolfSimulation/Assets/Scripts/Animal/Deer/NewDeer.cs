using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class NewDeer : MonoBehaviour
{
    public enum NDeerState
    {
        Eat,
        Idle,
        Search,
        Move,
        Run,
        Die,
        MAX,
    }

    [Serializable]
    private class StatePercentage
    {
        public NDeerState state;
        [Range(1, 100)] public float value;
        public float Percentage { get; set; }
    }

    [Header("Status")]
    [SerializeField] private float meat;
    [SerializeField] private float minMeatReturn;
    [SerializeField] private float maxMeetReturn;

    [Header("StatePercentage")]
    [SerializeField] private StatePercentage[] percentages
        = new StatePercentage[(int)NDeerState.MAX];

    [Header("State")]
    [SerializeField] private NDeerState state;
    public NDeerState State { get => state; private set => state = value; }

    //[SerializeField] private float[] stateFactors = new float[(int)NDeerState.MAX];
    private AnimalStateBehaviour[] stateBehaviours =
        new AnimalStateBehaviour[(int)NDeerState.MAX];

    [Header("Component")]
    [SerializeField] public Rigidbody rigid;
    [SerializeField] public Renderer rend;
    [SerializeField] public Collider sight;
    [SerializeField] public AnimalAnimation anim;

    [field: SerializeField]
    public NewDeerState BaseStatus { get; set; }

    private Material mat;
    public Material Mat => mat;


    private bool isDied;
    public bool IsDied
    {
        get => isDied;
        set
        {
            Debug.Log($"die {value}");
            isDied = value;
        }
    }


    protected UnityAction enviromentChanged;
    protected UnityAction stateChanged;
    protected UnityAction stateReset;
    protected UnityAction fixedUpdate;

    protected delegate bool _CurrentBehaviour();
    protected _CurrentBehaviour currentBehaviour;


    public void Awake()
    {
        stateBehaviours[(int)NDeerState.Eat] = new NDeerEat();
        stateBehaviours[(int)NDeerState.Idle] = new NDeerIdle();
        stateBehaviours[(int)NDeerState.Search] = new NDeerSearch();
        stateBehaviours[(int)NDeerState.Move] = new NDeerMove();
        stateBehaviours[(int)NDeerState.Run] = new NDeerRun();
        stateBehaviours[(int)NDeerState.Die] = new NDearDie();

        float total = 0;
        foreach(var p in percentages)
        {
            total += p.value;
        }

        float per = 0f;
        foreach(var p in percentages)
        {
            p.Percentage = per + p.value / total;
            per = p.Percentage;
        }

        foreach (var state in stateBehaviours)
        {
            state?.Init(gameObject);
        }

        SelectStateAndBehave();
    }

    private void FixedUpdate()
    {
        TileMapManager.Instance.LeaveDeerScent(transform.position, BaseStatus.scentRadius, BaseStatus.scentStrength);

        if (IsDied == true)
            return;

        fixedUpdate?.Invoke();
    }

    public bool Attacked(float damage)
    {
        BaseStatus.Health -= damage;
        if (BaseStatus.Health < 0f)
        {
            IsDied = true;
            SelectStateAndBehave((int)NDeerState.Die);
            return true;
        }
        return false;
    }

    public float Eaten()
    {
        if(minMeatReturn > meat)
        {
            meat = 0f;
            Destroy(gameObject);
            return meat;
        }

        float meatReturn = UnityEngine.Random.Range(minMeatReturn, maxMeetReturn);
        meat -= meatReturn;

        if (meatReturn <= 0f)
        {
            Destroy(gameObject, 0.1f);
            return -1f;
        }

        return meatReturn;
    }

    public void SelectStateAndBehave(int _newState = -1)
    {
        if (IsDied == true)
            return;

        var newState = _newState >= 0 ? (NDeerState)_newState : GetRandomState();

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
        fixedUpdate = stateBehaviours[(int)state].OnFixedUpdate;
        stateReset = stateBehaviours[(int)state].OnReset;
        stateChanged = stateBehaviours[(int)state].OnExit;

        stateBehaviours[(int)state].OnEnter();
    }

    private NDeerState GetRandomState()
    {
        float num = UnityEngine.Random.Range(0f, 1f);
        foreach (var p in percentages)
        {
            if (p.Percentage >= num)
            {
                return p.state;
            }
        }

        return NDeerState.Idle;
    }

    private bool CheckState(NDeerState state)
    {
        return this.state == state;
    }

    #region LookOutEnviroment
    // near by enviroment
    public List<Grass> GrassList { get; set; } = new List<Grass>();
    public List<NewDeer> DeerList { get; set; } = new List<NewDeer>();
    public List<Wolf> WolfList { get; set; } = new List<Wolf>();
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (IsDied == true)
            return;

        //if( true /* todo: 시야 거리 관련 조건 필요 시 추가*/ )
        {
            CheckTriggedObjAndAdd(other);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (IsDied == true)
            return;

        //if( true /* todo: 시야 거리 관련 조건 필요 시 추가*/ )
        {
            CheckTriggerObjAndRemove(other);
        }
    }

    protected virtual void CheckTriggedObjAndAdd(Collider other)
    {
        Grass grass;
        if (IsThereComponent<Grass>(other, out grass) == true)
        {
            if (grass.reservedBy != null)
                return;

            GrassList.Add(grass);
            enviromentChanged?.Invoke();
            return;
        }

        NewDeer deer;
        if (IsThereComponent<NewDeer>(other, out deer) == true)
        {
            if (deer == this ||
                DeerList.Any(_ => _ == deer))
            {
                // 자기자신 검출 추가 안함
                return;
            }

            DeerList.Add(deer);
            enviromentChanged?.Invoke();
            return;
        }

        Wolf wolf;
        if (IsThereComponent<Wolf>(other, out wolf) == true)
        {
            if (wolf as Animal == this ||
                WolfList.Any(_ => _ == wolf))
            {
                // 자기자신 검출 추가 안함
                return;
            }

            WolfList.Add(wolf);
            enviromentChanged?.Invoke();

            SelectStateAndBehave((int)NDeerState.Run);
            return;
        }
    }

    protected virtual void CheckTriggerObjAndRemove(Collider other)
    {
        Grass grass;
        if (IsThereComponent<Grass>(other, out grass) == true)
        {
            GrassList.Remove(grass);
            enviromentChanged?.Invoke();
            return;
        }

        NewDeer deer;
        if (IsThereComponent<NewDeer>(other, out deer) == true)
        {
            DeerList.Remove(deer);
            enviromentChanged?.Invoke();
            return;
        }

        Wolf wolf;
        if (IsThereComponent<Wolf>(other, out wolf) == true)
        {
            WolfList.Remove(wolf);
            enviromentChanged?.Invoke();
            return;
        }
    }

    protected bool IsThereComponent<T>(Collider other, out T comp) where T : MonoBehaviour
    {
        if (other.TryGetComponent<T>(out comp) == true)
            return true;

        comp = other.GetComponentInChildren<T>();
        if (comp != null)
            return true;

        comp = other.GetComponentInParent<T>();
        if (comp != null)
            return true;

        return false;
    }
    #endregion


    public void MovePosition(float _moveSpeed = -1f)
    {
        if (_moveSpeed < 0f) _moveSpeed = BaseStatus.WalkSpeed;
        rigid.MovePosition(transform.position + transform.forward * _moveSpeed * Time.deltaTime);
    }

    public void TurnToDesiredDir(Vector3 _targetDir)
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            Quaternion.LookRotation(_targetDir),
            BaseStatus.MaxTurnAngleBySec * Time.deltaTime);
    }
}
