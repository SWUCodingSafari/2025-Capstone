using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetRankInfo : MonoBehaviour
{
    [SerializeField] private Text rank;
    [SerializeField] private Text userName;
    [SerializeField] private Text Score;

    public void SetInfo(int rankNum, string userNameSTr, int scorenNum)
    {
        rank.text = rankNum.ToString();
        userName.text = userNameSTr.ToString();
        Score.text = scorenNum.ToString();
    }
}
