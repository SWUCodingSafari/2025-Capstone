using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLeaderBoard : MonoBehaviour
{
    [field: SerializeField] public NetworkManager.GameMap Map { get; set; }
    [SerializeField] private SetRankInfo rankInfo;
    [SerializeField] private Transform parent;
    [SerializeField] private GameObject noRecordText;

    public void SetBoard(List<NetworkManager.TopRow> rows)
    {
        while(parent.childCount > 0)
        {
            Destroy(parent.GetChild(0));
        }

        noRecordText.SetActive(rows.Count <= 0);

        int curScore = 0;
        int rankNum = 1;
        int count = 1;
        foreach(var row in rows)
        {
            if(curScore != row.best_score)
            {
                rankNum = count;
            }

            SetRankInfo info = Instantiate(rankInfo, parent);
            info.SetInfo(rankNum, row.username, row.best_score);
            ++count;
        }
    }
}
