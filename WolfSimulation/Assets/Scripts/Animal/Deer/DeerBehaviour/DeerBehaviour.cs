using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 사슴 AI가 늑대를 감지하고 회피하는 코드 (임시)
public class DeerBehaviour : MonoBehaviour
{
    private DeerStatus status;

    void Start()
    {
        status = GetComponent<DeerStatus>();
    }

    void Update()
    {
        // 늑대 감지 범위 및 회피 거리
        Collider[] threats = Physics.OverlapSphere(transform.position, status.dna.viewDistance);

        foreach (var col in threats)
        {
            if (col.CompareTag("Wolf"))
            {
                Vector3 dir = transform.position - col.transform.position;
                if (dir.magnitude < status.dna.evadeDistance)
                {
                    // 회피 행동 실행
                    GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(transform.position + dir.normalized * 10f);
                }
            }
        }
    }
}

