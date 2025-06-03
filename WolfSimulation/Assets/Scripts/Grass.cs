using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    public float growDuration = 3f;             // Ǯ ���� �ð� (��)
    public float regrowDelay = 2f;              // ���� �� ��������� ��� �ð� (��)
    public Vector3 maxScale = new Vector3(5f, 5f, 5f); // �ִ� ���� �� ũ��

    private Vector3 initialScale = Vector3.zero;   // �ʱ� ũ��
    private bool isGrown = false;                  // �� �ڶ� ���� ����
    public bool IsGrown { get => isGrown; }        // �ܺ� ���ٿ� getter
    private bool isEaten = false;                  // ���� ���� ����
    private float growTimer = 0f;                  // ���� �ð� ������

    public GameObject reservedBy { get; set; }     // (����) AI�� ���� �� ���

    void Start()
    {
        transform.localScale = initialScale;       // ó���� ũ�� 0���� ����
        StartCoroutine(Grow());                    // ���� �ڷ�ƾ ����
    }

    void Update()
    {
        // �ڶ�� ���� ���� ũ�� ���� ó��
        if (!isGrown && !isEaten)
        {
            growTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(growTimer / growDuration); // 0~1 ����
            transform.localScale = Vector3.Lerp(initialScale, maxScale, progress);

            if (progress >= 1f)
                isGrown = true;
        }
    }

    public void OnMouseDown()
    {
        Eaten();
    }

    public void Eaten()
    {
        Debug.Log("Ŭ����");

        if (isGrown && !isEaten)
        {
            StartCoroutine(EatGrass());
        }
    }

    IEnumerator Grow()
    {
        // �ڶ�� ���� (�ʱ�ȭ)
        isGrown = false;
        isEaten = false;
        growTimer = 0f;
        transform.localScale = initialScale;
        yield return null; // ������ �� �� ��
    }

    IEnumerator EatGrass()
    {
        // Ǯ�� ������ �������, ���� �ð� �� �缺�� ����
        isEaten = true;
        transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(regrowDelay);
        StartCoroutine(Grow());
    }
}