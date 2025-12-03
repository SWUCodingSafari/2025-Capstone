using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    public enum WeatherState
    {
        None,
        Rain,
        Snow,
    }

    public enum Degree
    {
        Warm,
        Cool,
        Cold,
    }

    public enum WindDirection
    {
        None,
        North,
        South,
        East,
        West,
        NorthEast,
        NorthWest,
        SouthEast,
        SouthWest,
    }

    public static Vector2Int DirectionToOffset(WindDirection dir)
    {
        switch(dir)
        {
            case WindDirection.North: return new Vector2Int(0, 1);
            case WindDirection.South: return new Vector2Int(0, -1);
            case WindDirection.East: return new Vector2Int(1, 0);
            case WindDirection.West: return new Vector2Int(-1, 0);
            case WindDirection.NorthEast: return new Vector2Int(1, 1);
            case WindDirection.NorthWest: return new Vector2Int(-1, 1);
            case WindDirection.SouthEast: return new Vector2Int(1, -1);
            case WindDirection.SouthWest: return new Vector2Int(-1, -1);
            default: return Vector2Int.zero;
        }
    }

    [Serializable]
    public class WeatherSetting
    {
        public WeatherState State;
        public float RunAdder;
        public float GrassGrowAdder;
        public float VisionAdder;
        public float Percentage;
    }

    public class WeatherInfo
    {
        public WeatherState State;
        public Degree Degree;
        public WindDirection windDirection;
        public float windSpeed;

        public Transform Center;
        public float Range;
    }

    private void OnWeatherStart()
    {
        /*
         * 여기에 날씨 설정
         * - 날씨 Center Transform 생성
         * - Center Transform 기준으로 날씨 범위(random.range)
         */
    }

    [Header("WeatherSetting")]
    [SerializeField] private List<WeatherSetting> weatherSettings;
    private Dictionary<WeatherState, WeatherSetting> WeatherDiction;

    [SerializeField] private float AddWeatherRate = 0.01f;


    [Header("비 설정")]
    public ParticleSystem rainParticleSystem;             // 비 파티클 시스템
    [SerializeField] private float rainDuration = 8f;     // 비 지속 시간
    [SerializeField] private float clearDuration = 12f;   // 맑은 날 지속 시간
    [SerializeField] private float transitionDuration = 5f; // 비 강도 전환 시간
    [SerializeField] private float maxRainRate = 800f;    // 고정된 최대 비 강도

    private ParticleSystem.EmissionModule emissionModule; // 파티클 방출 제어

    [Header("바람 설정")]
    public WindZone windZone;                             // WindZone 컴포넌트
    [SerializeField] private float windMainStrength = 5f; // 고정 바람 세기
    [SerializeField] private float windTurbulence = 1f;   // 고정 난기류

    void Start()
    {
        foreach (var weatherSetting in weatherSettings)
        {
            WeatherDiction[weatherSetting.State] = weatherSetting;
        }

        // 비 파티클 시스템 초기화
        emissionModule = rainParticleSystem.emission;
        emissionModule.rateOverTime = 0f;

        // WindZone이 없으면 자동으로 찾기
        if (windZone == null)
            windZone = FindObjectOfType<WindZone>();

        // WindZone 고정 세기 설정
        windZone.windMain = 0f;
        windZone.windTurbulence = 0f;

        // 날씨, 바람 루프 시작
        StartCoroutine(CoWeatherCycle());
        StartCoroutine(CoWindCycle());
    }

    // 비와 맑은 날 반복 루프
    private IEnumerator CoWeatherCycle()
    {
        while (true)
        {
            // 비 시작 (점점 강해짐)
            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                emissionModule.rateOverTime = Mathf.Lerp(0f, maxRainRate, elapsed / transitionDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            emissionModule.rateOverTime = maxRainRate;

            // 비 지속
            yield return new WaitForSeconds(rainDuration - 2f * transitionDuration);

            // 비 종료 (점점 약해짐)
            elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                emissionModule.rateOverTime = Mathf.Lerp(maxRainRate, 0f, elapsed / transitionDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            emissionModule.rateOverTime = 0f;

            // 맑은 날 지속
            yield return new WaitForSeconds(clearDuration);
        }
    }

    // 바람 10초 불고 10초 멈추는 반복 루프
    private IEnumerator CoWindCycle()
    {
        while (true)
        {
            // 바람 시작
            windZone.windMain = windMainStrength;
            windZone.windTurbulence = windTurbulence;
            yield return new WaitForSeconds(10f);

            // 바람 멈춤
            windZone.windMain = 0f;
            windZone.windTurbulence = 0f;
            yield return new WaitForSeconds(10f);
        }
    }

    private WeatherSetting GetRandomWeatherByPercentage()
    {
        float total = 0f;

        foreach (var ws in weatherSettings)
            total += ws.Percentage;

        float rand = UnityEngine.Random.Range(0, total);
        float cumulative = 0f;

        foreach (var ws in weatherSettings)
        {
            cumulative += ws.Percentage;
            if (rand <= cumulative)
            {
                return ws;
            }
        }

        return weatherSettings[0];
    }
}