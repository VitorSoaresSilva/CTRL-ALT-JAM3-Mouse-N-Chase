using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCar : MonoBehaviour
{
    [field: SerializeField] public CarDamage carDamage { get; private set; }


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
                if(CareerPoints.instance.ShieldUnlocked) ShieldObject.SetActive(true);
            }

            if (SlotObject != null)
            {
                if (CareerPoints.instance.SlotUnlocked) SlotObject.SetActive(true);
            }

            if (BumperObject != null)
            {
                if (CareerPoints.instance.BumperUnlocked) BumperObject.SetActive(true);
            }
        }
    }

    void Update()
    {
        
    }
}
