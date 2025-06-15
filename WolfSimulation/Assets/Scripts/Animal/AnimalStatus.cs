using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimalStatus :MonoBehaviour
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

    // 염색체 (유전 형질의 집합)
    public virtual void DNA(float addHungerBySec, float subfearBySec, float subUrgeToMateAfterMate,
        float viewDist, float moveSpeed, float runSpeed, float maxRunSpeed, float maxTurnAngleBySec,
        float independence, float hungerToRestAdder, float eatAdder, float searchAdder, float heallingAdder,
        float mateAdder, float moveAdder, float hysteresisAdder)
    {
        this.addHungerBySec = addHungerBySec;
        this.subfearBySec = subfearBySec;
        this.subUrgeToMateAfterMate = subUrgeToMateAfterMate;
        this.viewDist = viewDist;
        this.moveSpeed = moveSpeed;
        this.runSpeed = runSpeed;
        this.maxRunSpeed = maxRunSpeed;
        this.maxTurnAngleBySec = maxTurnAngleBySec;
        this.independence = independence;
        this.hungerToRestAdder = hungerToRestAdder;
        this.eatAdder = eatAdder;
        this.searchAdder = searchAdder;
        this.heallingAdder = heallingAdder;
        this.mateAdder = mateAdder;
        this.moveAdder = moveAdder;
        this.hysteresisAdder = hysteresisAdder;
    }



    // 랜덤 유전자 생성
    /*public virtual AnimalStatus RandomGeneration()
    {
        return new AnimalStatus( // 수정 필요
            addHungerBySec = UnityEngine.Random.Range(0.2f, 1.0f),
            subfearBySec = UnityEngine.Random.Range(0.5f, 2.0f),
            subUrgeToMateAfterMate = UnityEngine.Random.Range(3f, 8f),
            viewDist = UnityEngine.Random.Range(3f, 10f),
            moveSpeed = UnityEngine.Random.Range(1f, 4f),
            runSpeed = UnityEngine.Random.Range(2f, 5f),
            maxRunSpeed = UnityEngine.Random.Range(5f, 10f),
            maxTurnAngleBySec = UnityEngine.Random.Range(10f, 45f),
            independence = UnityEngine.Random.Range(0.1f, 1.0f),
            hungerToRestAdder = UnityEngine.Random.Range(1f, 5f),
            eatAdder = UnityEngine.Random.Range(1f, 5f),
            searchAdder = UnityEngine.Random.Range(3f, 8f),
            heallingAdder = UnityEngine.Random.Range(3f, 8f),
            mateAdder = UnityEngine.Random.Range(1f, 5f),
            moveAdder = UnityEngine.Random.Range(5f, 10f),
            hysteresisAdder = UnityEngine.Random.Range(1f, 4f)
        );
    }*/

    public virtual AnimalStatus GetNewStatus(AnimalStatus _that, AnimalStatus _baby)
    {
        _baby = this;

        // 여기에 작성

        return _baby;
    }
}
