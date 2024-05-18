using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CareerPoints : Singleton<CareerPoints>
{
    // Points
    public int Points { get; private set; }
    public int LostPoints { get; private set; }
    public int MissionsCompleted { get => FastResponseCompleted + PursuitCompleted + RescueCompleted + BossCompleted; }

    // Missions
    public enum MissionType { FastResponse, Pursuit, Rescue, Boss }
    private int FastResponseCompleted = 0;
    private int PursuitCompleted = 0;
    private int RescueCompleted = 0;
    private int BossCompleted = 0;
    [SerializeField] int FastResponsePoints = 1500;
    [SerializeField] int PursuitPoints = 1500;
    [SerializeField] int RescuePoints = 1500;
    [SerializeField] int BossPoints = 3000;

    // Powerups
    public bool ShieldUnlocked { get => Points >= 10000; }
    public bool SlotUnlocked { get => Points >= 20000; }
    public bool BumperUnlocked { get => Points >= 30000; }

    public bool debug = false;

    void OnEnable()
    {
        Load();
    }

    public void AddPoints(int points)
    {
        this.Points += points;
    }

    public void RemovePoints(int points)
    {
        this.Points -= points;
        LostPoints += points;
    }

    public void Load()
    {
        Points = PlayerPrefs.GetInt("Points", 100);
        LostPoints = PlayerPrefs.GetInt("LostPoints", 0);
        FastResponseCompleted = PlayerPrefs.GetInt("FastResponseCompleted", 0);
        PursuitCompleted = PlayerPrefs.GetInt("PursuitCompleted", 0);
        RescueCompleted = PlayerPrefs.GetInt("RescueCompleted", 0);
        BossCompleted = PlayerPrefs.GetInt("BossCompleted", 0);

        Log($"Points: {Points}");
        Log($"LostPoints: {LostPoints}");
        Log($"MissionsCompleted: {MissionsCompleted}");
    }

    public void Save()
    {
        PlayerPrefs.SetInt("Points", Points);
        PlayerPrefs.SetInt("LostPoints", LostPoints);
        PlayerPrefs.SetInt("FastResponseCompleted", FastResponseCompleted);
        PlayerPrefs.SetInt("PursuitCompleted", PursuitCompleted);
        PlayerPrefs.SetInt("RescueCompleted", RescueCompleted);
        PlayerPrefs.SetInt("BossCompleted", BossCompleted);
    }

    public void CompleteMission(MissionType mission)
    {
        switch (mission)
        {
            case MissionType.FastResponse:
                FastResponseCompleted++;
                AddPoints(FastResponsePoints);
                break;
            case MissionType.Pursuit:
                PursuitCompleted++;
                AddPoints(PursuitPoints);
                break;
            case MissionType.Rescue:
                RescueCompleted++;
                AddPoints(RescuePoints);
                break;
            case MissionType.Boss:
                BossCompleted++;
                AddPoints(BossPoints);
                break;
        }
    }

    public bool CheckGameOver()
    {
        return Points < 0;
    }

    void Log(string text)
    {
        if(debug)
            Debug.Log($"CarrerPoints: {text}");
    }

    public void SetPoints(int points)
    {
        Points = points;
    }
}
