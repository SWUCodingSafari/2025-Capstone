using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfDie : AnimalStateBehaviour
{
    private float animTime = 0.417f;

    public override void OnEnter()
    {
        animal.IsDied = true;

        animal.anim.SetTrigger(DeerAnimation.Death);

        GAManager.Instance.AnimalDied(animal, animal.LivingTime);
    }

    public override void OnExit()
    {
    }

    public override bool Update()
    {
        return false;
    }
}
