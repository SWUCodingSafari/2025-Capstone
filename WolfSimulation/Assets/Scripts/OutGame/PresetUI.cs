using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PresetUI : MonoBehaviour
{
    [SerializeField] public NetworkConnecter connecter;
    [SerializeField] public NetworkManager.GameMap map;
    [SerializeField] private GameObject LoadingPage;

    [Header("Pre")]
    [SerializeField] private Button GetMyBest;
    [SerializeField] private Text MyBestScore;

    [Header("UI")]
    [SerializeField] private Text remainState;

    [Header("Health")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthCount;

    [Header("Speed")]
    [SerializeField] private Slider speedSlider;
    [SerializeField] private Text speedCount;

    [Header("Sight")]
    [SerializeField] private Slider SightSlider;
    [SerializeField] private Text SightCount;

    [Header("Hunger")]
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Text hungerCount;

    [Header("Scent")]
    [SerializeField] private Slider scentSlider;
    [SerializeField] private Text scentCount;

    private WolfState lastBestState = new WolfState();

    private int maxStatusPoint = 20;
    private int remainStatusPoint = 10;

    private void Awake()
    {
        healthSlider.onValueChanged.AddListener((value) =>
        {
            healthCount.text = Mathf.CeilToInt(value / 0.1f).ToString();
        });
        speedSlider.onValueChanged.AddListener((value) =>
        {
            speedCount.text = Mathf.CeilToInt(value / 0.1f).ToString();
        });
        SightSlider.onValueChanged.AddListener((value) =>
        {
            SightCount.text = Mathf.CeilToInt(value / 0.1f).ToString();
        });
        hungerSlider.onValueChanged.AddListener((value) =>
        {
            hungerCount.text = Mathf.CeilToInt(value / 0.1f).ToString();
        });
        scentSlider.onValueChanged.AddListener((value) =>
        {
            scentCount.text = Mathf.CeilToInt(value / 0.1f).ToString();
        });
    }

    public void UIInit(int maxStatusPoint)
    {
        LoadingPage.SetActive(false);
        this.maxStatusPoint = maxStatusPoint;
        remainStatusPoint = maxStatusPoint;
        remainState.text = remainStatusPoint.ToString();

        GetMyBest.gameObject.SetActive(false);

        connecter.GetMyBest(map, (ok, msg, Best) =>
        {
            if (ok == false || Best == null)
            {
                MyBestScore.gameObject.SetActive(false);
                return;
            }

            GetMyBest.gameObject.SetActive(true);

            GetMyBest.onClick.RemoveAllListeners();
            GetMyBest.onClick.AddListener(OnSetState);
            MyBestScore.text = Mathf.CeilToInt(Best.score).ToString();
            lastBestState.SetState(Best.stats);
        });

        healthSlider.value = 0f;
        speedSlider.value = 0f;
        SightSlider.value = 0f;
        hungerSlider.value = 0f;
        scentSlider.value = 0f;
    }

    public void AddState(Slider slider)
    {
        if (remainStatusPoint - 1 < 0)
            return;

        float newVal = Mathf.Clamp01(slider.value + 0.1f);
        if(newVal != slider.value)
        {
            slider.value = newVal;
            remainStatusPoint -= 1;
            remainState.text = remainStatusPoint.ToString();
        }

    }
    public void SubState(Slider slider)
    {
        if (remainStatusPoint + 1 > maxStatusPoint)
            return;

        float newVal = Mathf.Clamp01(slider.value - 0.1f);
        if (newVal != slider.value)
        {

            remainStatusPoint += 1;
            remainState.text = remainStatusPoint.ToString();

            slider.value = newVal;
        }
    }

    public void OnGameStart()
    {
        WolfState state = new WolfState();
        state.Health = Mathf.CeilToInt(healthSlider.value / 0.1f);
        state.Speed = Mathf.CeilToInt(speedSlider.value / 0.1f);
        state.Sight = Mathf.CeilToInt(SightSlider.value / 0.1f);
        state.HungerSensitivity = Mathf.CeilToInt(hungerSlider.value / 0.1f);
        state.ScentSensitivity = Mathf.CeilToInt(scentSlider.value / 0.1f);

        GameManager.Instance.StartSimulation(state, map);
    }
    public void OnSetState()
    {
        healthSlider.value = 0.1f * lastBestState.Health;
        speedSlider.value = 0.1f * lastBestState.Speed;
        SightSlider.value = 0.1f * lastBestState.Sight;
        hungerSlider.value = 0.1f * lastBestState.HungerSensitivity;
        scentSlider.value = 0.1f * lastBestState.ScentSensitivity;

        int used =
            lastBestState.Health +
            lastBestState.Speed +
            lastBestState.Sight +
            lastBestState.HungerSensitivity +
            lastBestState.ScentSensitivity;

        remainStatusPoint = Mathf.Max(0, maxStatusPoint - used);
        remainState.text = remainStatusPoint.ToString();
    }
}
