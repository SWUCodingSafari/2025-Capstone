using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static NetworkManager;

public class OutGameUI : MonoBehaviour
{
    private enum OutGameStage
    {
        LogIn,
        Main,
        SelectMapForRank,
        SelectMapForPlay,
        HowToPlay,
    }

    [SerializeField] private NetworkConnecter connecter;

    [Header("LogIn")]
    [SerializeField] private GameObject LoginPanel;
    [SerializeField] private InputField userName;
    [SerializeField] private InputField password;
    [SerializeField] private Text LoginErrorMsg;
    [SerializeField] private GameObject RegistPanel;
    [SerializeField] private InputField registerUserName;
    [SerializeField] private InputField registerPassword;
    [SerializeField] private Text registerErrorMsg;

    [Header("Main")]
    [SerializeField] private GameObject MainPanel;

    [Header("Challenge")]
    [SerializeField] private GameObject ChallengePanelMapSelect;
    [SerializeField] private PresetUI[] presetUIs = new PresetUI[(int)NetworkManager.GameMap.MAX];

    [Header("LeaderBoard")]
    [SerializeField] private GameObject LeaderBoardMapSelect;
    [SerializeField] private SetLeaderBoard[] LeaderBoards = new SetLeaderBoard[(int)NetworkManager.GameMap.MAX];

    [Header("HpwToPlay")]
    [SerializeField] private GameObject HowToPlayPanel;


    private void Awake()
    {
        SetPanel(OutGameStage.LogIn);
        InitLoginPanel();

        if(NetworkManager.Instance.IsLoggedIn == true)
        {
            SetPanel(OutGameStage.Main);
        }
    }

    private void SetPanel(OutGameStage stage)
    {
        LoginPanel.SetActive(stage == OutGameStage.LogIn);
        MainPanel.SetActive(stage == OutGameStage.Main ||
            stage == OutGameStage.SelectMapForRank ||
            stage == OutGameStage.SelectMapForPlay ||
            stage == OutGameStage.HowToPlay);
        ChallengePanelMapSelect.SetActive(stage == OutGameStage.SelectMapForPlay);
        LeaderBoardMapSelect.SetActive(stage == OutGameStage.SelectMapForRank);
        HowToPlayPanel.SetActive(stage == OutGameStage.HowToPlay);
    }

    #region Login
    private void InitLoginPanel()
    {
        LoginErrorMsg.text = string.Empty;
        RegistPanel.SetActive(false);
        registerErrorMsg.text = string.Empty;
    }

    public void OnLogIn()
    {
        userName.enabled = false;
        password.enabled = false;
        connecter.Login(userName.text, password.text, (ok, msg) =>
        {
            userName.enabled = true;
            password.enabled = true;

            if (ok == true)
            {
                SetPanel(OutGameStage.Main);
                return;
            }

            userName.text = string.Empty;
            password.text = string.Empty;
            LoginErrorMsg.text = msg;
        });
    }

    public void GoToSignIn()
    {
        LoginErrorMsg.text = string.Empty;
        registerErrorMsg.text = string.Empty;
        RegistPanel.SetActive(true);
    }

    public void CloseSignUp()
    {
        registerPassword.text = string.Empty;
        registerErrorMsg.text = string.Empty;
        registerPassword.text = string.Empty;
        RegistPanel.SetActive(false);
    }

    public void OnCreateAccount()
    {
        if (registerUserName.text.Length <= 0 || registerPassword.text.Length <= 0) {
            return;
        }

        registerUserName.enabled = false;
        registerPassword.enabled = false;
        connecter.Register(registerUserName.text, registerPassword.text, (ok, msg) =>
        {
            registerUserName.enabled = true;
            registerPassword.enabled = true;
            if (ok == false)
            {
                Debug.LogError(msg);
                registerPassword.text = string.Empty;
                registerUserName.text = string.Empty;
                registerErrorMsg.text = msg;
                return;
            }

            RegistPanel.SetActive(false);
            
            userName.text = string.Empty;
            password.text = string.Empty;
            registerErrorMsg.text = string.Empty;
        });
    }
    #endregion

    #region Main
    public void OnRankChallen()
    {
        SetPanel(OutGameStage.SelectMapForPlay);

        foreach (var pre in presetUIs)
        {
            if (pre == null)
                continue;
            pre.gameObject.SetActive(false);
        }
    }
    public void OnLeaderBoard()
    {
        SetPanel(OutGameStage.SelectMapForRank);
        foreach (SetLeaderBoard l in LeaderBoards)
        {
            if (l == null)
                continue;

            l.gameObject.SetActive(false);
        }
    }
    public void OnHowtoPlay()
    {
        SetPanel(OutGameStage.HowToPlay);
    }
    #endregion

    #region Challenge
    public void ShowPlainMapPreset()
    {
        ShowPresetUI(NetworkManager.GameMap.plain);
    }
    public void ShowRAinMapPreset()
    {
        ShowPresetUI(NetworkManager.GameMap.rain);
    }
    public void ShowSnowMapPreset()
    {
        ShowPresetUI(NetworkManager.GameMap.snow);
    }

    private void ShowPresetUI(NetworkManager.GameMap gameMap)
    {
        foreach(var pre in presetUIs)
        {
            pre.gameObject.SetActive(pre.map == gameMap);
        }

        presetUIs[(int)gameMap].UIInit(GameManager.Instance.StateCount);
    }


    #endregion

    #region LeaderBoard()
    public void OnSelectPlainForLeaderBoard()
    {
        GetRank(NetworkManager.GameMap.plain);
    }
    public void OnSelectRainForLeaderBoard()
    {
        GetRank(NetworkManager.GameMap.rain);
    }
    public void OnSelectSnowForLeaderBoard()
    {
        GetRank(NetworkManager.GameMap.snow);
    }


    private void GetRank(NetworkManager.GameMap map)
    {
        foreach (SetLeaderBoard l in LeaderBoards)
        {
            if (l == null)
                continue;

            l.gameObject.SetActive(l.Map == map);
        }

        connecter.GetTop(map, 10, (ok, msg, rank) =>
        {
            if (ok == false)
                return;

            LeaderBoards[(int)map]?.SetBoard(rank);
        });
    }
    #endregion
}
