using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    public float GrowSpeed = 0.5f; // �ܵ� ���� �ӵ�
    public Vector3 maxScale = new Vector3(2f, 2f, 2f); // �ִ� ũ��

    void Update()
    {
        // �ִ� ũ�⿡ �������� �ʾ��� ��
        if (transform.localScale.x < maxScale.x ||
            transform.localScale.y < maxScale.y ||
            transform.localScale.z < maxScale.z)
        {
            // ���� ũ�⿡ �ӵ� ��ŭ ���� �� ũ�� ���
            Vector3 newScale = transform.localScale + Vector3.one * GrowSpeed * Time.deltaTime;

            // �ִ� ũ�⸦ ���� �ʵ��� ����
            newScale = Vector3.Min(newScale, maxScale);

            // �� ũ�� ����
            transform.localScale = newScale;
        }
    }
}
