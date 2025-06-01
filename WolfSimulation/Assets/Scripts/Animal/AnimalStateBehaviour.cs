using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimalStateBehaviour
{
    protected Animal animal;

    public void Init(Animal _animal)
    {
        animal = _animal;
    }

    public abstract void OnEnter();
    public virtual void OnReset() { }
    public virtual float ReducedStamina()
    {
        return animal.BaseStatus.subStaminaByWalkSec;
    }
    public virtual void OnBehaviourCycle() { }
    public abstract bool Update();
    public abstract void OnExit();
}
