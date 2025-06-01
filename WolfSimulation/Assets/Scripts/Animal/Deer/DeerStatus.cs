using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerStatus : AnimalStatus
{
    [Header("Attack")]
    [SerializeField] public float damage = 1.5f;
    [SerializeField] public float attackRange = 1.5f;
    [SerializeField] public float attackCoolTime = 1f;
}
