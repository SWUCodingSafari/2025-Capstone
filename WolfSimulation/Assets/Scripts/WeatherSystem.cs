using System.Collections;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    [SerializeField] private float weatherChangeOffsetTime = 10f;
    private float changeElapsedTime = 0f;

    [Header("Rain Settings")]
    public ParticleSystem rainParticleSystem;
    public float percentage = 0.3f;
    public float rainMinDuration = 10f;        // 비 최소 시간
    public float rainMaxDuration = 20f;        // 비 최대 시간
    public float clearMinDuration = 10f;       // 맑은 최소 시간
    public float clearMaxDuration = 20f;       // 맑은 최대 시간
    public float transitionDuration = 5f;      // 트랜지션 시간
    public float maxRainRate = 800f;

    [Header("Wind Settings")]
    public bool windActive = false;
    public Vector3 windDirection = Vector3.right; // 바람 방향
    public float windStrength = 5f;               // 바람 세기
    public float windOscillationSpeed = 0.5f;     // 바람 방향 변화 속도

    private ParticleSystem.EmissionModule emissionModule;
    private float timer = 0f;
    private float currentCycleDuration;
    private bool isRaining = false;

    void Start()
    {
        emissionModule = rainParticleSystem.emission;
        emissionModule.rateOverTime = 0f;

        StartCoroutine(CoWeatherSystem());
    }

    void Update()
    {
        return;

        if (isRaining)
        {
            Rain();
        }

        if(windActive)
        {
            Wind();
        }

        changeElapsedTime += Time.deltaTime;
        if (changeElapsedTime >= weatherChangeOffsetTime)
        {
            changeElapsedTime -= weatherChangeOffsetTime;

            
        }

        HandleWindEffect();
    }

    private void Wind()
    {
    }

    void StartNewWeatherCycle()
    {
        if (isRaining || windActive)
        {
            return;
        }

        changeElapsedTime += Time.deltaTime;
        if (changeElapsedTime > currentCycleDuration)
        {

        }

        isRaining = !isRaining;
        timer = 0f;
        currentCycleDuration = isRaining ?
            Random.Range(rainMinDuration, rainMaxDuration) :
            Random.Range(clearMinDuration, clearMaxDuration);
    }

    private float rainDuration = 0f;
    private float rainElapsedTime = 0f;
    private void Rain()
    {
        float t = timer / (transitionDuration / 2);
        t = Mathf.Clamp01(t);
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        // 비 시작
        if (timer < transitionDuration)
            emissionModule.rateOverTime = Mathf.Lerp(0f, maxRainRate, smoothT);
        else if (timer > currentCycleDuration)
            isRaining = false;
        else
            emissionModule.rateOverTime = maxRainRate;
    }

    void HandleRainTransition(bool raining)
    {
        float t = timer / transitionDuration;
        t = Mathf.Clamp01(t);
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        if (raining)
        {
            // 비 시작
            if (timer < transitionDuration)
                emissionModule.rateOverTime = Mathf.Lerp(0f, maxRainRate, smoothT);
            else if (timer > currentCycleDuration)
                StartNewWeatherCycle();
            else
                emissionModule.rateOverTime = maxRainRate;
        }
        else
        {
            // 비 그침
            if (timer < transitionDuration)
                emissionModule.rateOverTime = Mathf.Lerp(maxRainRate, 0f, smoothT);
            else if (timer > currentCycleDuration)
                StartNewWeatherCycle();
            else
                emissionModule.rateOverTime = 0f;
        }
    }

    void HandleWindEffect()
    {
        if (!windActive) return;

        // 바람 세기 시간에 따라 부드럽게 변화
        float dynamicStrength = windStrength + Mathf.Sin(Time.time * windOscillationSpeed) * 2f;

        // 만약 파티클이 있다면, 바람 방향 적용
        var velocityOverLifetime = rainParticleSystem.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.x = windDirection.x * dynamicStrength;
        velocityOverLifetime.z = windDirection.z * dynamicStrength;

        // TODO: 다른 오브젝트에 영향 주고 싶다면 여기에서 Rigidbody.AddForce 등 추가 가능
    }

    private IEnumerator CoWeatherSystem()
    {
        yield return new WaitForSeconds(weatherChangeOffsetTime);

        float n = Random.Range(0f, 10f);
        if (n / 10 >= percentage)
        {
            isRaining = true;

            changeElapsedTime = 0f;
            rainDuration = Random.Range(rainMinDuration, rainMaxDuration);

            StartCoroutine(CoRaining());
        }
        else
        {
            StartCoroutine(CoWeatherSystem());
        }
    }

    private IEnumerator CoRaining()
    {
        while(true)
        {
            rainElapsedTime += Time.deltaTime;
            float smoothT = rainElapsedTime / rainDuration;
            if(rainElapsedTime < transitionDuration)
            {
                emissionModule.rateOverTime = Mathf.Lerp(0f, maxRainRate, smoothT);
            }
            else if(rainElapsedTime < rainDuration - transitionDuration)
            {
                emissionModule.rateOverTime = Mathf.Lerp(maxRainRate, 0f, smoothT);
            }
            else if(rainElapsedTime < rainDuration)
            {
                emissionModule.rateOverTime = 0f;

                break;
            }
            else
            {
                emissionModule.rateOverTime = maxRainRate;
            }

            yield return null;
        }

        StartCoroutine(CoWeatherSystem());
    }
}
