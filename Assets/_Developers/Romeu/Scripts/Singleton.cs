using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] bool DestroyOnLoad;

    public static T instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = GetComponent<T>();
        }
        else if (instance != GetComponent<T>())
        {
            Destroy(gameObject);
        }

        if (!DestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }
}
