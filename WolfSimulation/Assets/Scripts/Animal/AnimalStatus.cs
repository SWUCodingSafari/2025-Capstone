using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class AnimalStatus : MonoBehaviour
{
    [Header("Point")]
    [Header("Health")]
    [SerializeField] public float health = 20f;
    [SerializeField] public float maxHealth = 20f;
    [SerializeField] public float reduceHealthBySec = 1f;
    [SerializeField] public float addHealthByHealingSec = 2f;

    [Header("Hunger")]
    [SerializeField] public float hunger = 10f;
    [SerializeField] public float maxHunger = 10f;
    [SerializeField] public float subHungerWhenEat = 3f;
    [SerializeField] public AnimationCurve hungerCurve;
    [SerializeField] public float addHungerBySec = 0.5f; // 
    [SerializeField] public float addHungerByheallingSec = 1f;

    [Header("Stamina")]
    [SerializeField] public float stamina = 10f;
    [SerializeField] public float maxStamina = 10f;
    [SerializeField] public float subStaminaByWalkSec = 0.5f;
    [SerializeField] public float subStaminaByRunSec = 1f;
    [SerializeField] public float subStaminaByMateSec = 0.8f;
    [SerializeField] public float addStaminaBySec = 1.5f;

    [Header("Fear")]
    [SerializeField] public float fear = 0f;
    [SerializeField] public float maxFear = 10f;
    [SerializeField] public float subfearBySec = 1f; //

    [Header("Mate")]
    [SerializeField] public float urgeToMate = 3f;
    [SerializeField] public float maxUrgeToMate = 3f;
    [SerializeField] public float subUrgeToMateAfterMate = 6f; //
    [SerializeField] public float mateTime = 2f;
    [SerializeField] public float addUrgeToMateBySec = 0.3f;

    [Header("Boid")]
    [SerializeField] public float maxClusterDistance = 3f;
    [SerializeField] public float minClusterDistance = 1.5f;

    [SerializeField] public float loosePackTime = 5f;
    public WaitForSeconds wfLoosePack;
    public Coroutine coLoosePack = null;

    [Header("Basic State")]
    [SerializeField] public float viewDist = 5f; //
    [SerializeField] public float moveSpeed = 1f; //
    [SerializeField] public float runSpeed = 2f; //
    [SerializeField] public float maxRunSpeed = 7f; // 
    [SerializeField] public float maxTurnAngleBySec = 15f; //

    [Header("Position")]
    [SerializeField] public float independence = 0.5f; //

    [Header("PointAdder")]
    [SerializeField] public float hungerToRestAdder = 3f; //
    [SerializeField] public float eatAdder = 2f; //
    [SerializeField] public float searchAdder = 6f; //
    [SerializeField] public float heallingAdder = 5f; //
    [SerializeField] public float mateAdder = 3f; //
    [SerializeField] public float moveAdder = 7f; //
    [SerializeField] public float hysteresisAdder = 2f; //

    [Header("GA")]
    [SerializeField] public float settingOffset = 0.3f;
    [SerializeField] public float mutationRate = 0.05f;
    [SerializeField] public float mutationValue = 0.05f;

    public DNAFactors dna;

    public struct DNAFactors
    {
        public enum Factors
        {
            addHungerBySec,
            subfearBySec,
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
            hysteresisAdder,
            MAX,

            attackRange = MAX,
            chaseAdder,
            attackAdder,
            WolfMAX
        }

        public float[] factors;

        //public float addHungerBySec;
        //public float subfearBySec;
        //public float subUrgeToMateAfterMate;
        //public float viewDist;
        //public float moveSpeed;
        //public float runSpeed;
        //public float maxRunSpeed;
        //public float maxTurnAngleBySec;
        //public float independence;
        //public float hungerToRestAdder;
        //public float eatAdder;
        //public float searchAdder;
        //public float heallingAdder;
        //public float mateAdder;
        //public float moveAdder;
        //public float hysteresisAdder;

        public DNAFactors(float[] factors)
        {
            this.factors = factors;
        }

        public DNAFactors(
            float addHungerBySec,
            float subfearBySec,
            float subUrgeToMateAfterMate,
            float viewDist,
            float moveSpeed,
            float runSpeed,
            float maxRunSpeed,
            float maxTurnAngleBySec,
            float independence,
            float hungerToRestAdder,
            float eatAdder,
            float searchAdder,
            float heallingAdder,
            float mateAdder,
            float moveAdder,
            float hysteresisAdder,

            float attackRange = 0f,
            float chaseAdder = 0f,
            float attackAdder = 0f
            )
        {
            this.factors = new float[(int)Factors.MAX];

            this.factors[(int)Factors.addHungerBySec ] = addHungerBySec;
            this.factors[(int)Factors.subfearBySec ] = subfearBySec;
            this.factors[(int)Factors.subUrgeToMateAfterMate ] = subUrgeToMateAfterMate;
            this.factors[(int)Factors.viewDist ] = viewDist;
            this.factors[(int)Factors.moveSpeed ] = moveSpeed;
            this.factors[(int)Factors.runSpeed ] = runSpeed;
            this.factors[(int)Factors.maxRunSpeed ] = maxRunSpeed;
            this.factors[(int)Factors.maxTurnAngleBySec ] = maxTurnAngleBySec;
            this.factors[(int)Factors.independence ] = independence;
            this.factors[(int)Factors.hungerToRestAdder ] = hungerToRestAdder;
            this.factors[(int)Factors.eatAdder ] = eatAdder;
            this.factors[(int)Factors.searchAdder ] = searchAdder;
            this.factors[(int)Factors.heallingAdder ] = heallingAdder;
            this.factors[(int)Factors.mateAdder ] = mateAdder;
            this.factors[(int)Factors.moveAdder ] = moveAdder;
            this.factors[(int)Factors.hysteresisAdder] = hysteresisAdder;

            this.factors[(int)Factors.attackRange] = attackRange;
            this.factors[(int)Factors.chaseAdder] = chaseAdder;
            this.factors[(int)Factors.attackAdder] = attackAdder;
        }
    }

    public virtual void Awake()
    {
        dna = new DNAFactors(
            addHungerBySec,
            subfearBySec,
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
            hysteresisAdder
            );

        // RandomGeneration();
    }

    // 랜덤 유전자 생성
    public virtual void RandomGeneration()
    {
        int max = (int)DNAFactors.Factors.MAX;
        float[] factors = new float[max];

        for (int i = 0; i < max; ++i)
        {
            factors[i] = UnityEngine.Random.Range(
                dna.factors[i] * (1 - settingOffset),
                dna.factors[i] * (1 + settingOffset)
                );
        }
    }

    public AnimalStatus GetNewStatus(AnimalStatus _that, AnimalStatus _baby)
    {
        _baby = this;

        _baby.dna = GetParentHalf(this, _that);

        return _baby;
    }

    public virtual DNAFactors GetParentHalf(AnimalStatus _a, AnimalStatus _b)
    {
        int max = (int)DNAFactors.Factors.MAX;
        float[] factors = new float[max];

        for (int i = 0; i < max; ++i)
        {
            bool isMutate = (float)UnityEngine.Random.Range(0, 100) / 100 < mutationRate;
            if(isMutate)
            {
                factors[i] = GetMutationValue(factors[i]);
                continue;
            }

            int randomNum = UnityEngine.Random.Range(0, 10);

            if(randomNum < 5) factors[i] = _a.dna.factors[i];
            else factors[i] = _b.dna.factors[i];
        }

        return new DNAFactors(factors);
    }

    protected float GetMutationValue(float _value)
    {
        return UnityEngine.Random.Range(
                _value * (1 - mutationValue),
                _value * (1 + mutationValue)
                );
    }
}
