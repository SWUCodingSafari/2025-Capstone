using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimalStateBehaviour
{
    protected Animal animal;
    protected GameObject obj;

    public void Init(Animal _animal)
    {
        animal = _animal;
    }

    public void Init(GameObject _obj)
    {
        obj = _obj;
    }

    public abstract void OnEnter();
    public virtual void OnReset() { }
    public virtual float ReducedStamina()
    {
        return animal.BaseStatus.subStaminaByWalkSec;
    }
    public virtual void OnBehaviourCycle() { }
    public abstract bool Update();
    public virtual void OnFixedUpdate() { }
    public abstract void OnExit();
}
