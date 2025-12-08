using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static NetworkManager;

public class NetworkConnecter : MonoBehaviour
{
    public void Login(string username, string password, Action<bool, string> callback)
    {
        if ((callback == null))
        {
            Debug.LogError("No Callback");
            return;
        }
        StartCoroutine(CoLogin(username, password, callback));
    }

    private IEnumerator CoLogin(string username, string password, Action<bool, string> callback)
    {
        yield return NetworkManager.Instance.Login(username, password, callback);
    }

    public void Register(string username, string password, Action<bool, string> callback)
    {
        if ((callback == null))
        {
            Debug.LogError("No Callback");
            return;
        }

        if(NetworkManager.Instance.IsLoggedIn == true)
        {
            Debug.LogError("IsLoggedIn");
            return;
        }
        StartCoroutine (CoRegister(username, password, callback));
    }
    private IEnumerator CoRegister(string username, string password, Action<bool, string> callback)
    {
        yield return NetworkManager.Instance.Register(username, password, callback);
    }

    public void Submit(GameMap map, int score, WolfState stats, Action<bool, string, bool> callback)
    {
        if ((callback == null))
        {
            Debug.LogError("No Callback");
            return;
        }

        if (NetworkManager.Instance.IsLoggedIn == false)
        {
            Debug.LogError("Need Login");
            return;
        }

        StartCoroutine(CoSubmit(map, score, stats, callback));
    }

    private IEnumerator CoSubmit(GameMap map, int score, WolfState stats, Action<bool, string, bool> done)
    {
        yield return NetworkManager.Instance.Submit(NetworkManager.GameMap.plain, score: 2530, stats, done);
    }

    public void  GetTop(GameMap map, int limit, Action<bool, string, List<TopRow>> done)
    {
        if ((done == null))
        {
            Debug.LogError("No Callback");
            return;
        }

        if (NetworkManager.Instance.IsLoggedIn == false)
        {
            Debug.LogError("Need Login");
            return;
        }

        StartCoroutine(CoGetTop(map, limit, done));
    }

    private IEnumerator CoGetTop(GameMap map, int limit, Action<bool, string, List<TopRow>> done)
    {
        yield return NetworkManager.Instance.GetTop(NetworkManager.GameMap.plain, 10, done);
    }


    public void GetMyRank(GameMap map, Action<bool, string, int?, int?> done)
    {
        if ((done == null))
        {
            Debug.LogError("No Callback");
            return;
        }

        if (NetworkManager.Instance.IsLoggedIn == false)
        {
            Debug.LogError("Need Login");
            return;
        }

        StartCoroutine(CoGetMyRank(map, done));
    }

    private IEnumerator CoGetMyRank(GameMap map, Action<bool, string, int?, int?> done)
    {
        yield return NetworkManager.Instance.GetMyRank(NetworkManager.GameMap.plain, done);
    }

    public void GetMyBest(GameMap map, Action<bool, string, MyBestRes.Best> done)
    {
        if ((done == null))
        {
            Debug.LogError("No Callback");
            return;
        }

        if (NetworkManager.Instance.IsLoggedIn == false)
        {
            Debug.LogError("Need Login");
            return;
        }

        StartCoroutine(CoGetMyBest(map, done));
    }

    private IEnumerator CoGetMyBest(GameMap map, Action<bool, string, MyBestRes.Best> done)
    {
        yield return NetworkManager.Instance.GetMyBest(NetworkManager.GameMap.plain, done);
    }
}
