using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfSearch : AnimalStateBehaviour
{
    private Vector3 searchDir = Vector3.zero;
    private enum SearchType
    {
        None,
        SearchPack,
        SearchDeer,
        SearchBoth,
        MAX
    }
    private SearchType searchType = SearchType.None;

    private float elapsedTime = 0f;
    private float searchTime = 4f;
    private float thinkTime = 0.5f;

    private bool isThinking = false;


    public override void OnEnter()
    {
        OnReset();

        animal.anim.SetBool(DeerAnimation.IsWalking, true);
    }

    public override void OnReset()
    {
        if (searchDir != Vector3.zero)
            return;

        elapsedTime = 0f;
        isThinking = false;

        SetDir();
    }

    private void SetDir()
    {
        int deerCount = animal.DeerList.Count;
        int wolfCount = animal.WolfList.Count;

        if (deerCount > 0 && wolfCount == 0) // 먹이는 있는데 무리가 없음
        {
            searchType = SearchType.SearchPack;
            MoveToDeer();
        }
        else if (wolfCount > 0 && deerCount == 0) // 무리는 있는데 먹이 없음
        {
            searchType = SearchType.SearchDeer;
            int n = Random.Range(0, 10);
            if (n / 10f <= animal.BaseStatus.independence)
                FollowPack();
            else
                SearchRandom();
        }
        else // wolfCount == 0 && deerCount == 0 >> 무리도 먹이도 없음
        {
            searchType = SearchType.SearchBoth;
            SearchRandom();
        }
    }

    public override void OnExit()
    {
        elapsedTime = 0f;
        searchDir = Vector3.zero;

        animal.anim.SetBool(DeerAnimation.IsWalking, false);
    }

    public override bool Update()
    {
        elapsedTime += Time.deltaTime;
        if (isThinking == true)
        {
            if(elapsedTime >= thinkTime)
            {
                animal.anim.SetBool(DeerAnimation.IsWalking, true);
                elapsedTime = 0f;
                isThinking = false;
            }
            return false;
        }

        if (elapsedTime >= searchTime)
        { 
            animal.anim.SetBool(DeerAnimation.IsWalking, false);

            isThinking = true;
            elapsedTime = 0f;
            SetDir();
            return false;
        }

        // 종료 조건 없음, 만약 풀이나 사슴을 찾았다면,
        // 그 쪽에서 환경 변화가 발생했다고 트리거감
        animal.TurnToDesiredDir(searchDir);
        animal.MovePosition();

        return false;
    }

    private void MoveToDeer()
    {
        // 무리가 없긴하지만, 일단 있는 자원인 풀 쪽으로 향하는 것이 생존상 유리

        animal.UpdateEnviroment();

        Deer targetD = animal.DeerList[animal.DeerList.Count / 2]; // 적당한 거리의 풀을 선택
        searchDir = (targetD.transform.position - animal.transform.position).normalized;
        searchDir.y = 0f;
        searchDir = searchDir.normalized;
    }

    private void FollowPack()
    {
        // 무리가 향하는 방향 => 풀이 있을 가능성 높음
        Vector3 center = Vector3.zero;
        foreach (var wolf in animal.WolfList)
            center += wolf.transform.position;
        center /= animal.DeerList.Count;

        searchDir = (center - animal.transform.position).normalized;
    }

    private void SearchRandom()
    {
        currentSearchWay = 0b10000; // 새로운 방향 초기화 어느 뱡향도 아님

        PickSearchDir();
    }

    private enum SearchWay : byte
    {
        Forward = 0b0100,
        Backward = 0b1000,
        Side_Left = 0b0000,     // Side_right와 동일, 비트 연산을 위함
        Side_Right = 0b1100,    // Side_left와 동일, 비트 연산을 위함

        Left = 0b0001,
        Right = 0b0010,
        Straight_Up = 0b0000,   // Straight_Down과 동일, 비트 연산을 위함
        Straight_Down = 0b0011, // Straight_Up과 동일, 비트 연산을 위함
    }
    private byte currentSearchWay = 0b10000;
    private void PickSearchDir()
    {
        byte newDir = 0b00;

        // 새로운 방향이 기존 방향과 동일하지 않고, 완전 반대 방향이 아닌 경우
        do
        {
            newDir = (byte)((Random.Range(0, 4) << 2) + Random.Range(0, 4));
        } while ((((newDir & currentSearchWay) == currentSearchWay) && 
        ((~newDir & currentSearchWay) == currentSearchWay)));

        currentSearchWay = newDir;

        // 앞 뒤 방향 선택(로컬 기준)
        if ((currentSearchWay & (byte)SearchWay.Forward) == (byte)SearchWay.Forward)
        {
            searchDir = animal.transform.forward;
        }
        else if ((currentSearchWay & (byte)SearchWay.Backward) == (byte)SearchWay.Backward)
        {
            searchDir = -animal.transform.forward;
        }

        // 양옆 방향 선택(로컬 기준)
        if ((currentSearchWay & (byte)SearchWay.Left) == (byte)SearchWay.Left)
        {
            searchDir -= animal.transform.right;
        }
        else if ((currentSearchWay & (byte)SearchWay.Right) == (byte)SearchWay.Right)
        {
            searchDir += animal.transform.right;
        }

        searchDir = searchDir.normalized;
    }
}
