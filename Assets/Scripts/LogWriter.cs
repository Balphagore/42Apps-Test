using System;
using System.IO;
using UnityEngine;

public class LogWriter : MonoBehaviour
{
    [SerializeField]
    private string path;
    private StreamWriter writer;
    private float startTime;

    //������� � ������ �� �������.
    private void OnEnable()
    {
        Player.EndGameEvent += OnEndGameEvent;
        Player.PlayerShootEvent += OnPlayerShootEvent;
        Agent.AgentHitEvent += OnAgentHitEvent;
    }

    private void OnDisable()
    {
        Player.EndGameEvent -= OnEndGameEvent;
        Player.PlayerShootEvent -= OnPlayerShootEvent;
        Agent.AgentHitEvent -= OnAgentHitEvent;
    }

    //�������� ������ � ��� �� ������� �����.
    private void Start()
    {
        startTime = Time.realtimeSinceStartup;
        path = Application.persistentDataPath + "/Log.txt";
        writer = new StreamWriter(path, true);
        writer.WriteLine("----------");
        writer.WriteLine("Start round at " + DateTime.Now);
    }
    
    //����������� ������ � ��� ��� ������ ��� ���������.
    private void OnEndGameEvent(string resultText)
    {
        writer.WriteLine(resultText + " round at " + (Time.realtimeSinceStartup - startTime) + " second");
        writer.Close();
    }

    //������ ������������ �������� � ���.
    private void OnPlayerShootEvent()
    {
        writer.WriteLine("Player shoot at " + (Time.realtimeSinceStartup - startTime) + " second");
    }

    private void OnAgentHitEvent()
    {
        writer.WriteLine("Agent hit at " + +(Time.realtimeSinceStartup - startTime) + " second");
    }
}