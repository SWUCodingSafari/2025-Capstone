using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonBehaviour<T>: MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = GameObject.FindObjectOfType<T>();
                if(_instance == null)
                {
                    GameObject obj = new GameObject();
                    _instance = obj.AddComponent<T>();
                }
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }

        Init();
    }

    protected virtual void Init()
    {

    }
}
