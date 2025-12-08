using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WolfState : MonoBehaviour
{
    public const int MAX_State = 10;

    public int Health = 0;
    public int Speed = 0;
    public int AttackDamage = 0;
    public int ScentSensitivity = 0;
    public int HungerSensitivity = 0;
}
