using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static NetworkManager;

public class NetworkConnecter : MonoBehaviour
{
    private bool isLoginWaiting;
    private bool isRegisterWaiting;
    private bool isSubMitWaiting;
    private bool isGetTopWating;
    private bool isGetMyRankWaiting;
    private bool isGetMyBestWaiting;

    public void Login(string username, string password, Action<bool, string> callback)
    {
        if (isLoginWaiting)
            return;

        if ((callback == null))
        {
            Debug.LogError("No Callback");
            return;
        }
        StartCoroutine(CoLogin(username, password, callback));
    }
    private IEnumerator CoLogin(string username, string password, Action<bool, string> callback)
    {
        isLoginWaiting = true;
        yield return NetworkManager.Instance.Login(username, password, callback);
        isLoginWaiting = false;
    }

    public void Register(string username, string password, Action<bool, string> callback)
    {
        if (isRegisterWaiting)
        {
            return;
        }

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
        isRegisterWaiting = true;
        yield return NetworkManager.Instance.Register(username, password, callback);
        isRegisterWaiting = false;
    }

    public void Submit(GameMap map, int score, WolfState stats, Action<bool, string, bool> callback)
    {
        if (isSubMitWaiting)
            return;

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
        isSubMitWaiting = true;
        yield return NetworkManager.Instance.Submit(map, score, stats, done);
        isSubMitWaiting = false;
    }

    public void  GetTop(GameMap map, int limit, Action<bool, string, List<TopRow>> done)
    {
        if (isGetTopWating)
            return;

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
        isGetTopWating = true;
        yield return NetworkManager.Instance.GetTop(map, limit, done);
        isGetTopWating = false;
    }


    public void GetMyRank(GameMap map, Action<bool, string, int?, int?> done)
    {
        if(isGetMyRankWaiting)
        {
            return;
        }

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
        isGetMyRankWaiting = true;
        yield return NetworkManager.Instance.GetMyRank(map, done);
        isGetMyRankWaiting = false;
    }

    public void GetMyBest(GameMap map, Action<bool, string, MyBestRes.Best> done)
    {
        if (isGetMyBestWaiting)
            return;

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
        isGetMyBestWaiting = true;
        yield return NetworkManager.Instance.GetMyBest(map, done);
        isGetMyBestWaiting = false;
    }
}
