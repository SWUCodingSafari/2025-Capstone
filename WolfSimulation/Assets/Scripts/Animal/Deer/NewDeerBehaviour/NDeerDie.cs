using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NDearDie : AnimalStateBehaviour
{
    NewDeer deer;
    NewDeerState state;

    public override void OnEnter()
    {
        deer = obj.GetComponent<NewDeer>();
        state = deer.BaseStatus;

        deer.IsDied = true;

        deer.anim.SetTrigger(DeerAnimation.Death);
    }

    public override void OnExit()
    {

    }

    public override bool Update()
    {
        return false;
    }
}
