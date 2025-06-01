using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// todo
public class DeerRun : AnimalStateBehaviour
{
    public override void OnEnter()
    {
    }

    public override void OnExit()
    {
    }

    public override float ReducedStamina()
    {
        return animal.BaseStatus.subStaminaByRunSec;
    }

    public override bool Update()
    {
        animal.MovePosition();
        return false;
    }
}
