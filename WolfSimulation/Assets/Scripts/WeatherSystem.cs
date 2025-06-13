using System.Collections;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    [Header("날씨 주기 설정")]
    [SerializeField] private float weatherChangeOffsetTime = 10f; // 날씨가 바뀌기 전 대기 시간
    private float changeElapsedTime = 0f;                          // 시간 누적용 타이머

    [Header("비 설정")]
    public ParticleSystem rainParticleSystem;                     // 비 파티클 시스템
    public float percentage = 0.3f;                                // 비가 내릴 확률
    public float rainMinDuration = 10f;                            // 비 최소 지속 시간
    public float rainMaxDuration = 20f;                            // 비 최대 지속 시간
    public float clearMinDuration = 10f;                           // 맑은 날 최소 지속 시간
    public float clearMaxDuration = 20f;                           // 맑은 날 최대 지속 시간
    public float transitionDuration = 5f;                          // 비 시작/종료 시 부드러운 전환 시간
    public float maxRainRate = 800f;                               // 최대 비 강도

    [Header("바람 설정")]
    public WindZone windZone;                                      // WindZone 컴포넌트
    public Transform windZoneParent;                               // WindZone이 붙은 부모 오브젝트 (회전용)
    public float windBaseStrength = 5f;                            // 기본 바람 세기
    public float windStrengthVariation = 2f;                       // 바람 세기 진동값
    public float turbulenceBase = 1f;                              // 기본 난기류
    public float turbulenceVariation = 1f;                         // 난기류 진동값
    public float rotationSpeed = 60f;                              // 바람 방향 회전 속도 (도/초)
    public float windFadeDuration = 2f;                            // 바람이 잦아드는 데 걸리는 시간

    private enum WindState { Idle, Blowing, DyingDown, ChangingDirection }
    private WindState windState = WindState.Idle;                  // 현재 바람 상태

    private float currentWindAngle;                                // 현재 바람 방향 (Y축 각도)
    private float targetWindAngle;                                 // 목표 바람 방향 (Y축 각도)

    private ParticleSystem.EmissionModule emissionModule;          // 비 파티클 제어용 모듈
    private bool isRaining = false;                                // 현재 비가 오는지 여부
    private float rainElapsedTime = 0f;                            // 비 시작 이후 경과 시간
    private float rainDuration = 0f;                               // 현재 비 지속 시간

    // 초기 설정
    void Start()
    {
        emissionModule = rainParticleSystem.emission;
        emissionModule.rateOverTime = 0f;

        if (windZone == null)
            windZone = FindObjectOfType<WindZone>();

        if (windZoneParent == null)
            windZoneParent = windZone.transform.parent != null ? windZone.transform.parent : windZone.transform;

        currentWindAngle = windZoneParent.eulerAngles.y;
        targetWindAngle = GetRandomYAngleDifferentFrom(currentWindAngle);

        StartCoroutine(CoWeatherSystem());
    }

    // 매 프레임마다 바람 상태 갱신
    void Update()
    {
        changeElapsedTime += Time.deltaTime;
        HandleWindLogic();
    }

    // 바람 상태 머신 처리
    void HandleWindLogic()
    {
        switch (windState)
        {
            case WindState.Blowing:
                UpdateWind(currentWindAngle, 1f); // 강한 바람 유지
                break;

            case WindState.DyingDown:
                // 바람 점차 줄어들기
                UpdateWind(currentWindAngle, Mathf.Lerp(1f, 0f, Time.deltaTime / windFadeDuration));

                if (windZone.windMain <= 0.1f)
                {
                    windZone.windMain = 0f;
                    windZone.windTurbulence = 0f;
                    targetWindAngle = GetRandomYAngleDifferentFrom(currentWindAngle);
                    windState = WindState.ChangingDirection;
                }
                break;

            case WindState.ChangingDirection:
                // 방향 회전
                currentWindAngle = Mathf.MoveTowardsAngle(currentWindAngle, targetWindAngle, rotationSpeed * Time.deltaTime);
                windZoneParent.rotation = Quaternion.Euler(0f, currentWindAngle, 0f);

                // 목표 각도에 거의 도달하면 종료
                if (Mathf.Abs(Mathf.DeltaAngle(currentWindAngle, targetWindAngle)) < 3f)
                {
                    windState = WindState.Blowing;
                    targetWindAngle = GetRandomYAngleDifferentFrom(currentWindAngle);
                }

                UpdateWind(currentWindAngle, 0f); // 바람은 불지 않지만 회전은 진행
                break;

            case WindState.Idle:
                UpdateWind(currentWindAngle, 0f); // 완전히 멈춤
                break;
        }
    }

    // 바람 세기와 난기류 적용
    void UpdateWind(float angleY, float strengthRatio)
    {
        if (windZone == null) return;

        windZone.windMain = (windBaseStrength + Mathf.Sin(Time.time * 0.5f) * windStrengthVariation) * strengthRatio;
        windZone.windTurbulence = (turbulenceBase + Mathf.PerlinNoise(Time.time * 0.3f, 0f) * turbulenceVariation) * strengthRatio;

        // Debug.Log("Wind Y Angle: " + windZoneParent.eulerAngles.y); // 확인용
    }

    // 현재 방향에서 최소 10도 이상 차이나는 새로운 랜덤 각도 반환
    float GetRandomYAngleDifferentFrom(float baseAngle)
    {
        float newAngle;
        int attempts = 0;

        do
        {
            newAngle = Random.Range(0f, 360f);
            attempts++;
        } while (Mathf.Abs(Mathf.DeltaAngle(baseAngle, newAngle)) < 10f && attempts < 100);

        return newAngle;
    }

    // 날씨 주기 루프 시작
    private IEnumerator CoWeatherSystem()
    {
        yield return new WaitForSeconds(weatherChangeOffsetTime);

        float rainChance = Random.Range(0f, 1f);
        float windChance = Random.Range(0f, 1f);

        if (rainChance <= percentage)
        {
            isRaining = true;
            rainElapsedTime = 0f;
            rainDuration = Random.Range(rainMinDuration, rainMaxDuration);
            StartCoroutine(CoRaining());
        }

        if (windChance <= 0.8f)
        {
            float windDuration = Random.Range(3f, 6f); // 바람 주기 짧게
            StartCoroutine(CoWindCycle(windDuration));
        }
        else
        {
            StartCoroutine(CoWeatherSystem());
        }
    }

    // 비가 시작되고 끝나는 전환 효과 처리
    private IEnumerator CoRaining()
    {
        while (true)
        {
            rainElapsedTime += Time.deltaTime;
            float smoothT = rainElapsedTime / rainDuration;

            if (rainElapsedTime < transitionDuration)
                emissionModule.rateOverTime = Mathf.Lerp(0f, maxRainRate, smoothT);
            else if (rainElapsedTime < rainDuration - transitionDuration)
                emissionModule.rateOverTime = maxRainRate;
            else if (rainElapsedTime < rainDuration)
                emissionModule.rateOverTime = Mathf.Lerp(maxRainRate, 0f, smoothT);
            else
            {
                emissionModule.rateOverTime = 0f;
                break;
            }

            yield return null;
        }

        isRaining = false;
        StartCoroutine(CoWeatherSystem());
    }

    // 바람 루틴: 바람이 불다가 줄어들고, 방향 바꿔 다시 붐
    private IEnumerator CoWindCycle(float windDuration)
    {
        windState = WindState.Blowing;

        float elapsed = 0f;
        while (elapsed < windDuration * 0.6f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        windState = WindState.DyingDown;

        yield return new WaitUntil(() => windState == WindState.Blowing); // 방향 전환 대기

        float remaining = windDuration * 0.4f;
        elapsed = 0f;
        while (elapsed < remaining)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        windState = WindState.Idle;
        StartCoroutine(CoWeatherSystem());
    }
}