using UnityEngine;
using System.IO;
using Unity.VisualScripting;
using System;

public class MyLogger : SingletonBehaviour<MyLogger>
{
    private string logFilePath;

    void Start()
    {
        // 로그 파일 경로 지정
        logFilePath = Path.Combine(Application.persistentDataPath, 
            $"{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.txt");

        // 파일이 없다면 생성
        if (!File.Exists(logFilePath))
        {
            File.WriteAllText(logFilePath, "=== Simulation Start ===\n");
        }
    }

    // 로그를 파일에 직접 쓰는 함수
    public void WriteLog(string message)
    {
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine($"[{System.DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}

