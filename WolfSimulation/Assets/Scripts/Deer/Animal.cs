using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public abstract class Animal : MonoBehaviour
{
    [Header("Component")]
    [SerializeField] protected Rigidbody rigid;
    [SerializeField] protected Renderer rend;
    [SerializeField] protected Collider sight;
    private Material mat;
    public Material Mat => mat;

    [Header("Point")]
    [SerializeField] protected float health = 20f;
    [SerializeField] protected float maxHealth = 20f;
    [SerializeField] protected float reduceHealthBySec = 1f;

    [SerializeField] protected float hunger = 10f;
    [SerializeField] protected float subHungerWhenEat = 3f;
    [SerializeField] protected float maxHunger = 10f;
    [SerializeField] protected AnimationCurve hungerCurve;
    [SerializeField] protected float addHungerBySec = 0.5f;

    [SerializeField] protected float viewDist = 5f;
    [SerializeField] protected float moveSpeed = 1f;
    [SerializeField] protected float runSpeed = 2f;

    [SerializeField] protected float maxClusterDistance = 3f;
    [SerializeField] protected float minClusterDistance = 1.5f;

    [SerializeField] protected float loosePackTime = 5f;
    protected WaitForSeconds wfLoosePack;
    protected Coroutine coLoosePack = null;

    [Header("PointAdder")]
    [SerializeField] protected float hungerToRestAdder = 3f;
    [SerializeField] protected float eatAdder = 2f;
    [SerializeField] protected float searchAdder = 6f;
    [SerializeField] protected float mateAdder = 7f;

    // near by enviroment
    protected List<Grass> grassList;
    protected List<Deer> deerList;
    protected List<Transform> wolfList; // debug, 늑대 스크립트 생기면 그때 수정

    protected UnityAction enviromentChanged;
    protected UnityAction stateChanged;

    protected delegate bool _CurrentBehaviour();
    protected _CurrentBehaviour currentBehaviour;

    protected bool isDied = false;
    protected bool wantToMate = false;
    public bool WantToMate => wantToMate;

    // 무리 관리용
    public int PackNumber { get; set; } = -1;

    public virtual void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        health = maxHealth;

        hunger = 0f;

        (sight as SphereCollider).radius = viewDist;
        mat = rend?.material;
        enviromentChanged = OnEnviromentChanged;

        grassList = new List<Grass>();
        deerList = new List<Deer>();
        wolfList = new List<Transform>();

        wfLoosePack = new WaitForSeconds(loosePackTime);
    }

    public virtual void Update()
    {
        if (isDied == true)
            return;

        // 해당 행동 진행 이후 변경 사항 있을 경우 다시 환경 체크
        if(currentBehaviour?.Invoke() == true)
        {
            enviromentChanged?.Invoke();
        }

        BehaviourCycle(); // 배고픔 처리
        SelectStateAndBehave(); // 배고픔 팩터 수정 후 다시 상태 변화
    }

    protected abstract void SelectStateAndBehave();
    protected abstract void OnEnviromentChanged();

    protected virtual void BehaviourCycle()
    {
        hunger = Mathf.Clamp(hunger + addHungerBySec * Time.deltaTime, 0f, maxHunger);

        if(hunger >= maxHunger)
        {
            health = Mathf.Clamp(health - reduceHealthBySec * Time.deltaTime, 0f, maxHealth);
        }
    }
    public abstract void GetDamaged(float _value);

    #region LookOutEnviroment
    protected virtual void OnTriggerEnter(Collider other)
    {
        //if( true /* todo: 시야 거리 관련 조건 필요 시 추가*/ )
        {
            CheckTriggedObjAndAdd(other);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
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

            grassList.Add(grass);
            enviromentChanged?.Invoke();
            return;
        }

        Deer deer;
        if(IsThereComponent<Deer>(other, out deer) == true)
        {
            if(deer as Animal == this)
            {
                // 자기자신 검출 추가 안함
                return;
            }

            deerList.Add(deer);
            enviromentChanged?.Invoke();
            return;
        }

        // todo: 늑대 추가바람
    }

    protected virtual void CheckTriggerObjAndRemove(Collider other)
    {
        Grass grass;
        if (IsThereComponent<Grass>(other, out grass) == true)
        {
            grassList.Remove(grass);
            enviromentChanged?.Invoke();
            return;
        }

        Deer deer;
        if (IsThereComponent<Deer>(other, out deer) == true)
        {
            deerList.Remove(deer);
            enviromentChanged?.Invoke();
            return;
        }

        // todo: 늑대 추가바람

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

    protected virtual IEnumerator CoLoosePack<T>() where T : Animal
    {
        yield return wfLoosePack;
        AnimalManager<T>.Instance.LoosePack(this as T);
    }
}
