using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfChase : AnimalStateBehaviour
{
    private WolfStatus status;

    private Deer targetDeer = null;
    private float elapsedTime = 0f;

    public override void OnEnter()
    {
        animal.anim.SetBool(WolfAnimation.IsChasing, true);

        status = animal.BaseStatus as WolfStatus;
    }

    public override void OnExit()
    {
        animal.anim.SetBool(WolfAnimation.IsChasing, false);
    }

    public override bool Update()
    {
        targetDeer = SetTarget();
        if (targetDeer == null)
            return true;

        Debug.Log($"Target id: {targetDeer.id}, ({animal.id})");

        Vector3 subVec = targetDeer.transform.position - animal.transform.position;
        subVec.y = 0;

        elapsedTime += Time.deltaTime;
        bool canAttack = elapsedTime >= status.attackCoolTime;
        
        if (subVec.sqrMagnitude <=  Mathf.Pow(status.attackRange, 2f))
        {
            if(canAttack)
            {
                elapsedTime -= status.attackCoolTime;
                animal.anim.SetTrigger(WolfAnimation.Attack);

            }
        }

        float runSpeed = (status.stamina / status.maxStamina) *
            (status.runSpeed - status.moveSpeed) +
            status.moveSpeed *
            (canAttack ? 1f : status.slowAfterAttack);

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
