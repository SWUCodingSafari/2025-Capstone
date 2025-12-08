using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameManager : MonoBehaviour
{
    [SerializeField] private Text timeText;
    private int sec;
    private float time;

    [SerializeField] private GameObject InGameUI;
    [SerializeField] private GameObject GameOverUI;
    [SerializeField] private GameObject loadingUI;

    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Text errorText;
    [SerializeField] private GameObject myScorePanel;
    [SerializeField] private Text myScore;
    [SerializeField] private GameObject myRankPanel;
    [SerializeField] private Text myRank;
    [SerializeField] private GameObject bestScore;

    [SerializeField] private NetworkConnecter connecter;
    [SerializeField] private NetworkManager.GameMap curMap;
    private WolfState wolfState;

    private int RetryCount = 5;
    private int curScore = 0;

    private int PreSceneNumber = 0;

    private void Awake()
    {
        sec = 0;
        RetryCount = 5;
        GameManager.Instance.OnGameOver.AddListener(SendRecordToServer);

        InGameUI.SetActive(true);
        GameOverUI.SetActive(false);
    }

    private void FixedUpdate()
    {
        time += Time.fixedDeltaTime;

    }

    private void LateUpdate()
    {
        if (GameManager.Instance.IsGameOver)
            return;

        int newSec = Mathf.CeilToInt(time);
        if (newSec > sec)
            timeText.text = $"{newSec}";

        sec = newSec;
    }

    private void SendRecordToServer()
    {
        if(RetryCount < 0)
        {
            Debug.LogError("Sumit Fail");
            return;
        }

        GameOverUI.SetActive(true);
        loadingUI.SetActive(true);
        resultPanel.SetActive(false);

        curScore = Mathf.FloorToInt(time);
        myScore.text = curScore.ToString();
        
        connecter.Submit(curMap, curScore, wolfState, OnSubmit);
    }

    private void OnSubmit(bool ok, string msg, bool code)
    {
        resultPanel.SetActive(true);

        errorText.gameObject.SetActive(ok == false);
        myScorePanel.SetActive(ok);
        myRankPanel.SetActive(false);


        if (ok == false)
        {
            SendRecordToServer();
        }


        connecter.GetMyRank(curMap, OnRankGet);
    }

    private void OnRankGet(bool succ, string txt, int? rank, int? score)
    {

        if (succ == false)
            return;

        if ((rank.HasValue == false) || score.HasValue == false)
        {
            return;
        }

        myRankPanel.SetActive(succ);
        myRank.text = rank.ToString();
        bestScore.SetActive(score == curScore);
    }

    public void ReturnToMain()
    {
        SceneManager.LoadScene(PreSceneNumber);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameOver.RemoveListener(SendRecordToServer);
    }
}
