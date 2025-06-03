using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfEat : AnimalStateBehaviour
{
    private Deer targetDeer = null;

    private float elapsedTime = 0f;
    private float eatTime = 0.471f;
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

        targetDeer = animal.DeerList.Find(_ => _.IsDied );
        if (targetDeer == null)
        {
            animal.OnEnviromentChanged();
            return;
        }

        elapsedTime = 0f;
        eatTime = -1f;
        isSearching = true;
    }

    public override void OnExit()
    {
        Debug.Log("Eat Exit");
        if (targetDeer == null)
            return;

        targetDeer = null;

        animal.anim.SetBool(DeerAnimation.IsWalking, false);
    }

    public override bool Update()
    {
        if (targetDeer == null)
            return true;

        Vector3 subVec = targetDeer.transform.position - animal.transform.position;
        subVec.y = 0f;
        
        if (subVec.sqrMagnitude <= 1f)
        {
            if (isSearching == true)
            {
                animal.anim.SetBool(DeerAnimation.IsWalking, false);
                animal.anim.SetBool(DeerAnimation.IsEating, true);
                isSearching = false;

                return false;
            }

            elapsedTime += Time.deltaTime;
            if (elapsedTime < eatTime) return false;

            animal.anim.SetBool(DeerAnimation.IsEating, false);

            animal.DeerList.Remove(targetDeer);
            targetDeer.GetEaten();
            targetDeer = null;
            animal.BaseStatus.hunger = Mathf.Clamp(
                animal.BaseStatus.hunger - animal.BaseStatus.subHungerWhenEat, 0, animal.BaseStatus.maxHunger);
            return true;
        }

        animal.TurnToDesiredDir(subVec.normalized);
        animal.MovePosition();

        return false;
    }
}
