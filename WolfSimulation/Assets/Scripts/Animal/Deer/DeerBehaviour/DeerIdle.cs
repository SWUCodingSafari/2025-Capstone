using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DeerIdle : AnimalStateBehaviour
{
    public override void OnEnter()
    {
        animal.animator.speed = 0f;
    }

    public override void OnExit()
    {
        animal.animator.speed = 1f;
    }

    public override float ReducedStamina()
    {
        return 0f;
    }

    public override bool Update()
    {
        animal.BaseStatus.stamina = Mathf.Clamp(animal.BaseStatus.stamina + animal.BaseStatus.addStaminaBySec * Time.deltaTime,
            0, animal.BaseStatus.maxStamina);
        return false;
    }
}
