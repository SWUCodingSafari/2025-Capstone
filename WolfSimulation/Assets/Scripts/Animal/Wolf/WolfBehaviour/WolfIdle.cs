using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfIdle : AnimalStateBehaviour
{
    private float elapsedTime = 0f;
    private float randomActTime = 2f;

    public override void OnEnter()
    {
        animal.anim.SetBool(WolfAnimation.IsWalking, false);
        animal.anim.SetBool(WolfAnimation.IsChasing, false);

        elapsedTime = 0f;
    }

    public override void OnExit()
    {
    }

    public override float ReducedStamina()
    {
        return 0f;
    }

    public override bool Update()
    {
        elapsedTime += Time.fixedDeltaTime;
        if (elapsedTime >= randomActTime)
        {
            elapsedTime -= randomActTime;

            int randomNum = Random.Range(0, 20);
            if (randomNum < 2)
                animal.anim.SetTrigger(WolfAnimation.IdleRandom1);
            else if (randomNum < 4)
                animal.anim.SetTrigger(WolfAnimation.IdleRandom2);
        }

        animal.BaseStatus.stamina = Mathf.Clamp(animal.BaseStatus.stamina + animal.BaseStatus.addStaminaBySec * Time.fixedDeltaTime,
            0, animal.BaseStatus.maxStamina);
        return false;
    }
}
