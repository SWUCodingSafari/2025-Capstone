using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class WolfMate : AnimalStateBehaviour
{
    private float elapsedTime = 0f;

    public override void OnEnter()
    {
        animal.LookingForMate = false;
        animal.IsMating = true;

        elapsedTime = 0f;

        Vector3 mateDir = animal.MatePair.transform.position - animal.transform.position;
        animal.TurnToDesiredDir(mateDir);
        
        animal.anim.SetBool(WolfAnimation.IsWalking, true);
    }

    public override void OnExit()
    {
        animal.IsMating = false;
        animal.anim.SetBool(WolfAnimation.IsMating, false);
        animal.anim.SetBool(WolfAnimation.IsWalking, false);
    }

    public override float ReducedStamina()
    {
        return animal.BaseStatus.subStaminaByMateSec;
    }

    public override bool Update()
    {
        Vector3 vec = animal.MatePair.transform.position - animal.transform.position;
        vec.y = 0f;

        animal.TurnToDesiredDir(vec.normalized);

        if(vec.sqrMagnitude <= 1f)
        {
            if(elapsedTime <= 0f)
            {
                animal.anim.SetBool(WolfAnimation.IsMating, true);
                animal.anim.SetBool(WolfAnimation.IsWalking, false);
            }

            elapsedTime += Time.deltaTime;

            if (elapsedTime > animal.BaseStatus.mateTime)
            {
                animal.MateOver();

                return true;
            }

            return false;
        }

        animal.MovePosition();

        return false;
    }
}
