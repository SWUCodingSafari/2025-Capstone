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
    [SerializeField] public float addHungerBySec = 0.5f;

    [Header("Stamina")]
    [SerializeField] public float stamina = 10f;
    [SerializeField] public float maxStamina = 10f;
    [SerializeField] public float subStaminaByWalkSec = 0.5f;
    [SerializeField] public float subStaminaByRunSec = 1f;
    [SerializeField] public float subStaminaByMateSec = 0.8f;
    [SerializeField] public float addStaminaBySec = 1.5f;

    [Header("Mate")]
    [SerializeField] public float urgeToMate = 3f;
    [SerializeField] public float maxUrgeToMate = 3f;
    [SerializeField] public float subUrgeToMateAfterMate = 6f;
    [SerializeField] public float mateTime = 2f;
    [SerializeField] public float addUrgeToMateBySec = 0.3f;

    [Header("Basic State")]
    [SerializeField] public float viewDist = 5f;
    [SerializeField] public float moveSpeed = 1f;
    [SerializeField] public float runSpeed = 2f;
    [SerializeField] public float maxTurnAngleBySec = 15f;

    [Header("Boid")]
    [SerializeField] public float maxClusterDistance = 3f;
    [SerializeField] public float minClusterDistance = 1.5f;

    [SerializeField] public float loosePackTime = 5f;
    public WaitForSeconds wfLoosePack;
    public Coroutine coLoosePack = null;

    [Header("PointAdder")]
    [SerializeField] public float hungerToRestAdder = 3f;
    [SerializeField] public float eatAdder = 2f;
    [SerializeField] public float searchAdder = 6f;
    [SerializeField] public float heallingAdder = 5f;
    [SerializeField] public float mateAdder = 3f;
    [SerializeField] public float moveAdder = 7f;
    [SerializeField] public float hysteresisAdder = 2f;


    public virtual AnimalStatus GetNewStatus(AnimalStatus _that)
    {
        return this;
    }
}
