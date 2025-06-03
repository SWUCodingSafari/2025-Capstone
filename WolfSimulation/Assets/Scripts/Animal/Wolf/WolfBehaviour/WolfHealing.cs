using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfHealing : AnimalStateBehaviour
{
    public override void OnEnter()
    {
        animal.anim.SetBool(DeerAnimation.IsHealing, true);
    }

    public override void OnExit()
    {
        animal.anim.SetBool(DeerAnimation.IsHealing, false);
    }

    public override bool Update()
    {
        animal.BaseStatus.health =
            Mathf.Clamp(animal.BaseStatus.health + animal.BaseStatus.addHealthByHealingSec * Time.deltaTime,
            0f, animal.BaseStatus.maxHealth);
        return true;
    }
}
