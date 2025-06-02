using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    public float growDuration = 3f;             // 풀 성장 시간 (초)
    public float regrowDelay = 2f;              // 먹힌 후 재생성까지 대기 시간 (초)
    public Vector3 maxScale = new Vector3(5f, 5f, 5f); // 다 자란 상태의 크기

    private Vector3 initialScale = Vector3.zero;
    private bool isGrown = false;
    public bool IsGrown { get => isGrown; }
    private bool isEaten = false;
    private float growTimer = 0f;

    public GameObject reservedBy {  get; set; }

    void Start()
    {
        transform.localScale = initialScale;
        StartCoroutine(Grow());
    }

    void Update()
    {
        // 자라는 중일 때만 크기 증가
        if (!isGrown && !isEaten)
        {
            growTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(growTimer / growDuration);
            transform.localScale = Vector3.Lerp(initialScale, maxScale, progress);

            if (progress >= 1f)
            {
                isGrown = true;
            }
        }
    }

    public void OnMouseDown()
    {
        Eaten();
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
        isGrown = false;
        isEaten = false;
        growTimer = 0f;
        transform.localScale = initialScale;
        yield return null; // 프레임 한 번 쉬고 자라기 시작
    }

    IEnumerator EatGrass()
    {
        isEaten = true;
        transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(regrowDelay);
        StartCoroutine(Grow());
    }
}
