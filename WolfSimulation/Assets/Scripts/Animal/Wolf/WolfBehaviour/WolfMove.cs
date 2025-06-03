using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfMove : AnimalStateBehaviour
{
    private Vector3 moveTargetDir = Vector3.zero;

    public override void OnEnter()
    {
        animal.anim.SetBool(DeerAnimation.IsWalking, true);
    }

    public override void OnExit()
    {
        moveTargetDir = Vector3.zero;
        animal.anim.SetBool(DeerAnimation.IsWalking, false);
    }

    public override bool Update()
    {
        SetMoveDir();

        animal.TurnToDesiredDir(moveTargetDir);
        animal.MovePosition();
        return false; // 어차피 해당 업데이트 이후 Behaviour Cycle에 따라 처리가 됨
    }

    private void SetMoveDir()
    {
        const int MaxCalWolfCount = 5;
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
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
            moveTargetDir = animal.transform.forward;
        }

        alignment /= count;
        cohesion = (cohesion / count - animal.transform.position).normalized;

        // 각 요소에 가중치를 곱해서 합산
        float weightSeparation = 1.5f;
        float weightAlignment = 1.0f;
        float weightCohesion = 1.0f;

        moveTargetDir = (
            separation.normalized * weightSeparation +
            alignment.normalized * weightAlignment +
            cohesion.normalized * weightCohesion
        );
        moveTargetDir.y = 0f;
        moveTargetDir = moveTargetDir.normalized;

        return;
    }
}
