using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        curWolfIndex = -1;
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
            if (index < 0)
                return;
        }

        curWolfIndex = index;
        curWolf = wolfList[curWolfIndex].transform;

        StopAllCoroutines();
        StartCoroutine(CoMoveToNewWolf(curWolf));
    }

    public void ShowNext(bool isToLeft = false)
    {
        int nextWolfIndex = GetNextWolfIndex(isToLeft, true);
        if (nextWolfIndex < 0)
            return;

        ShowWolf(nextWolfIndex);
    }

    private int GetNextWolfIndex(bool isToLeft = false, bool isStartNext = false)
    {
        if (curWolfIndex < 0)
            return -1;

        int index = curWolfIndex;
        if(isStartNext)
        {
            index = (isToLeft ? index - 1 : index + 1) % wolfList.Length;
            index = index < 0 ? wolfList.Length - 1 : index;
        }

        bool isThereLivingWolf = wolfList.Any(w => w.IsDied == false);
        if (isThereLivingWolf == false)
        {
            return -1;
        }

        while (wolfList[index].IsDied == true)
        {
            if (isToLeft)
                index = (index - 1 + wolfList.Length) % wolfList.Length;
            else
                index = (index + 1) % wolfList.Length;
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

        camTrans.position = newWolf.transform.position + camPosOffset;

        isMoving = false;
    }

    private void LateUpdate()
    {
        if (isMoving || curWolf == null || GameManager.Instance.IsGameOver)
            return;

        camTrans.position = curWolf.transform.position + camPosOffset;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnWolfListSet.RemoveListener(GetWolfList);
    }
}
