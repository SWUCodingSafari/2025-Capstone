using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NDeerIdle : AnimalStateBehaviour
{
    NewDeer deer;
    NewDeerState state;

    float time = 0f;
    float elapsedTime = 0f;

    float animElapsedTime = 0f;

    public override void OnEnter()
    {
        deer = obj.GetComponent<NewDeer>();
        state = deer.BaseStatus;

        elapsedTime = 0f;
        time = Random.Range(state.MinStateTime, state.MaxStateTime);

        deer.anim.SetBool(DeerAnimation.IsWalking, false);
        deer.anim.SetBool(DeerAnimation.IsRunning, false);
    }

    public override void OnExit()
    {

    }

    public override bool Update()
    {
        return false;
    }

    public override void OnFixedUpdate()
    {
        elapsedTime += Time.fixedDeltaTime;
        if ((elapsedTime >= time))
        {
            deer.SelectStateAndBehave();
        }
    }
}
