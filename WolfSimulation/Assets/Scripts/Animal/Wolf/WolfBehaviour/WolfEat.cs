using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfEat : AnimalStateBehaviour
{
    private NewDeer targetDeer = null;

    private float elapsedTime = 0f;
    private float eatTime = 1f;
    private bool isSearching = true;

    public override void OnEnter()
    {
        OnReset();

        animal.anim.SetBool(DeerAnimation.IsWalking, true);
    }

    public override void OnReset()
    {
        if (targetDeer != null)
            return;

        targetDeer = animal.DeerList.Find(_ => _.IsDied == true);
        if (targetDeer == null)
        {
            animal.OnEnviromentChanged();
            return;
        }

        elapsedTime = 0f;
        isSearching = true;
    }

    public override void OnExit()
    {
        targetDeer = null;

        animal.anim.SetBool(DeerAnimation.IsEating, false);
        animal.anim.SetBool(DeerAnimation.IsWalking, false);
    }

    public override bool Update()
    {
        if (targetDeer == null)
            return true;

        Vector3 subVec = targetDeer.transform.position - animal.transform.position;
        subVec.y = 0f;
        
        if (subVec.sqrMagnitude <= 3f)
        {
            if (isSearching == true)
            {
                animal.anim.SetBool(DeerAnimation.IsWalking, false);
                animal.anim.SetBool(DeerAnimation.IsEating, true);
                isSearching = false;

                return false;
            }

            elapsedTime += Time.fixedDeltaTime;
            if (elapsedTime < eatTime) return false;

            elapsedTime -= eatTime;

            float meat = targetDeer.Eaten();
            if(meat == -1f)
            {
                animal.DeerList.Remove(targetDeer);
                targetDeer = null;
            }
            animal.BaseStatus.hunger = Mathf.Clamp(
                animal.BaseStatus.hunger - animal.BaseStatus.subHungerWhenEat * meat, 0, animal.BaseStatus.maxHunger);
            return true;
        }

        animal.TurnToDesiredDir(subVec.normalized);
        animal.MovePosition();

        return false;
    }
}
