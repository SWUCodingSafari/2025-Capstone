using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class DeerSearch : AnimalStateBehaviour
{
    private Vector3 searchDir = Vector3.zero;
    private enum SearchType
    {
        None,
        SearchPack,
        SearchGrass,
        SearchBoth,
        MAX
    }
    private SearchType searchType = SearchType.None;

    private float searchTime = 8f;
    private float elapsedTime = 0f;

    public override void OnEnter()
    {
        OnReset();

        animal.anim.SetBool(DeerAnimation.IsWalking, true);
    }

    public override void OnReset()
    {
        int grassCount = animal.GrassList.Count;
        int deerCount = animal.DeerList.Count;

        if (grassCount > 0 && deerCount == 0) // 풀은 있는데 무리가 없음
        {
            searchType = SearchType.SearchPack;
            MoveToGrass();
        }
        else if (deerCount > 0 && grassCount == 0) // 무리는 있는데 풀이 없음
        {
            searchType = SearchType.SearchGrass;
            int n = Random.Range(0, 10);
            if(n / 10 <= animal.BaseStatus.independence)
                FollowPack();
            else
                SearchRandom();
        }
        else // deerCount == 0 && grassCount == 0 >> 무리도 풀도 없음
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
        if (searchType == SearchType.SearchBoth)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= searchTime)
            {
                // todo: 자연스럽게 하기 위해서 조금 멈췄다가 다시 이동하는 걸로 해야함
                elapsedTime -= searchTime;
                PickSearchDir();
            }
        }

        // 종료 조건 없음, 만약 풀이나 사슴을 찾았다면,
        // 그 쪽에서 환경 변화가 발생했다고 트리거감
        animal.TurnToDesiredDir(searchDir);
        animal.MovePosition();

        return false;
    }

    private void MoveToGrass()
    {
        // 무리가 없긴하지만, 일단 있는 자원인 풀 쪽으로 향하는 것이 생존상 유리

        animal.UpdateEnviroment();

        Grass targetG = animal.GrassList[animal.GrassList.Count / 2]; // 적당한 거리의 풀을 선택
        searchDir = (targetG.transform.position - animal.transform.position).normalized;
        searchDir.y = 0f;
        searchDir = searchDir.normalized;
    }

    private void FollowPack()
    {
        // 무리가 향하는 방향 => 풀이 있을 가능성 높음
        Vector3 center = Vector3.zero;
        foreach (var deer in animal.DeerList)
            center += deer.transform.position;
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
        } while (((newDir == currentSearchWay) && (~newDir == currentSearchWay)));

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
    }
}
