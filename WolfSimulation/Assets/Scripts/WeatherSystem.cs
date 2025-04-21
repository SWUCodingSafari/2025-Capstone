using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    public ParticleSystem rainParticleSystem;  // 비 파티클 시스템
    public float rainDuration = 15f;           // 비가 오는 시간
    public float clearDuration = 15f;          // 맑은 시간
    public float transitionDuration = 6f;      // 서서히 시작/종료 시간

    private ParticleSystem.EmissionModule emissionModule;
    private float timer = 0f;
    private bool isRaining = true;

    private float maxRateOverTime = 1000f;

    void Start()
    {
        // Emission 모듈 참조
        emissionModule = rainParticleSystem.emission;
        emissionModule.rateOverTime = 0f; // 처음엔 비가 안 오는 상태로 시작
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (isRaining)
        {
            if (timer < transitionDuration)
            {
                // 비가 서서히 내리기 시작
                float rate = Mathf.Lerp(0f, maxRateOverTime, timer / transitionDuration);
                emissionModule.rateOverTime = rate;
            }
            else if (timer >= rainDuration)
            {
                // 비가 끝남 -> 맑은 상태로 전환
                isRaining = false;
                timer = 0f;
            }
            else
            {
                // 비가 내리는 중
                emissionModule.rateOverTime = maxRateOverTime;
            }
        }
        else
        {
            // 비가 점점 그치는 부분
            if (timer < transitionDuration)
            {
                float t = timer / transitionDuration;
                float smoothT = Mathf.SmoothStep(0f, 1f, t); // 자연스럽게 감속
                float rate = Mathf.Lerp(maxRateOverTime, 0f, smoothT);
                emissionModule.rateOverTime = rate;
            }
            else if (timer >= clearDuration)
            {
                // 맑은 시간 끝 -> 다시 비 상태로 전환
                isRaining = true;
                timer = 0f;
            }
            else
            {
                // 비가 안 오는 중
                emissionModule.rateOverTime = 0f;
            }
        }
    }
}