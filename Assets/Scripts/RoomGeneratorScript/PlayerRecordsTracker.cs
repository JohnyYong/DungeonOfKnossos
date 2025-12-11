using System.Collections;
using System.IO;        
using System.Collections.Generic;
using UnityEngine;

public class PlayerRecordsTracker : MonoBehaviour
{
    public int playerDeath = 0;
    public int playerLevelClear = 0;
    public static PlayerRecordsTracker instance;
    private float playTimePerRun = 0.0f;
    [SerializeField]
    private float averagePlayTime = 0.0f;
    private int runCount = 0;
    private string logFilePath;

    public float GetAveragePlayTime()
    {
        return averagePlayTime;
    }
    public void SetPlayerDeathCount(int count)
    {
        runCount++;
        int mins = (int)(playTimePerRun / 60f);
        int secs = (int)(playTimePerRun % 60f);

        Debug.Log($"Run {runCount}'s play time: {(mins > 0 ? $"{mins} mins {secs}" : $"{secs}")} seconds...");
        LogRunTime(runCount, playTimePerRun);

        playerDeath = count;
        averagePlayTime = (averagePlayTime + playTimePerRun) / runCount;
        playTimePerRun = 0.0f;
    }

    public int GetPlayerDeathCount()
    {
        return playerDeath;
    }
    public void SetPlayerLevelClear(int count)
    {
        runCount++;
        int mins = (int)(playTimePerRun / 60f);
        int secs = (int)(playTimePerRun % 60f);

        Debug.Log($"Run {runCount}'s play time: {(mins > 0 ? $"{mins} mins {secs}" : $"{secs}")} seconds...");
        LogRunTime(runCount, playTimePerRun);

        playerLevelClear = count;
        averagePlayTime = (averagePlayTime + playTimePerRun) / runCount;
        playTimePerRun = 0.0f;
    }

    public int GetPlayerLevelClear()
    {
        return playerLevelClear;
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            logFilePath = Path.GetFullPath(Path.Combine(Application.dataPath, "../PlayerRunLog.txt"));
            if (!File.Exists(logFilePath))
                File.WriteAllText(logFilePath, "Player Run Logs:\n");
        }
    }
    private void LogRunTime(int runNum, float timeSeconds)
    {
        int mins = (int)(timeSeconds / 60f);
        int secs = (int)(timeSeconds % 60f);
        string timeStr = mins > 0 ? $"{mins} mins {secs} seconds" : $"{secs} seconds";
        string log = $"Run {runNum}: {timeStr}\n";
        File.AppendAllText(logFilePath, log);
    }
    void Update()
    {
        playTimePerRun += Time.deltaTime;
    }

}
