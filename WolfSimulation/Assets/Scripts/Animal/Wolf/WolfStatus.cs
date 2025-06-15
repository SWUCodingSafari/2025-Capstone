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
}
