using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerMate : AnimalStateBehaviour
{
    private float elapsedTime = 0f;

    public override void OnEnter()
    {
        animal.LookingForMate = false;
        animal.IsMating = true;

        elapsedTime = 0f;
    }

    public override void OnExit()
    {
        animal.IsMating = false;
    }

    public override float ReducedStamina()
    {
        return animal.BaseStatus.subStaminaByMateSec;
    }

    public override bool Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime > animal.BaseStatus.mateTime)
        {
            animal.MateOver();

            return true;
        }

        return false;
    }
}
