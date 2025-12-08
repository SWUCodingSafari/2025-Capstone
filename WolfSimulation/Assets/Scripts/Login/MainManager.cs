using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    public GameObject MainView; // 메인 화면
    public GameObject RankingView; // 랭킹 화면
    public GameObject LeaderboardPanel; // 리더보드 화면
    public GameObject HowToPlayPanel; // 설명서 화면

    public Button rank_challenge_button; // 랭킹 버튼
    public Button leaderboard_button; // 리더보드 버튼
    public Button how_to_play_button; // 설명서 버튼
    public Button how_to_play_x_button; // 설명서 창 닫기 버튼

    public void RankChallengeButtonClick()
    {
        MainView.SetActive(false); // 메인 화면 열기
        RankingView.SetActive(true); // 랭킹 화면 열기
    }

    public void LeaderboardButtonClick()
    {
        LeaderboardPanel.SetActive(true); // 리더보드 화면 띄우기
    }

    public void HowToPlayButtonClick()
    {
        HowToPlayPanel.SetActive(true); // 설명서 화면 띄우기
    }

    public void XButtonClick()
    {
        HowToPlayPanel.SetActive(false); // 창 닫기
    }
}
