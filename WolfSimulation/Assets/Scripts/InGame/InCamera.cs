using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InCamera : MonoBehaviour
{
    [SerializeField] private Vector3 camPosOffset = Vector3.up * 15f;
    [SerializeField] private float camMoveTime = 0.5f;
    [SerializeField] private Transform curWolf;
    private int curWolfIndex;

    private Transform camTrans;
    private Wolf[] wolfList;

    private bool isMoving = false;

    private void Start()
    {
        camTrans = Camera.main.transform;
        GameManager.Instance.OnWolfListSet.AddListener(GetWolfList);
    }

    private void Update()
    {
        if (isMoving)
            return;

        if(Input.GetKeyUp(KeyCode.RightArrow))
        {
            ShowNext();
        }
        if(Input.GetKeyUp(KeyCode.LeftArrow))
        {
            ShowNext(true);
        }
    }

    public void GetWolfList()
    {
        wolfList = GameManager.Instance.wolfList;

        foreach (var w in wolfList)
        {
            w.OnWolfDied.AddListener(() => ShowNext());
        }

        ShowWolf(0);
    }

    public void ShowWolf(int index)
    {
        if(GameManager.Instance.IsGameOver)
        {
            return;
        }

        if (wolfList[index].IsDied == true)
        {
            index = GetNextWolfIndex();
        }

        curWolfIndex = index;
        curWolf = wolfList[curWolfIndex].transform;

        StopAllCoroutines();
        StartCoroutine(CoMoveToNewWolf(curWolf));
    }

    public void ShowNext(bool isToLeft = false)
    {
        int nextWolfIndex = GetNextWolfIndex(isToLeft);
        ShowWolf(nextWolfIndex);
    }

    private int GetNextWolfIndex(bool isToLeft = false, bool isStartNext = false)
    {
        int index = curWolfIndex;
        if(isStartNext)
        {
            index = (isToLeft ? index - 1 : index + 1) % 4;
            index = index < 0 ? wolfList.Length - 1 : index;
        }

        while (wolfList[index].IsDied == true)
        {
            index = (isToLeft ? index - 1 : index + 1) % 4;
            index = index < 0 ? wolfList.Length - 1 : index;
        }

        return index;
    }

    private IEnumerator CoMoveToNewWolf(Transform newWolf)
    {
        if (newWolf == null)
            yield break;

        float elapsedTime = 0f;
        isMoving = true;

        camTrans.SetParent(null);
        Vector3 curPos = camTrans.position;

        while(elapsedTime < camMoveTime)
        {
            float remainTime = camMoveTime - elapsedTime;

            if (remainTime < 0.001f)
                break;

            Vector3 targetPos = newWolf.position + camPosOffset;
            Vector3 toTarget = targetPos - camTrans.position;

            if(toTarget.sqrMagnitude < 0.001f)
            {
                break;
            }
            else
            {
                Vector3 velocity = toTarget / remainTime;
                camTrans.position += velocity * Time.unscaledDeltaTime;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        camTrans.SetParent(newWolf);
        camTrans.localPosition = camPosOffset;

        isMoving = false;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnWolfListSet.RemoveListener(GetWolfList);
    }
}
