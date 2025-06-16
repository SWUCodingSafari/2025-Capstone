using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestInfoUI : MonoBehaviour
{
    public TextMeshProUGUI infoText; // UI Text 연결
    private float elapsedTime = 0f;

    void Start()
    {
        UpdateText();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        UpdateText();
    }

    private void UpdateText()
    {
        // GAManager 인스턴스에서 필요한 정보 가져오기

        // 텍스트 갱신
        infoText.text = $"Time: {elapsedTime:F1}s\n";
    }
}