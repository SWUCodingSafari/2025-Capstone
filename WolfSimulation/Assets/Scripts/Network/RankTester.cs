using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankTester : MonoBehaviour
{
    [SerializeField] private string username;
    [SerializeField] private string password;

    [ContextMenu("Register")]
    public void Register()
    {
        StartCoroutine(Start());
    }

    private IEnumerator Start()
    {
        // 2) 로그인 or 회원가입
        if (!NetworkManager.Instance.IsLoggedIn)
        {
            // 먼저 로그인 시도
            yield return NetworkManager.Instance.Login(username, password, (ok, msg) =>
            {
                Debug.Log($"Login => {ok}, {msg}");
            });

            // 없으면 회원가입 후 로그인 상태가 됨
            if (!NetworkManager.Instance.IsLoggedIn)
            {
                yield return NetworkManager.Instance.Register(username, password, (ok, msg) =>
                {
                    Debug.Log($"Register => {ok}, {msg}");
                });
            }
        }
        Debug.Log($"Logged in as {NetworkManager.Instance.CurrentUsername} (uid={NetworkManager.Instance.CurrentUid})");

        // 3) 점수 제출(초원/폭우/설산 중 하나)
        var stats = new Dictionary<string, float>
        {
            { "health", 7 },
            { "attack", 3 },
            { "speed", 5 }
        };
        yield return NetworkManager.Instance.Submit(NetworkManager.GameMap.plain, score: 2530, stats, (ok, msg, updated) =>
        {
            Debug.Log($"Submit => {ok}, updated:{updated}, msg:{msg}");
        });

        // 4) Top10 조회
        yield return NetworkManager.Instance.GetTop(NetworkManager.GameMap.plain, 10, (ok, msg, rows) =>
        {
            if (!ok) { Debug.LogError(msg); return; }
            Debug.Log("=== TOP (plain) ===");
            int i = 1;
            foreach (var r in rows) Debug.Log($"{i++}. {r.username}  {r.best_score}");
        });

        // 5) 내 순위
        yield return NetworkManager.Instance.GetMyRank(NetworkManager.GameMap.plain, (ok, msg, rank, score) =>
        {
            if (!ok) { Debug.LogError(msg); return; }
            Debug.Log($"MyRank: {(rank.HasValue ? rank.Value.ToString() : "None")} (score={score})");
        });

        // 6) 내 최고 기록
        yield return NetworkManager.Instance.GetMyBest(NetworkManager.GameMap.plain, (ok, msg, best) =>
        {
            if (!ok) { Debug.LogError(msg); return; }
            if (best == null) Debug.Log("No best yet.");
            else Debug.Log($"Best: {best.map}, score={best.score}, stats={best.stats?.Count}, at={UnixToKst(best.updated_at)}");
        });
    }
du
    private static string UnixToKst(long ms)
    {
        var epoch = System.DateTimeOffset.FromUnixTimeMilliseconds(ms).ToLocalTime();
        return epoch.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
