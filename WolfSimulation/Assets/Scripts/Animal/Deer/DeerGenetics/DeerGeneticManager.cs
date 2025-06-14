using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerGeneticManager : MonoBehaviour
{
    [Header("Simulation Settings")]
    public GameObject deerPrefab;          // 사슴 프리팹
    public int populationSize = 20;        // 개체군 수
    public float mutationRate = 0.1f;      // 돌연변이 확률
    public float generationDuration = 60f; // 한 세대 시뮬레이션 시간

    private List<GameObject> currentDeers = new List<GameObject>();                   // 현재 사슴 리스트
    private List<(DeerDNA, float)> fitnessRecords = new List<(DeerDNA, float)>();     // 유전자 및 생존 시간 기록

    private float generationTimer = 0f;

    void Start()
    {
        CreateInitialPopulation(); // 최초 개체군 생성
    }

    void Update()
    {
        generationTimer += Time.deltaTime;

        if (generationTimer >= generationDuration)
        {
            EvaluateFitness();         // 각 개체의 생존 시간 평가
            EvolveNextGeneration();    // 다음 세대 생성
            generationTimer = 0f;
        }
    }

    // 무작위 유전자 기반으로 초기 개체군 생성
    void CreateInitialPopulation()
    {
        for (int i = 0; i < populationSize; i++)
        {
            DeerDNA dna = DeerDNA.GenerateRandom();
            SpawnDeer(dna);
        }
    }

    // 유전 정보를 기반으로 사슴 생성
    void SpawnDeer(DeerDNA dna)
    {
        GameObject deer = Instantiate(deerPrefab, GetRandomSpawnPosition(), Quaternion.identity);
        deer.GetComponent<DeerStatus>().ApplyDNA(dna);
        currentDeers.Add(deer);
    }

    // 랜덤 스폰 위치 반환
    Vector3 GetRandomSpawnPosition()
    {
        return new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
    }

    // 각 사슴의 생존 시간으로 적합도 평가
    void EvaluateFitness()
    {
        fitnessRecords.Clear();

        foreach (GameObject deer in currentDeers)
        {
            if (deer == null) continue;
            var status = deer.GetComponent<DeerStatus>();
            fitnessRecords.Add((status.dna, status.surviveTime));
            Destroy(deer); // 이전 세대 제거
        }

        currentDeers.Clear();
    }

    // 선택된 유전자를 기반으로 다음 세대 생성
    void EvolveNextGeneration()
    {
        // 생존 시간 기준 내림차순 정렬
        fitnessRecords.Sort((a, b) => b.Item2.CompareTo(a.Item2));

        int topCount = Mathf.CeilToInt(populationSize * 0.5f); // 상위 50% 선택

        for (int i = 0; i < populationSize; i++)
        {
            // 무작위 상위 유전자 두 개 선택
            var parent1 = fitnessRecords[Random.Range(0, topCount)].Item1;
            var parent2 = fitnessRecords[Random.Range(0, topCount)].Item1;

            // 교차 및 돌연변이
            var childDNA = DeerDNA.Crossover(parent1, parent2);
            childDNA.Mutate(mutationRate);

            SpawnDeer(childDNA);
        }
    }
}
