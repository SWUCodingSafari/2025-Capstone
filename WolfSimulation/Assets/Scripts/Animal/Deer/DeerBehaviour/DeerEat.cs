using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DeerEat : AnimalStateBehaviour
{

    private Grass targetGrass = null;
    public Grass TGrass
    {
        get => targetGrass;
        set
        {
            targetGrass = value;
            if (value == null)
            {
                Debug.Log("³Î¶ä");
            }
        }
    }

    private float elapsedTime = 0f;
    private float eatTime = 0.471f;
    private bool isSearching = true;

    public override void OnEnter()
    {
        OnReset();
    }

    public override void OnReset()
    {
        if (TGrass != null)
            return;

        TGrass = animal.GrassList.Find(_ => _.IsGrown && _.reservedBy == null);
        if (TGrass == null)
        {
            animal.OnEnviromentChanged();
            return;
        }

        TGrass.reservedBy = animal.gameObject;
        elapsedTime = 0f;
        eatTime = -1f;
        isSearching = true;

        animal.anim.SetBool(DeerAnimation.IsWalking, true);
    }

    public override void OnExit()
    {
        Debug.Log("Eat Exit");
        if (TGrass == null)
            return;

        TGrass.reservedBy = null;
        TGrass = null;

        animal.anim.SetBool(DeerAnimation.IsWalking, false);
    }

    public override bool Update()
    {
        if (TGrass == null)
            return true;

        Vector3 subVec = TGrass.transform.position - animal.transform.position;
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

            animal.GrassList.Remove(TGrass);
            TGrass.Eaten();
            TGrass = null;
            animal.BaseStatus.hunger = Mathf.Clamp(
                animal.BaseStatus.hunger - animal.BaseStatus.subHungerWhenEat, 0, animal.BaseStatus.maxHunger);
            return true;
        }

        animal.TurnToDesiredDir(subVec.normalized);
        animal.MovePosition();

        return false;
    }
}
