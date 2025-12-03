using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NDeerEat : AnimalStateBehaviour
{
    NewDeer deer;
    NewDeerState state;

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
        deer = obj.GetComponent<NewDeer>();
        state = deer.BaseStatus;

        OnReset();
    }

    public override void OnReset()
    {
        if (TGrass != null)
            return;

        foreach (var grass in deer.GrassList)
        {
            if (grass.reservedBy == deer.gameObject || (grass.IsGrown == true && grass.reservedBy == null))
            {
                TGrass = grass;
                break;
            }
        }

        TGrass = deer.GrassList.Find(_ => _.IsGrown && _.reservedBy == null);
        if (TGrass == null)
        {
            deer.SelectStateAndBehave((int)NewDeer.NDeerState.Search);
            return;
        }

        TGrass.reservedBy = deer.gameObject;
        elapsedTime = 0f;
        eatTime = -1f;
        isSearching = true;

        deer.anim.SetBool(DeerAnimation.IsWalking, true);
    }

    public override void OnExit()
    {
        Debug.Log("Eat Exit");
        if (TGrass == null)
            return;

        TGrass.reservedBy = null;
        TGrass = null;


    }

    public override bool Update()
    {
        return false;
    }

    public override void OnFixedUpdate()
    {
        if (TGrass == null)
        {
            deer.SelectStateAndBehave();
            return;
        }

        Vector3 subVec = TGrass.transform.position - deer.transform.position;
        subVec.y = 0f;
        
        if (subVec.sqrMagnitude <= 1f)
        {
            if (isSearching == true)
            {
                deer.anim.SetBool(DeerAnimation.IsWalking, false);
                deer.anim.SetBool(DeerAnimation.IsEating, true);
                isSearching = false;

                return ;
            }

            elapsedTime += Time.deltaTime;
            if (elapsedTime < eatTime) return ;

            deer.anim.SetBool(DeerAnimation.IsEating, false);

            deer.GrassList.Remove(TGrass);
            TGrass.Eaten();
            
            deer.SelectStateAndBehave();
            return ;
        }

        deer.TurnToDesiredDir(subVec.normalized);
        deer.MovePosition();
    }
}
