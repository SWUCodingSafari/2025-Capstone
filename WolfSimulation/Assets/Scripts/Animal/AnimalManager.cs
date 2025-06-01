using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AnimalManager<T> : Singleton<AnimalManager<T>> where T : Animal
{
    [SerializeField] private GameObject animalPrefab;
    [SerializeField] private int initialCount = 10;

    private List<T> animals = new List<T>();

    private List<List<T>> packs = new List<List<T>>();

    /// <summary>
    /// 무리(다른 동족)을 만남
    /// </summary>
    /// <param name="_this"></param>
    /// <param name="_other"></param>
    public void MeetPack(T _this, T _other)
    {

        // 이미 무리 성사가 완료됨
        if (_this.PackNumber != -1 && _other.PackNumber != -1 &&
            _this.PackNumber == _other.PackNumber)
        {
            return;
        }

        // 둘다 무리가 있음
        if(_this.PackNumber != -1 && _other.PackNumber != -1 &&
            _this.PackNumber != _other.PackNumber)
        {
            var thisPack = packs[_this.PackNumber];
            var otherPack = packs[_other.PackNumber];
            foreach (var animal in otherPack)
            {
                thisPack.Add(animal);
                animal.Mat.color = _this.Mat.color;
            }

            otherPack.Clear();

        }

        // 나는 무리가 없으며 이미 만난 동물이 무리가 있음
        if (_this.PackNumber == -1 && _other.PackNumber != -1)
        {
            _this.PackNumber = _other.PackNumber;
            packs[_other.PackNumber].Add(_this);

            // debug: 같은 무리면 색으로 표기
            _this.Mat.color = _other.Mat.color;

            return;
        }

        // 나는 무리가 있는데 새로 만난 동물이 무리가 없음
        if(_this.PackNumber != -1 && _other.PackNumber == -1)
        {
            _other.PackNumber = _this.PackNumber;
            packs[_this.PackNumber].Add(_other);

            _other.Mat.color = _this.Mat.color;

            return;
        }

        // 둘 다 무리가 정의 되지 않음
        // 메모리 단편화 방지를 위해 비어있는 무리가 있는지 순회
        int newPackNumber = packs.Count;
        for(int i = 0; i < packs.Count; ++i)
        {
            if (packs[i].Count == 0)
            {
                newPackNumber = i;
                break;
            }
        }

        if(newPackNumber == packs.Count)
        {
            packs.Add(new List<T>());
        }

        packs[newPackNumber].Add(_this);
        packs[newPackNumber].Add(_other);

        _this.PackNumber = newPackNumber;
        _other.PackNumber = newPackNumber;

        // debug: 같은 무리면 색으로 표기
        _this.Mat.color = new Color(Random.Range(0f, 1f), 
            Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        _other.Mat.color = _this.Mat.color;
    }

    /// <summary>
    /// 무리를 잃어버림
    /// </summary>
    /// <param name="_this"></param>
    public void LoosePack(T _this)
    {
        if (_this.PackNumber < 0 || _this.PackNumber >= packs.Count)
            return;

        packs[_this.PackNumber].Remove(_this);
        _this.PackNumber = -1;

        // debug: 무리 색상 초기화
        _this.Mat.color = Color.white;
    }

    public T WantMate(T _this)
    {
        if (_this.PackNumber < 0 && packs[_this.PackNumber].Count <= 1)
            return null;

        List<T> pack = packs[_this.PackNumber];
        pack.Sort((a, b) => 
        (int) ((b.transform.position - _this.transform.position).sqrMagnitude
        - (a.transform.position - _this.transform.position).sqrMagnitude));
        
        for(int i = 1, count = pack.Count; i < count; ++i)
        {
            T p = pack[i];

            if (p.IsDied &&
                p.LookingForMate == true &&
                p.CheckIfThisCanMate() == true)
                return p;
        }

        return null;
    }
}
