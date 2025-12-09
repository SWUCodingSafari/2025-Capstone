using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WolfState : MonoBehaviour
{
    public enum StateType
    {
        health,
        speed,
        Sight,
        HungerSensitivity,
        ScentSensitivity,
        MAX
    }
    public const int MAX_State = 10;

    public int Health = 0;
    public int Speed = 0;
    public int Sight = 0;
    public int ScentSensitivity = 0;
    public int HungerSensitivity = 0;

    public Dictionary<string, float> GetDict()
    {
        var dict = new Dictionary<string, float>();

        dict[StateType.health.ToString()] = Health; 
        dict[StateType.speed.ToString()] = Speed;
        dict[StateType.Sight.ToString()] = Sight;
        dict[StateType.HungerSensitivity.ToString()] = HungerSensitivity;
        dict[StateType.ScentSensitivity.ToString()] = ScentSensitivity;

        return dict;
    }

    public void SetState(Dictionary<string, float> dict)
    {
        if (dict == null)
        {
            Health = Speed = Sight = HungerSensitivity = ScentSensitivity = 0;
            return;
        }

        Health = dict.ContainsKey(StateType.health.ToString()) ?
            Mathf.CeilToInt(dict[StateType.health.ToString()]) : 0;

        Speed = dict.ContainsKey(StateType.speed.ToString()) ? 
            Mathf.CeilToInt(dict[StateType.speed.ToString()]) : 0;

        Sight = dict.ContainsKey(StateType.Sight.ToString()) ? 
            Mathf.CeilToInt(dict[StateType.Sight.ToString()]) : 0;

        ScentSensitivity = dict.ContainsKey(StateType.ScentSensitivity.ToString()) ? 
            Mathf.CeilToInt(dict[StateType.ScentSensitivity.ToString()]) : 0;

        HungerSensitivity = dict.ContainsKey(StateType.HungerSensitivity.ToString()) ? 
            Mathf.CeilToInt(dict[StateType.HungerSensitivity.ToString()]) : 0;
    }
}
