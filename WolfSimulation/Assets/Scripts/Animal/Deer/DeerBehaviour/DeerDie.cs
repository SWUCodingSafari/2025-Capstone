using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerDie : AnimalStateBehaviour
{

    public override void OnEnter()
    {
        animal.IsDied = true;

        animal.animator.speed = 0f;
    }

    public override void OnExit()
    {
    }

    public override bool Update()
    {
        return false;
    }
}
