using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewDeerState : MonoBehaviour
{
    [field: SerializeField] public float Health { get; set; } = 20f;
    [field: SerializeField] public float WalkSpeed { get; set; } = 1f;
    [field: SerializeField] public float MaxTurnAngleBySec { get; set; } = 15f;
    [field: SerializeField] public float RunSpeed { get; set; } = 2f;
    [field: SerializeField] public float ViewDist { get; set; } = 5f;

    [field: SerializeField] public float MaxClusterDistance { get; set; } = 3f;
    [field: SerializeField] public float MinClusterDistance { get; set; } = 1.5f;

    [field: SerializeField] public float LoosePackTime { get; set; } = 5f;

    [field: SerializeField] public float MaxStateTime { get; set; } = 3f;
    [field: SerializeField] public float MinStateTime { get; set; } = 1f;

    [field: SerializeField] public float scentStrength { get; set; } = 0.05f;
    [field: SerializeField] public int scentRadius { get; set; } = 5;

}
