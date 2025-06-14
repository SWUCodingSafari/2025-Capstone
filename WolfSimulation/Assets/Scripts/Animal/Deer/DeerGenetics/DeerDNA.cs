using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DeerDNA
{
    // 유전 형질: 이동 속도
    public float moveSpeed;
    // 유전 형질: 늑대에게 반응할 거리
    public float evadeDistance;
    // 유전 형질: 시야 각도
    public float viewAngle;
    // 유전 형질: 시야 거리
    public float viewDistance;

    // 생성자
    public DeerDNA(float speed, float evade, float angle, float distance)
    {
        moveSpeed = speed;
        evadeDistance = evade;
        viewAngle = angle;
        viewDistance = distance;
    }

    // 무작위 유전자 생성
    public static DeerDNA GenerateRandom()
    {
        return new DeerDNA(
            Random.Range(2f, 6f),
            Random.Range(5f, 15f),
            Random.Range(90f, 150f),
            Random.Range(5f, 20f)
        );
    }

    // 부모 두 개체로부터 교차(Crossover)
    public static DeerDNA Crossover(DeerDNA a, DeerDNA b)
    {
        return new DeerDNA(
            Random.value < 0.5f ? a.moveSpeed : b.moveSpeed,
            Random.value < 0.5f ? a.evadeDistance : b.evadeDistance,
            Random.value < 0.5f ? a.viewAngle : b.viewAngle,
            Random.value < 0.5f ? a.viewDistance : b.viewDistance
        );
    }

    // 돌연변이 적용
    public void Mutate(float rate)
    {
        if (Random.value < rate) moveSpeed += Random.Range(-0.5f, 0.5f);
        if (Random.value < rate) evadeDistance += Random.Range(-1f, 1f);
        if (Random.value < rate) viewAngle += Random.Range(-10f, 10f);
        if (Random.value < rate) viewDistance += Random.Range(-2f, 2f);
    }
}
