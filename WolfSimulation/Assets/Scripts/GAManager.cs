using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using DNA = AnimalStatus.DNAFactors;

public class GAManager : SingletonBehaviour<GAManager>
{
    public struct GAResults
    {
        public int num;
        public DNA dna;
        public float lifeTime;
    }

    [SerializeField]
    [Range(1f, 5f)]
    private float timeScale = 1f;
    [SerializeField] private WolfStatus initialStatus;
    private static int GenCount = 0;
    private int topWolfId, secondWolfId;
    private const int MAINSCENENUMBER = 1;

    [Header("Wolf")]
    [SerializeField] private Wolf[] wolfList;
    private GAResults[] wolfStatusList;
    private int totalWolfCount = 0;
    private int initialWolfCount = 0;

    [Header("Deer")]
    [SerializeField] private Deer[] deerList;

    private string logFilePath;


    protected override void Init()
    {
        base.Init();

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += WaitForSceneLoad;

        SceneManager.LoadScene(MAINSCENENUMBER);

    }

    private void SetFistSimulation()
    {
        wolfList = GameObject.FindObjectsOfType<Wolf>();
        //deerList = GameObject.FindObjectsOfType<Deer>();

        foreach (var item in wolfList)
        {
            item.BaseStatus.SetStatus(initialStatus);
            item.BaseStatus.dna = WolfStatus.RandomGeneration(initialStatus.dna);
            item.Init();
        }

        initialWolfCount = wolfList.Length;
        totalWolfCount = initialWolfCount;

        wolfStatusList = new GAResults[initialWolfCount];

        topWolfId = 0;
        secondWolfId = 0;

        ++GenCount;
    }

    private void SetNextSimulation()
    {
        ++GenCount;
        wolfList = GameObject.FindObjectsOfType<Wolf>();
        //deerList = GameObject.FindObjectsOfType<Deer>();

        foreach (var item in wolfList)
        {
            item.BaseStatus = initialStatus.GetNewStatus(initialStatus,
                wolfStatusList[topWolfId].dna, wolfStatusList[secondWolfId].dna, item.BaseStatus);
            item.Init();

        }

        topWolfId = 0;
        secondWolfId = 0;
    }

    private void WaitForSceneLoad(Scene arg0, LoadSceneMode arg1)
    {
        StartCoroutine(CoWaitForSetup(GenCount <= 0 ? SetFistSimulation: SetNextSimulation));
    }

    private IEnumerator CoWaitForSetup(UnityAction _func)
    {
        yield return null;
        _func?.Invoke();
    }

    public void AnimalDied(Animal _animal, float _lifeTime)
    {
        if (_animal is not Wolf)
            return;

        Wolf wolf = _animal as Wolf;

        for(int i = 0; i < initialWolfCount; ++i)
        {
            if (_animal != wolfList[i])
                continue;

            wolfStatusList[i].num = i;
            wolfStatusList[i].dna = wolf.BaseStatus.dna;
            wolfStatusList[i].lifeTime = _lifeTime;

            --totalWolfCount;
            if(totalWolfCount <= 0)
            {
                GetTopAndSecond(out topWolfId, out secondWolfId);
                RecordGenerationResult(topWolfId, secondWolfId);
                ResetSimulation();
            }
        }
    }

    private void GetTopAndSecond(out int top, out int second)
    {
        top = 0; second = 0;
        for(int i = 0; i < initialWolfCount; ++i)
        {
            if (wolfStatusList[i].lifeTime > wolfStatusList[top].lifeTime)
            {
                second = top;
                top = i;
            }
        }
    }

    private void RecordGenerationResult(int _fist, int _second)
    {
        MyLogger.Instance.WriteLog($"[============Generation {GenCount}===========]");
        MyLogger.Instance.WriteLog(string.Format(
            "{0,-8}: {1,10} | {2,10} | {3,10} | {4,10} | {5,10} | {6,10} | {7,10} | {8,10} | {9,10} | {10,10} | {11,10} | {12,10} | {13,10} | {14,10} | {15,10} | {16,10} | {17,10} | {18,10} | {19,10} | {20,10} |",
            "Name", "LIFE_TIME", "AHBS", "SFBS","SUTMAM", "VD", "MS", "RS", "MRS", "MTABS","INDPD", "HUNG2RAD","EATAD","SEARCHAD","HEALAD","MATEAD","MOVEAD","HYSAD", "ATKRNG","CASEAD","ATKAD"
            ));

        string txt = "";
        for (int i = 0; i < initialWolfCount; ++i)
        {
            txt += string.Format("{0,-8}: ", $"Wolf{i}");
            txt += string.Format("{0,10:F3} | ", wolfStatusList[i].lifeTime);
            txt += wolfStatusList[i].dna.ToString();

            MyLogger.Instance.WriteLog(txt);

            txt = "";
        }

        MyLogger.Instance.WriteLog("----------------------------------------------");
        MyLogger.Instance.WriteLog($"Next Gen Parent is.. Wolf{_fist + 1} and Wolf{_second + 1}.");
        MyLogger.Instance.WriteLog("");
    }

    private void ResetSimulation()
    {
        SceneManager.LoadScene(MAINSCENENUMBER);
    }

    private void Update()
    {
        Time.timeScale = timeScale;
    }
}
