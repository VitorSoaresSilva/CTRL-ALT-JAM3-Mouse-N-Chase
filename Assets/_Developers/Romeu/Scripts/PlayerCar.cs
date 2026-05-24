using _Developers.Vitor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCar : MonoBehaviour
{
    [field: SerializeField] public CarDamage carDamage { get; private set; }
    [field: SerializeField] public SirenController Siren { get; private set; }
    [field: SerializeField] public EngineSound Engine { get; private set; }
    [field: SerializeField] public CarFollowPath Follow { get; private set; }


    [SerializeField, Header("Upgrades")] private GameObject ShieldObject;
    [SerializeField] private GameObject SlotObject;
    [SerializeField] private GameObject BumperObject;



    void Start()
    {
        // Upgrades - Ativar visualmente e aplicar bonus de saúde
        if(CareerPoints.instance != null && carDamage != null)
        {
            // Shield Upgrade
            if(ShieldObject != null && CareerPoints.instance.ShieldUnlocked)
            { 
                ShieldObject.SetActive(true);
                // Adicionar saúde permanentemente ao desbloquear
                if (CareerPoints.instance.ShieldUnlockedPermanent)
                    carDamage.health += 10;
            }

            // Bumper Upgrade
            if (BumperObject != null && CareerPoints.instance.BumperUnlocked)
            {
                BumperObject.SetActive(true);
                // Adicionar saúde permanentemente ao desbloquear
                if (CareerPoints.instance.BumperUnlockedPermanent)
                    carDamage.health += 10;
            }

            // Slot Upgrade (comentado no original)
            //if (SlotObject != null && CareerPoints.instance.SlotUnlocked)
            //{
            //    SlotObject.SetActive(true);
            //}
        }
    }

    void Update()
    {
        
    }
}
