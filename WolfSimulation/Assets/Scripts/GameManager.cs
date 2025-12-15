using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using DNA = AnimalStatus.DNAFactors;

public class GameManager : SingletonBehaviour<GameManager>
{
    public enum SceneNumberConst
    {
        Plain = 1,
        Rain = 2,
        Snow = 3
    }

    public struct GAResults
    {
        public int num;
        public DNA dna;
        public float lifeTime;
    }

    [Serializable]
    public class AdderByState
    {
        public WolfState.StateType type;
        public float stateAdder;
        public int maxState = 10;
    }

    [Header("Simulation")]
    [SerializeField] private WolfStatus initialStatus;
    private WolfStatus curInitialStatus;
    [field: SerializeField] public int StateCount { get; private set; } = 20;
    [SerializeField] private AdderByState[] adderSetting = new AdderByState[(int)WolfState.StateType.MAX];
    public WolfState CurState { get; private set; }
    

    [SerializeField]
    [Range(1f, 5f)]
    private float timeScale = 1f;
    [SerializeField] private int maxGeneration = 55;
    private static int GenCount = 0;
    private int topWolfId, secondWolfId;
    private const int MAINSCENENUMBER = 1;


    [Header("Wolf")]
    [SerializeField] public Wolf[] wolfList;
    public UnityEvent OnWolfListSet { get; private set; } = new UnityEvent();
    private GAResults[] wolfStatusList;
    private int totalWolfCount = 0;
    private int initialWolfCount = 0;

    [Header("Deer")]
    [SerializeField] private NewDeer[] deerList;

    private string logFilePath;

    public bool IsGameOver { get; private set; } = false;
    public UnityEvent OnGameOver { get; private set; } = new UnityEvent();


    protected override void Init()
    {
        base.Init();

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        curInitialStatus = new WolfStatus();
    }

    public void StartSimulation(WolfState state, NetworkManager.GameMap map)
    {
        SceneManager.sceneLoaded += WaitForSceneLoad;

        CurState = state;

        curInitialStatus.SetStatus(initialStatus);
        curInitialStatus.health += state.Health * adderSetting[(int)WolfState.StateType.health].stateAdder;

        float speedAdder = 0.8f + state.Speed * adderSetting[(int)WolfState.StateType.speed].stateAdder;
        curInitialStatus.moveSpeed *= speedAdder;
        curInitialStatus.runSpeed *= speedAdder;
        curInitialStatus.maxRunSpeed *= speedAdder;


        curInitialStatus.viewDist += state.Sight * adderSetting[(int)WolfState.StateType.Sight].stateAdder;
        curInitialStatus.hungerSencitivity += state.HungerSensitivity * adderSetting[(int)WolfState.StateType.HungerSensitivity].stateAdder;
        curInitialStatus.scentSencitivity += Mathf.RoundToInt(state.ScentSensitivity * adderSetting[(int)WolfState.StateType.ScentSensitivity].stateAdder);

        if(map == NetworkManager.GameMap.rain)
        {
            curInitialStatus.viewDist *= 0.8f;
            curInitialStatus.addHungerBySec *= 1.2f;
            curInitialStatus.addHungerByheallingSec *= 1.2f;
        }
        else if(map == NetworkManager.GameMap.snow)
        {
            curInitialStatus.viewDist *= 0.95f;
            
            curInitialStatus.addHungerBySec *= 1.15f;
            curInitialStatus.addHungerByheallingSec *= 1.15f;

            curInitialStatus.moveSpeed *= 0.8f;
            curInitialStatus.runSpeed *= 0.8f;
            curInitialStatus.maxRunSpeed *= 0.8f;
        }

            int SceneNumber = 0;
        switch(map)
        {
            case NetworkManager.GameMap.plain: SceneNumber = (int)SceneNumberConst.Plain; break;
            case NetworkManager.GameMap.rain: SceneNumber = (int)SceneNumberConst.Rain; break;
            case NetworkManager.GameMap.snow: SceneNumber = (int)SceneNumberConst.Snow; break;
            default: SceneNumber = (int) SceneNumberConst.Plain;  break;
        }
        SceneManager.LoadScene(SceneNumber);
    }

    private void SetFistSimulation()
    {
        wolfList = GameObject.FindObjectsOfType<Wolf>();
        OnWolfListSet?.Invoke();
        //deerList = GameObject.FindObjectsOfType<Deer>();

        foreach (var item in wolfList)
        {
            item.BaseStatus.SetStatus(curInitialStatus);
            item.BaseStatus.dna = WolfStatus.RandomGeneration(curInitialStatus.dna);
            item.Init();
        }

        initialWolfCount = wolfList.Length;
        totalWolfCount = initialWolfCount;

        wolfStatusList = new GAResults[initialWolfCount];

        topWolfId = 0;
        secondWolfId = 0;

        ++GenCount;
        IsGameOver = false;
    }

    private void SetNextSimulation()
    {
        ++GenCount;

        // 최대 세대까지 시뮬레이션을 진행했다면 종료
        if (GenCount >= maxGeneration) 
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        wolfList = GameObject.FindObjectsOfType<Wolf>();
        //deerList = GameObject.FindObjectsOfType<Deer>();

        foreach (var item in wolfList)
        {
            item.BaseStatus = curInitialStatus.GetNewStatus(curInitialStatus,
                wolfStatusList[topWolfId].dna, wolfStatusList[secondWolfId].dna, item.BaseStatus);
            item.Init();

        }

        topWolfId = 0;
        secondWolfId = 0;
    }

    private void WaitForSceneLoad(Scene arg0, LoadSceneMode arg1)
    {
        SceneManager.sceneLoaded -= WaitForSceneLoad;
        //StartCoroutine(CoWaitForSetup(GenCount <= 0 ? SetFistSimulation: SetNextSimulation));
        StartCoroutine(CoWaitForSetup(SetFistSimulation));
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
            if(totalWolfCount <= 0 && IsGameOver == false)
            {
                //GetTopAndSecond(out topWolfId, out secondWolfId); // 다음 세대 부모 계산
                //RecordGenerationResult(topWolfId, secondWolfId); // 이번 세대 결과 저장
                //ResetSimulation(); // 시뮬레이션 재시작
                IsGameOver = true;
                OnGameOver?.Invoke();
            }
        }
    }

    private void GetTopAndSecond(out int top, out int second)
    {
        top = 0; second = -1;

        GAResults[] tempList = wolfStatusList;

        for(int i = 0; i < 2; ++i)
        {
            int minIndex = i;
            for(int j = i + 1; j < initialWolfCount; ++j)
            {
                if (tempList[j].lifeTime > tempList[minIndex].lifeTime)
                {
                    minIndex = j;
                }
            }

            var temp = tempList[i];
            tempList[i] = tempList[minIndex];
            tempList[minIndex] = temp;

            if(i ==0)
            {
                top = minIndex;
            }
            else
            {
                second = minIndex;
            }
        }

        // 예외 처리 (0번이 top이고, 0번을 제외한 모든 늑대가 같은 수명일 경우)
        if (top == 0 && second == 0 && initialWolfCount > 1)
        {
            second = UnityEngine.Random.Range(1, initialWolfCount);
        }
    }

    private void RecordGenerationResult(int _fist, int _second)
    {
        MyLogger.Instance.WriteLog($"[============Generation {GenCount}===========]");
        MyLogger.Instance.WriteLog(string.Format(
            "{0,-8}: {1,10} | {2,10} | {3,10} | {4,10} | {5,10} | {6,10} | {7,10} | {8,10} | {9,10} | {10,10} | {11,10} | {12,10} | {13,10} | {14,10} | {15,10} | {16,10} | {17,10} | {18,10} | {19,10} | {20,10} | {21,10} | {22,10} |",
            "Name", "LIFE_TIME", "AHBS", "SFBS","SHWE", "SUTMAM", "VD", "MS", "RS", "MRS", "MTABS","INDPD", "HUNG2RAD","EATAD","SEARCHAD","HEALAD","MATEAD","MOVEAD","HYSAD", "ATKRNG","CASEAD", "ATKAD", "MAXHUNGER"
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
