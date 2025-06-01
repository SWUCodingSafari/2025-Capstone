using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

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
        if (TGrass == null)
            return true;

        Vector3 subVec = TGrass.transform.position - animal.transform.position;
        subVec.y = 0f;
        animal.TurnToDesiredDir(subVec.normalized);
        animal.MovePosition();

        if (subVec.sqrMagnitude <= 0.5f)
        {
            animal.GrassList.Remove(TGrass);
            TGrass.OnMouseDown();
            animal.BaseStatus.hunger = Mathf.Clamp(
                animal.BaseStatus.hunger - animal.BaseStatus.subHungerWhenEat, 0, animal.BaseStatus.maxHunger);
            return true;
        }
        return false;
    }
}
