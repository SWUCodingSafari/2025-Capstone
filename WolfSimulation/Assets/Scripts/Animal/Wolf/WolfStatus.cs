using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfStatus : AnimalStatus
{
    [Header("Attack")]
    [SerializeField] public float damage = 1.5f;
    [SerializeField] public float attackRange = 1.5f; //
    [SerializeField] public float attackCoolTime = 1f;
    [SerializeField] public float slowAfterAttack = 0.7f;

    [Header("AttackAdder")]
    [SerializeField] public float chaseAdder = 3f; //
    [SerializeField] public float attackAdder = 2f; //


    public override void Awake()
    {
        dna = new DNAFactors(
            addHungerBySec,
            subfearBySec,
            subHungerWhenEat,
            subUrgeToMateAfterMate,
            viewDist,
            moveSpeed,
            runSpeed,
            maxRunSpeed,
            maxTurnAngleBySec,
            independence,
            hungerToRestAdder,
            eatAdder,
            searchAdder,
            heallingAdder,
            mateAdder,
            moveAdder,

            attackRange,
            chaseAdder,
            attackAdder
            );

    }

    public override void SetStatus(AnimalStatus status)
    {
        base.SetStatus(status);

        if (status is not WolfStatus)
            return;

        WolfStatus wStatus = status as WolfStatus;

        this.damage = wStatus.damage;
        this.attackRange = wStatus.attackRange;
        this.attackCoolTime = wStatus.attackCoolTime;
        this.slowAfterAttack = wStatus.slowAfterAttack;
        ;
        this.chaseAdder = wStatus.chaseAdder;
        this.attackAdder = wStatus.attackAdder;
    }

    public override DNAFactors GetParentHalf(DNAFactors _a, DNAFactors _b)
    {
        int max = (int)DNAFactors.Factors.WolfMAX;
        float[] factors = new float[max];

        for (int i = 0; i < max; ++i)
        {
            bool isMutate = (float)UnityEngine.Random.Range(0, 100) / 100 < mutationRate;
            if (isMutate)
            {
                factors[i] = GetMutationValue(factors[i]);
                continue;
            }

            int randomNum = UnityEngine.Random.Range(0, 10);

            if (randomNum < 5) factors[i] = _a.factors[i];
            else factors[i] = _b.factors[i];
        }

        return new DNAFactors(factors);
    }

    /// 1. 렌덤 초기값 => 10 마리?
    /// 2. 세대당 시뮬레이션 결과 값 필요 >> dna, 생존 시간 (늑대만)
    ///         >> 
}
