using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NDeerRun : AnimalStateBehaviour
{
    NewDeer deer;
    NewDeerState state;

    private Vector3 diraction;
    private Vector3 predaterDir;

    public override void OnEnter()
    {
        deer = obj.GetComponent<NewDeer>();
        state = deer.BaseStatus;

        deer.anim.SetBool(DeerAnimation.IsRunning, true);

        predaterDir = Vector3.zero;
    }

    public override void OnExit()
    {
        deer.anim.SetBool(DeerAnimation.IsRunning, false);

    }

    public override bool Update()
    {
        return false;
    }

    public override void OnFixedUpdate()
    {
        SetDirection();

        deer.TurnToDesiredDir(diraction);
        deer.MovePosition(state.RunSpeed);

        if (deer.WolfList.Count <= 0)
            deer.SelectStateAndBehave();
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

        foreach (var neighbor in deer.DeerList)
        {
            if (neighbor.IsDied == true)
                continue;

            Vector3 toNeighbor = neighbor.transform.position - deer.transform.position;
            float distance = toNeighbor.magnitude;

            // Separation
            if (distance < state.MinClusterDistance)
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

        foreach (var wolf in deer.WolfList)
        {
            if (wolf.IsDied == true)
                continue;

            Vector3 toWolf = wolf.transform.position - deer.transform.position;
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
            diraction = deer.transform.forward;
        }

        alignment /= count;
        cohesion = (cohesion / count - deer.transform.position).normalized;

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

}
