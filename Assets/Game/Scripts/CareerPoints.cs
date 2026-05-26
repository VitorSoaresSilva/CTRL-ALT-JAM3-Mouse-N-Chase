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
    public bool usingSecretCar = false;

    // Missions
    [HideInInspector] public int CurrentMissionPoints
    {
        get
        {
            if (SceneControl.instance != null)
            {
                switch (SceneControl.instance.currentMission)
                {
                    case MissionType.FastResponse:
                        return FastResponsePoints;
                    case MissionType.Pursuit:
                        return PursuitPoints;
                    case MissionType.Rescue:
                        return RescuePoints;
                    case MissionType.Boss:
                        return BossPoints;
                }
            }
            return 100;
        }
    }

    [HideInInspector] public int FastResponseCompleted = 0;
    [HideInInspector] public int PursuitCompleted = 0;
    [HideInInspector] public int RescueCompleted = 0;
    [HideInInspector] public int BossCompleted = 0;
    [SerializeField] int FastResponsePoints = 1500;
    [SerializeField] int PursuitPoints = 1500;
    [SerializeField] int RescuePoints = 1500;
    [SerializeField] int BossPoints = 3000;

    // Powerups - Desbloqueio Permanente
    private bool _shieldUnlockedPermanent = false;
    private bool _bumperUnlockedPermanent = false;

    // Pontos necessários para desbloquear upgrades (editável na Unity)
    [SerializeField] private int shieldUnlockPoints = 15000;
    [SerializeField] private int bumperUnlockPoints = 7000;
    [SerializeField] private int slotUnlockPoints = 25000;

    // Bônus do Bumper Upgrade (redução de dano em %)
    [SerializeField, Range(0f, 100f)] private float bumperDamageReduction = 33f; // 33% de redução = 1 chance extra

    public bool ShieldUnlockedPermanent
    { 
        get => _shieldUnlockedPermanent;
        private set => _shieldUnlockedPermanent = value;
    }

    public bool BumperUnlockedPermanent 
    { 
        get => _bumperUnlockedPermanent;
        private set => _bumperUnlockedPermanent = value;
    }

    // Desbloqueio automático por pontos (visível apenas se tem pontos suficientes E não foi permanentemente desbloqueado)
    public bool ShieldUnlocked { get => ShieldUnlockedPermanent || Points >= shieldUnlockPoints; }
    public bool BumperUnlocked { get => BumperUnlockedPermanent || Points >= bumperUnlockPoints; }
    public bool SlotUnlocked { get => Points >= slotUnlockPoints; }
    public bool SecretCarUnlocked { get; private set; }

    // Propriedade para acessar a redução de dano do Bumper
    public float BumperDamageReduction => bumperDamageReduction;

    public bool debug = false;

    void OnEnable()
    {
        Load();
    }

    public void AddPoints(int points)
    {
        this.Points += points;

        // Verificar desbloqueios permanentes de upgrades
        if (!ShieldUnlockedPermanent && Points >= shieldUnlockPoints)
        {
            ShieldUnlockedPermanent = true;
            Log("Shield Upgrade Desbloqueado Permanentemente!");
        }

        if (!BumperUnlockedPermanent && Points >= bumperUnlockPoints)
        {
            BumperUnlockedPermanent = true;
            Log("Bumper Upgrade Desbloqueado Permanentemente!");
        }
    }

    public void RemovePoints(int points)
    {
        Debug.Log($"Points to remove {points}");
        if (this.Points - points < 1)
        {
            if (this.Points != 1) this.Points = 1;
            else this.Points = 0;
        }
        else this.Points -= points;

        LostPoints += points;
    }

    public void Load()
    {
        Points = PlayerPrefs.GetInt("Points", 1000);
        LostPoints = PlayerPrefs.GetInt("LostPoints", 0);
        FastResponseCompleted = PlayerPrefs.GetInt("FastResponseCompleted", 0);
        PursuitCompleted = PlayerPrefs.GetInt("PursuitCompleted", 0);
        RescueCompleted = PlayerPrefs.GetInt("RescueCompleted", 0);
        BossCompleted = PlayerPrefs.GetInt("BossCompleted", 0);
        SecretCarUnlocked = PlayerPrefs.GetInt("SecretCarUnlocked", 0) == 1;

        // Carregar desbloqueios permanentes de upgrades
        ShieldUnlockedPermanent = PlayerPrefs.GetInt("ShieldUnlockedPermanent", 0) == 1;
        BumperUnlockedPermanent = PlayerPrefs.GetInt("BumperUnlockedPermanent", 0) == 1;

        if (PlayerPrefs.HasKey("Points") == false)
            Save();

        Log($"Points: {Points}");
        Log($"LostPoints: {LostPoints}");
        Log($"MissionsCompleted: {MissionsCompleted}");
    }

    public void Save()
    {
        if (!SecretCarUnlocked)
        {
            SecretCarUnlocked = FastResponseCompleted == 9 && PursuitCompleted == 1 && RescueCompleted == 1;
            usingSecretCar = true;
        }

        PlayerPrefs.SetInt("Points", Points);
        PlayerPrefs.SetInt("LostPoints", LostPoints);
        PlayerPrefs.SetInt("FastResponseCompleted", FastResponseCompleted);
        PlayerPrefs.SetInt("PursuitCompleted", PursuitCompleted);
        PlayerPrefs.SetInt("RescueCompleted", RescueCompleted);
        PlayerPrefs.SetInt("BossCompleted", BossCompleted);
        PlayerPrefs.SetInt("SecretCarUnlocked", SecretCarUnlocked ? 1 : 0);

        // Salvar desbloqueios permanentes de upgrades
        PlayerPrefs.SetInt("ShieldUnlockedPermanent", ShieldUnlockedPermanent ? 1 : 0);
        PlayerPrefs.SetInt("BumperUnlockedPermanent", BumperUnlockedPermanent ? 1 : 0);
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

        // Verificar desbloqueio do carro secreto (15 completações totais)
        if (!SecretCarUnlocked && MissionsCompleted >= 15)
        {
            SecretCarUnlocked = true;
            Log("Carro Secreto Desbloqueado!");
        }
    }

    public void ResetProgress()
    {
        Points = 1000;
        //LostPoints = 0;
        FastResponseCompleted = 0;
        PursuitCompleted = 0;
        RescueCompleted = 0;
        BossCompleted = 0;
        Save();
    }

    /// <summary>
    /// Verifica se uma missão está desbloqueada
    /// FastResponse: sempre desbloqueada (padrão)
    /// Pursuit: desbloqueada após completar FastResponse 1 vez
    /// Rescue: desbloqueada após completar Pursuit 1 vez
    /// Boss: desbloqueada após completar cada missão 10 vezes (total 30)
    /// </summary>
    public bool IsMissionUnlocked(MissionType mission)
    {
        switch (mission)
        {
            case MissionType.FastResponse:
                return true; // Sempre desbloqueada

            case MissionType.Pursuit:
                return FastResponseCompleted >= 1; // Após 1 vez FastResponse

            case MissionType.Rescue:
                return PursuitCompleted >= 1; // Após 1 vez Pursuit

            case MissionType.Boss:
                return FastResponseCompleted >= 10 && PursuitCompleted >= 10 && RescueCompleted >= 10;

            default:
                return false;
        }
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
