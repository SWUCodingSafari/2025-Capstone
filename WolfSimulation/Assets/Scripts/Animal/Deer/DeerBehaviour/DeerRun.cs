using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// todo
public class DeerRun : AnimalStateBehaviour
{
    private Vector3 diraction;
    private Vector3 predaterDir;

    public override void OnEnter()
    {
        animal.anim.SetBool(DeerAnimation.IsRunning, true);

        predaterDir = Vector3.zero;
    }

    public override void OnExit()
    {
        animal.anim.SetBool(DeerAnimation.IsRunning, false);
    }

    private void SetDirection()
    {
        const int MaxCalDeerCount = 5;
        const int MaxCalWolfCount = 3;
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        Vector3 runAway = Vector3.zero;
        int count = 0;

        foreach (var neighbor in animal.DeerList)
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
            if (count >= MaxCalDeerCount)
            {
                break;
            }
        }

        foreach (var wolf in animal.WolfList)
        {
            if (wolf.IsDied == true)
                continue;

            Vector3 toWolf = wolf.transform.position - animal.transform.position;
            float distance = toWolf.magnitude;

            // Run Away
            runAway -= toWolf.normalized / distance; // 가까울수록 더 강하게 밀어냄

            count++;
            if (count >= MaxCalWolfCount)
            {
                break;
            }
        }

        runAway = runAway.normalized.sqrMagnitude > predaterDir.sqrMagnitude ? runAway : predaterDir;

        if (count == 0)
        {
            diraction = animal.transform.forward;
        }

        alignment /= count;
        cohesion = (cohesion / count - animal.transform.position).normalized;

        // 각 요소에 가중치를 곱해서 합산
        float weightSeparation = 0.7f;
        float weightAlignment = 1.5f;
        float weightCohesion = 1.0f;
        float weightRunAway = 2.0f;

        diraction = (
            separation.normalized * weightSeparation +
            alignment.normalized * weightAlignment +
            cohesion.normalized * weightCohesion +
            runAway.normalized * weightRunAway
        );

        predaterDir = runAway.normalized;
        diraction.y = 0f;
        diraction = diraction.normalized;

        return;
    }

    public override float ReducedStamina()
    {
        return animal.BaseStatus.subStaminaByRunSec;
    }

    public override bool Update()
    {
        SetDirection();

        float runSpeed = (animal.BaseStatus.stamina / animal.BaseStatus.maxStamina) *
            (animal.BaseStatus.maxRunSpeed - animal.BaseStatus.runSpeed) +
            animal.BaseStatus.runSpeed;

        animal.TurnToDesiredDir(diraction);
        animal.MovePosition(runSpeed);

        animal.BaseStatus.fear = animal.BaseStatus.maxStamina;
        return false;
    }
}
