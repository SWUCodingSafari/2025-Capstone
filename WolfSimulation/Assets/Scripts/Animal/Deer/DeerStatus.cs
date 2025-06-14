using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerStatus : MonoBehaviour
{
    public DeerDNA dna;             // 사슴 개체의 유전자
    public float surviveTime = 0f;  // 생존 시간 (적합도)

    // 외부에서 유전자 정보를 받아 적용
    public void ApplyDNA(DeerDNA newDNA)
    {
        dna = newDNA;

        // 예: 이동 속도 적용
        GetComponent<UnityEngine.AI.NavMeshAgent>().speed = dna.moveSpeed;
        // 필요하면 시야 반경 등도 여기서 처리 가능
    }

    void Update()
    {
        surviveTime += Time.deltaTime; // 생존 시간 누적
    }
}
