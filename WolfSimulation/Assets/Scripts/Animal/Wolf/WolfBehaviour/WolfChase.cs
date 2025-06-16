using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class WolfChase : AnimalStateBehaviour
{
    private WolfStatus status;

    private Deer targetDeer = null;
    private float elapsedTime = 0f;
    private bool canAttack = true;

    private Vector3 chaseVec = Vector3.zero;

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

        SetMoveDir();

        if(canAttack == false)
        {
            elapsedTime += Time.deltaTime;

            canAttack = elapsedTime >= status.attackCoolTime;
        }
        
        if ((targetDeer.transform.position - animal.transform.position).sqrMagnitude <= 
            Mathf.Pow(status.attackRange, 2f))
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

        animal.TurnToDesiredDir(chaseVec.normalized);
        animal.MovePosition(runSpeed);

        return false;
    }

    private void SetMoveDir()
    {
        const int MaxCalWolfCount = 5;
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        Vector3 chase = targetDeer.transform.position - animal.transform.position;
        int count = 0;

        foreach (var neighbor in animal.WolfList)
        {
            if (neighbor.IsDied == true)
                continue;

            Vector3 toNeighbor = neighbor.transform.position - animal.transform.position;
            float distance = toNeighbor.magnitude;

            // Separation
            if (distance < animal.BaseStatus.minClusterDistance)
            {
                separation -= toNeighbor.normalized / distance; // 가까울수록 더 강하게 밀어냄
            }

            // Alignment
            alignment += neighbor.transform.forward; // 진행 방향

            // Cohesion
            cohesion += neighbor.transform.position;

            count++;
            if (count >= MaxCalWolfCount)
            {
                break;
            }
        }

        if (count == 0)
        {
            chaseVec = animal.transform.forward;
        }

        alignment /= count;
        cohesion = (cohesion / count - animal.transform.position).normalized;

        // 각 요소에 가중치를 곱해서 합산
        float weightSeparation = 1.5f;
        float weightAlignment = 1.0f;
        float weightCohesion = 1.0f;
        float weightChase = 1.5f;

        chaseVec = (
            separation.normalized * weightSeparation +
            alignment.normalized * weightAlignment +
            cohesion.normalized * weightCohesion +
            chase.normalized * weightChase
        );
        chaseVec.y = 0f;
        chaseVec = chaseVec.normalized;

        return;
    }

    private Deer SetTarget()
    {
        animal.UpdateEnviroment();

        return animal.DeerList[0];
    }
}
