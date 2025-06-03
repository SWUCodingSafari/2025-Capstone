using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public abstract class Animal : MonoBehaviour
{
    [Header("Component")]
    [SerializeField] public Rigidbody rigid;
    [SerializeField] public Renderer rend;
    [SerializeField] public Collider sight;
    [SerializeField] public AnimalAnimation anim;

    [field: SerializeField]
    public AnimalStatus BaseStatus { get; set; }
    
    private Material mat;
    public Material Mat => mat;

    // near by enviroment
    public List<Grass> GrassList { get; set; }
    public List<Deer> DeerList { get; set; }
    public List<Wolf> WolfList { get; set; }

    protected UnityAction enviromentChanged;
    protected UnityAction stateChanged;
    protected UnityAction stateReset;

    protected delegate bool _CurrentBehaviour();
    protected _CurrentBehaviour currentBehaviour;

    public bool IsDied { get; set; }

    [SerializeField] protected bool lookingForMate = false;
    public bool LookingForMate
    {
        get => lookingForMate;
        set => lookingForMate = value;
    }
    public bool IsMating { get; set; }
    protected Animal mate = null;

    // 무리 관리용
    public int PackNumber { get; set; } = -1;

    public int id;

    public virtual void Awake()
    {
        id = Random.Range(0, 1231);
        Init();
    }

    protected virtual void Init()
    {
        BaseStatus.health = BaseStatus.maxHealth;
        BaseStatus.hunger = 0f;
        BaseStatus.urgeToMate = 0f;
        BaseStatus.fear = 0f;

        (sight as SphereCollider).radius = BaseStatus.viewDist;
        mat = rend?.material;
        enviromentChanged = OnEnviromentChanged;

        GrassList = new List<Grass>();
        DeerList = new List<Deer>();
        WolfList = new List<Wolf>();

        BaseStatus.wfLoosePack = new WaitForSeconds(BaseStatus.loosePackTime);
    }

    public virtual void Update()
    {
        if (IsDied == true)
            return;

        // 해당 행동 진행 이후 변경 사항 있을 경우 다시 환경 체크
        if(currentBehaviour?.Invoke() == true)
        {
            enviromentChanged?.Invoke();
        }

        BehaviourCycle(); // 배고픔 처리
        SelectStateAndBehave(); // 배고픔 팩터 수정 후 다시 상태 변화
    }

    protected abstract void SelectStateAndBehave(int _newState = -1);
    public abstract void OnEnviromentChanged();

    protected virtual void BehaviourCycle()
    {
        BaseStatus.urgeToMate = Mathf.Clamp(BaseStatus.urgeToMate + BaseStatus.addUrgeToMateBySec * Time.deltaTime, 0f, BaseStatus.maxUrgeToMate);
        BaseStatus.hunger = Mathf.Clamp(BaseStatus.hunger + BaseStatus.addHungerBySec * Time.deltaTime, 0f, BaseStatus.maxHunger);

        if(BaseStatus.hunger >= BaseStatus.maxHunger)
        {
            BaseStatus.health = Mathf.Clamp(BaseStatus.health - BaseStatus.reduceHealthBySec * Time.deltaTime, 0f, BaseStatus.maxHealth);
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

            GrassList.Add(grass);
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

            DeerList.Add(deer);
            enviromentChanged?.Invoke();
            return;
        }

        Wolf wolf;
        if (IsThereComponent<Wolf>(other, out wolf) == true)
        {
            if (wolf as Animal == this)
            {
                // 자기자신 검출 추가 안함
                return;
            }

            WolfList.Add(wolf);
            enviromentChanged?.Invoke();
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

        Deer deer;
        if (IsThereComponent<Deer>(other, out deer) == true)
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

    protected virtual IEnumerator CoLoosePack<T>() where T : Animal
    {
        yield return BaseStatus.wfLoosePack;
        AnimalManager<T>.Instance.LoosePack(this as T);
    }

    public virtual void UpdateEnviroment()
    {

    }

    public virtual bool CheckIfThisCanMate()
    {
        return false;
    }
    public virtual void Mate(Animal _mateAnimal = null) { }
    public virtual void MateOver(Animal _mateAnimal = null) { }

    public void MovePosition(float _moveSpeed = -1f)
    {
        if(_moveSpeed < 0f) _moveSpeed = BaseStatus.moveSpeed;
        rigid.MovePosition(transform.position + transform.forward * BaseStatus.moveSpeed * Time.deltaTime);
    }

    public void TurnToDesiredDir(Vector3 _targetDir)
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            Quaternion.LookRotation(_targetDir),
            BaseStatus.maxTurnAngleBySec * Time.deltaTime);
    }

    public virtual void Die()
    {
        // todo.
        Destroy(gameObject, 5f);
    }

    protected abstract void GiveBirth(Animal _other);
}
