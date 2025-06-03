using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfDie : AnimalStateBehaviour
{

    public override void OnEnter()
    {
        animal.IsDied = true;

        animal.anim.SetTrigger(DeerAnimation.Death);
    }

    public override void OnExit()
    {
    }

    public override bool Update()
    {
        return false;
    }
}
