using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarUpgrades : MonoBehaviour
{
    [SerializeField] private GameObject ShieldObject;
    [SerializeField] private GameObject SlotObject;
    [SerializeField] private GameObject BumperObject;

    void Start()
    {
        if(CareerPoints.instance != null)
        {
            if(ShieldObject != null)
            { 
                if(CareerPoints.instance.ShieldUnlocked) ShieldObject.SetActive(true);
            }

            if (SlotObject != null)
            {
                if (CareerPoints.instance.ShieldUnlocked) SlotObject.SetActive(true);
            }

            if (BumperObject != null)
            {
                if (CareerPoints.instance.ShieldUnlocked) BumperObject.SetActive(true);
            }
        }
    }

    void Update()
    {
        
    }
}
