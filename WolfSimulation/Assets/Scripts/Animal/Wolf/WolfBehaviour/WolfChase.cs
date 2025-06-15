using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class WolfChase : AnimalStateBehaviour
{
    private WolfStatus status;

    private Deer targetDeer = null;
    private float elapsedTime = 0f;
    private bool canAttack = true;

    public override void OnEnter()
    {
        animal.anim.SetBool(WolfAnimation.IsChasing, true);

        status = animal.BaseStatus as WolfStatus;
        targetDeer = SetTarget();

        canAttack = true;
    }

    public override void OnExit()
    {
        animal.anim.SetBool(WolfAnimation.IsChasing, false);
    }

    public override bool Update()
    {
        Debug.Log($"Target id: {targetDeer.id}, ({animal.id})");

        Vector3 subVec = targetDeer.transform.position - animal.transform.position;
        subVec.y = 0;

        if(canAttack == false)
        {
            elapsedTime += Time.deltaTime;

            canAttack = elapsedTime >= status.attackCoolTime;
        }
        
        if (subVec.sqrMagnitude <=  Mathf.Pow(status.attackRange, 2f))
        {
            if (canAttack)
            {
                elapsedTime -= status.attackCoolTime;
                animal.anim.SetTrigger(WolfAnimation.Attack);

                (animal as Wolf).Attack(targetDeer);
                if(targetDeer.IsDied == true)
                {
                    return true;
                }

                canAttack = false;
                return false;
            }
        }

        float runSpeed = ((status.stamina / status.maxStamina) *
            (status.maxRunSpeed - status.runSpeed) +
            status.runSpeed) * (1f - status.slowAfterAttack) * (elapsedTime / status.attackCoolTime) + status.slowAfterAttack;

        animal.TurnToDesiredDir(subVec.normalized);
        animal.MovePosition(runSpeed);

        return false;
    }

    private Deer SetTarget()
    {
        animal.UpdateEnviroment();

        return animal.DeerList[0];
    }
}
