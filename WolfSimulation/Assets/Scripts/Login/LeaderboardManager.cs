using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    public GameObject LeaderboardPanel; // 리더보드 화면

    public Button x_button; // 창 닫기 버튼

    public void XButtonClick()
    {
        LeaderboardPanel.SetActive(false); // 창 닫기
    }
}
