using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NDeerMove : AnimalStateBehaviour
{
    NewDeer deer;
    NewDeerState state;

    float time = 0f;
    float elapsedTime = 0f;

    private Vector3 moveTargetDir = Vector3.zero;

    public override void OnEnter()
    {
        deer = obj.GetComponent<NewDeer>();
        state = deer.BaseStatus;

        elapsedTime = 0f;
        time = Random.Range(state.MinStateTime, state.MaxStateTime);

        deer.anim.SetBool(DeerAnimation.IsWalking, true);
    }

    public override void OnExit()
    {
        moveTargetDir = Vector3.zero;
        deer.anim.SetBool(DeerAnimation.IsWalking, false);
    }

    public override bool Update()
    {
        return false; // 어차피 해당 업데이트 이후 Behaviour Cycle에 따라 처리가 됨
    }

    private void SetMoveDir()
    {
        const int MaxCalDeerCount = 5;
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        int count = 0;

        foreach (var neighbor in deer.DeerList)
        {
            if (neighbor.IsDied == true)
                continue;

            Vector3 toNeighbor = neighbor.transform.position - deer.transform.position;
            float distance = toNeighbor.magnitude;

            // Separation
            if (distance < deer.BaseStatus.MinClusterDistance)
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

        if (count == 0)
        {
            moveTargetDir = deer.transform.forward;
        }

        alignment /= count;
        cohesion = (cohesion / count - deer.transform.position).normalized;

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

    public override void OnFixedUpdate()
    {
        elapsedTime += Time.fixedDeltaTime;
        if ((elapsedTime >= time))
        {
            deer.SelectStateAndBehave();
            return;
        }

        SetMoveDir();

        deer.TurnToDesiredDir(moveTargetDir);
        deer.MovePosition();
    }
}
