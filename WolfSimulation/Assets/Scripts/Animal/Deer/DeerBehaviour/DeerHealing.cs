using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerHealing : AnimalStateBehaviour
{
    public override void OnEnter()
    {
        animal.animator.speed = 0f;
    }

    public override void OnExit()
    {
        animal.animator.speed = 1f;
    }

    public override bool Update()
    {
        animal.BaseStatus.health = 
            Mathf.Clamp(animal.BaseStatus.health + animal.BaseStatus.addHealthByHealingSec * Time.deltaTime, 
            0f, animal.BaseStatus.maxHealth);
        return true;
    }
}
