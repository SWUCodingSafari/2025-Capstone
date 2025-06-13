using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    public float growDuration = 3f;             // 풀 성장 시간 (초)
    public float regrowDelay = 2f;              // 먹힌 후 재생성까지 대기 시간 (초)
    public Vector3 maxScale = new Vector3(5f, 5f, 5f); // 최대 성장 시 크기

    private Vector3 initialScale = Vector3.zero;   // 초기 크기
    private bool isGrown = false;                  // 다 자란 상태인지 여부
    public bool IsGrown { get => isGrown; }        // 외부 접근용 Getter
    private bool isEaten = false;                  // 먹혔는지 여부
    private float growTimer = 0f;                  // 성장 시간 누적

    public GameObject reservedBy { get; set; }     // (선택) AI가 예약한 오브젝트

    void Start()
    {
        transform.localScale = initialScale;       // 시작 시 크기를 0으로 설정
        StartCoroutine(Grow());                    // 성장 코루틴 시작
    }

    void Update()
    {
        // 자라는 중일 때만 크기 증가 처리
        if (!isGrown && !isEaten)
        {
            growTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(growTimer / growDuration); // 0~1 사이 비율
            transform.localScale = Vector3.Lerp(initialScale, maxScale, progress);

            if (progress >= 1f)
                isGrown = true;
        }
    }

    public void OnMouseDown()
    {
        Eaten(); // 마우스로 클릭했을 때 먹히는 함수 호출
    }

    public void Eaten()
    {
        Debug.Log("클릭됨");

        if (isGrown && !isEaten)
        {
            StartCoroutine(EatGrass());
        }
    }

    IEnumerator Grow()
    {
        // 성장 초기화
        isGrown = false;
        isEaten = false;
        growTimer = 0f;
        transform.localScale = initialScale;
        yield return null; // 다음 프레임까지 대기
    }

    IEnumerator EatGrass()
    {
        // 풀을 먹고, 재생성까지 대기한 뒤 다시 성장
        isEaten = true;
        transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(regrowDelay);
        StartCoroutine(Grow());
    }
}