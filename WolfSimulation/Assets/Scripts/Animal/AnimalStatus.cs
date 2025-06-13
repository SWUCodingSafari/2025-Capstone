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

    [Header("Fear")]
    [SerializeField] public float fear = 0f;
    [SerializeField] public float maxFear = 10f;
    [SerializeField] public float subfearBySec = 1f;

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


    /*public virtual AnimalStatus GetNewStatus(AnimalStatus _that)
    {
        // todo: 유전 알고리즘
        return this;
    }*/

    public virtual AnimalStatus GetNewStatus(AnimalStatus _that)
    {
        AnimalStatus offspring = gameObject.AddComponent<AnimalStatus>(); // 새로운 개체 생성

        float mutationRate = 0.1f; // 10% 확률로 돌연변이 발생
        System.Func<float, float, float> crossover = (a, b) =>
        {
            float value = UnityEngine.Random.value < 0.5f ? a : b; // 단순 교차
            if (UnityEngine.Random.value < mutationRate)
            {
                float mutationAmount = UnityEngine.Random.Range(-0.2f, 0.2f); // 돌연변이 범위
                value += mutationAmount;
                value = Mathf.Max(0f, value); // 음수 방지
            }
            return value;
        };

        // [Health]
        offspring.health = crossover(this.health, _that.health);
        offspring.maxHealth = crossover(this.maxHealth, _that.maxHealth);
        offspring.reduceHealthBySec = crossover(this.reduceHealthBySec, _that.reduceHealthBySec);
        offspring.addHealthByHealingSec = crossover(this.addHealthByHealingSec, _that.addHealthByHealingSec);

        // [Hunger]
        offspring.hunger = crossover(this.hunger, _that.hunger);
        offspring.maxHunger = crossover(this.maxHunger, _that.maxHunger);
        offspring.subHungerWhenEat = crossover(this.subHungerWhenEat, _that.subHungerWhenEat);
        offspring.addHungerBySec = crossover(this.addHungerBySec, _that.addHungerBySec);
        offspring.hungerCurve = this.hungerCurve; // 커브는 그대로 상속 (혹은 무작위 선택 가능)

        // [Stamina]
        offspring.stamina = crossover(this.stamina, _that.stamina);
        offspring.maxStamina = crossover(this.maxStamina, _that.maxStamina);
        offspring.subStaminaByWalkSec = crossover(this.subStaminaByWalkSec, _that.subStaminaByWalkSec);
        offspring.subStaminaByRunSec = crossover(this.subStaminaByRunSec, _that.subStaminaByRunSec);
        offspring.subStaminaByMateSec = crossover(this.subStaminaByMateSec, _that.subStaminaByMateSec);
        offspring.addStaminaBySec = crossover(this.addStaminaBySec, _that.addStaminaBySec);

        // [Fear]
        offspring.fear = crossover(this.fear, _that.fear);
        offspring.maxFear = crossover(this.maxFear, _that.maxFear);
        offspring.subfearBySec = crossover(this.subfearBySec, _that.subfearBySec);

        // [Mate]
        offspring.urgeToMate = crossover(this.urgeToMate, _that.urgeToMate);
        offspring.maxUrgeToMate = crossover(this.maxUrgeToMate, _that.maxUrgeToMate);
        offspring.subUrgeToMateAfterMate = crossover(this.subUrgeToMateAfterMate, _that.subUrgeToMateAfterMate);
        offspring.mateTime = crossover(this.mateTime, _that.mateTime);
        offspring.addUrgeToMateBySec = crossover(this.addUrgeToMateBySec, _that.addUrgeToMateBySec);

        // [Basic State]
        offspring.viewDist = crossover(this.viewDist, _that.viewDist);
        offspring.moveSpeed = crossover(this.moveSpeed, _that.moveSpeed);
        offspring.runSpeed = crossover(this.runSpeed, _that.runSpeed);
        offspring.maxTurnAngleBySec = crossover(this.maxTurnAngleBySec, _that.maxTurnAngleBySec);

        // [Boid]
        offspring.maxClusterDistance = crossover(this.maxClusterDistance, _that.maxClusterDistance);
        offspring.minClusterDistance = crossover(this.minClusterDistance, _that.minClusterDistance);
        offspring.loosePackTime = crossover(this.loosePackTime, _that.loosePackTime);
        offspring.wfLoosePack = new WaitForSeconds(offspring.loosePackTime);

        // [PointAdder]
        offspring.hungerToRestAdder = crossover(this.hungerToRestAdder, _that.hungerToRestAdder);
        offspring.eatAdder = crossover(this.eatAdder, _that.eatAdder);
        offspring.searchAdder = crossover(this.searchAdder, _that.searchAdder);
        offspring.heallingAdder = crossover(this.heallingAdder, _that.heallingAdder);
        offspring.mateAdder = crossover(this.mateAdder, _that.mateAdder);
        offspring.moveAdder = crossover(this.moveAdder, _that.moveAdder);
        offspring.hysteresisAdder = crossover(this.hysteresisAdder, _that.hysteresisAdder);

        return offspring;
    }
}
