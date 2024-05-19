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
        // Upgrades
        if(CareerPoints.instance != null)
        {
            if(ShieldObject != null)
            { 
                Debug.Log($"ShieldObject Unlocked: {CareerPoints.instance.ShieldUnlocked}");
                if(CareerPoints.instance.ShieldUnlocked) ShieldObject.SetActive(true);
                if (carDamage != null) carDamage.health += 10;
            }

            //if (SlotObject != null)
            //{
            //    Debug.Log($"ShieldObject Unlocked: {CareerPoints.instance.SlotUnlocked}");
            //    if (CareerPoints.instance.SlotUnlocked) SlotObject.SetActive(true);
            //}

            if (BumperObject != null)
            {
                Debug.Log($"ShieldObject Unlocked: {CareerPoints.instance.BumperUnlocked}");
                if (CareerPoints.instance.BumperUnlocked) BumperObject.SetActive(true);
                if(carDamage != null) carDamage.health += 10;
            }
        }
    }

    void Update()
    {
        
    }
}
