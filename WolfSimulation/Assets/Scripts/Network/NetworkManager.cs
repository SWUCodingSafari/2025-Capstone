using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : SingletonBehaviour<NetworkManager>
{
    [Header("Server")]
    public string baseURL = "https://wolfsimulationserver-production-9866.up.railway.app";

    [Header("Auth")]
    [SerializeField] private string jwtToken;
    [SerializeField] private string userName;
    [SerializeField] private int uId;

    private const string PREF_JWT = "RankJWT";
    private const string PREF_USERNAME = "RankUsername";
    private const string PREF_UID = "RankUid";

    public enum GameMap
    {
        plain,
        rain,
        snow,
        MAX
    }

    #region DTOs
    [Serializable]
    private class RegisterLoginReq
    {
        public string username;
        public string password;
    }

    [Serializable]
    private class RegisterLoginRes
    {
        public string token;
        public int uid;
        public string username;
    }

    [Serializable]
    private class SubmitReq
    {
        public string map;
        public int score;
        public WolfState stats;
    }

    [Serializable]
    private class OkUpdatedRes
    {
        public bool ok;
        public bool updated;
    }

    [Serializable]
    public class TopRow
    {
        public string username;
        public int best_score;
    }

    [Serializable]
    private class MyRankRes
    {
        public string map;
        public int? rank;
        public int? score;
    }

    [Serializable]
    public class MyBestRes
    {
        [Serializable]
        public class Best
        {
            public string map;
            public int score;
            public Dictionary<string , float> stats;
            public long updated_at;
        }

        public Best best;
    }
    #endregion

    protected override void Init()
    {
        base.Init();

        jwtToken = PlayerPrefs.GetString(PREF_JWT, "");
        userName = PlayerPrefs.GetString(PREF_USERNAME, "");
        uId = PlayerPrefs.GetInt(PREF_UID, 0);
    }

    public bool IsLoggedIn => !string.IsNullOrEmpty(jwtToken);
    public string CurrentUsername => userName;
    public int CurrentUid => uId;

    #region Auth
    public IEnumerator Register(string id, string pw, Action<bool, string> done)
    {
        var body = new RegisterLoginReq { username = id, password = pw };
        yield return Send("/auth/register", "POST", body, null, (succ, txt, code) =>
        {
            if (!succ) { done?.Invoke(false, $"Register failed [{code}]: {txt}"); return; }
            var res = JsonConvert.DeserializeObject<RegisterLoginRes>(txt);
            SaveAuth(res);
            done?.Invoke(true, "ok");
        });
    }

    public IEnumerator Login(string id, string pw, Action<bool, string> done)
    {
        var body = new RegisterLoginReq { username = id, password = pw };
        yield return Send("/auth/login", "POST", body, null, (succ, txt, code) =>
        {
            if (!succ) { done?.Invoke(false, $"Login failed [{code}]: {txt}"); return; }
            var res = JsonConvert.DeserializeObject<RegisterLoginRes>(txt);
            SaveAuth(res);
            done?.Invoke(true, "ok");
        });
    }

    public void Logout()
    {
        jwtToken = "";
        userName = "";
        uId = 0;
        PlayerPrefs.DeleteKey(PREF_JWT);
        PlayerPrefs.DeleteKey(PREF_USERNAME);
        PlayerPrefs.DeleteKey(PREF_UID);
        PlayerPrefs.Save();
    }

    private void SaveAuth(RegisterLoginRes res)
    {
        jwtToken = res.token;
        userName = res.username;
        uId = res.uid;
        PlayerPrefs.SetString(PREF_JWT, jwtToken);
        PlayerPrefs.SetString(PREF_USERNAME, userName);
        PlayerPrefs.SetInt(PREF_UID, uId);
        PlayerPrefs.Save();
    }
    #endregion

    #region API
    
    public IEnumerator Submit(GameMap map, int score, WolfState stats, Action<bool, string, bool> done)
    {
        if (!IsLoggedIn) { done?.Invoke(false, "not logged in", false); yield break; }
        if (stats == null) stats = new WolfState();

        var body = new SubmitReq { map = map.ToString(), score = score, stats = stats };
        yield return Send("/submit", "POST", body, jwtToken, (succ, txt, code) =>
        {
            if (!succ) { done?.Invoke(false, $"Submit failed [{code}]: {txt}", false); return; }
            var res = JsonConvert.DeserializeObject<OkUpdatedRes>(txt);
            done?.Invoke(true, "ok", res.updated);
        });
    }

    public IEnumerator GetTop(GameMap map, int limit, Action<bool, string, List<TopRow>> done)
    {
        string path = $"/top?map={map.ToString()}&limit={Mathf.Clamp(limit, 1, 100)}";
        yield return Send(path, "GET", null, null, (succ, txt, code) =>
        {
            if (!succ) { done?.Invoke(false, $"Top failed [{code}]: {txt}", null); return; }
            var rows = JsonConvert.DeserializeObject<List<TopRow>>(txt);
            done?.Invoke(true, "ok", rows);
        });
    }

    public IEnumerator GetMyRank(GameMap map, Action<bool, string, int?, int?> done)
    {
        if (!IsLoggedIn) { done?.Invoke(false, "not logged in", null, null); yield break; }
        string path = $"/myrank?map={map.ToString()}";
        yield return Send(path, "GET", null, jwtToken, (succ, txt, code) =>
        {
            if (!succ) { done?.Invoke(false, $"MyRank failed [{code}]: {txt}", null, null); return; }
            var res = JsonConvert.DeserializeObject<MyRankRes>(txt);
            done?.Invoke(true, "ok", res.rank, res.score);
        });
    }

    public IEnumerator GetMyBest(GameMap map, Action<bool, string, MyBestRes.Best> done)
    {
        if (!IsLoggedIn) { done?.Invoke(false, "not logged in", null); yield break; }
        string path = $"/me/best?map={map.ToString()}";
        yield return Send(path, "GET", null, jwtToken, (succ, txt, code) =>
        {
            if (!succ) { done?.Invoke(false, $"MyBest failed [{code}]: {txt}", null); return; }
            var res = JsonConvert.DeserializeObject<MyBestRes>(txt);
            done?.Invoke(true, "ok", res.best); // 없으면 null
        });
    }
    #endregion

    #region Core Send
    private IEnumerator Send(string path, string method, object bodyObj, string bearer, Action<bool, string, long> done)
    {
        if (string.IsNullOrEmpty(baseURL))
        {
            done?.Invoke(false, "Base URL empty", 0);
            yield break;
        }
        var url = $"{baseURL.TrimEnd('/')}{path}";

        UnityWebRequest req;
        if (method == "GET")
        {
            req = UnityWebRequest.Get(url);
        }
        else
        {
            string json = bodyObj != null ? JsonConvert.SerializeObject(bodyObj) : "{}";
            var bytes = Encoding.UTF8.GetBytes(json);
            req = new UnityWebRequest(url, method);
            req.uploadHandler = new UploadHandlerRaw(bytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
        }

        if (!string.IsNullOrEmpty(bearer))
            req.SetRequestHeader("Authorization", $"Bearer {bearer}");

        // 서버는 helmet + cors 켜져 있음
        yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
        bool ok = (req.result == UnityWebRequest.Result.Success) && (req.responseCode >= 200 && req.responseCode < 300);
#else
        bool ok = !req.isNetworkError && !req.isHttpError && (req.responseCode >= 200 && req.responseCode < 300);
#endif
        string txt = req.downloadHandler != null ? req.downloadHandler.text : "";
        long code = req.responseCode;

        done?.Invoke(ok, ok ? txt : (string.IsNullOrEmpty(txt) ? req.error : txt), code);
        req.Dispose();
    }
    #endregion
}
