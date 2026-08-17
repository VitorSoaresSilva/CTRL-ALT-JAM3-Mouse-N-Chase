using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] bool DestroyOnLoad;

    public static T instance { get; private set; }

    private void Awake()
    {
        T currentComponent = GetComponent<T>();

        // Proteção contra duplicação: se já existe uma instância, destruir esta
        if (instance != null)
        {
            if (instance == currentComponent)
            {
                // Já é a mesma instância, não fazer nada
                return;
            }
            else
            {
                // Há duplicação! Destruir esta (desativa antes pra não rodar OnEnable no duplicado)
                Debug.LogWarning($"[Singleton] {typeof(T).Name} já existe! Destruindo duplicata. " +
                    $"Instancia existente: {instance.gameObject.name}, " +
                    $"Nova tentativa: {gameObject.name}", gameObject);
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }
        }

        instance = currentComponent;

        if (!DestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[Singleton] {typeof(T).Name} marked as DontDestroyOnLoad");
        }
    }

    private void OnDestroy()
    {
        // Limpar instância se este objeto está sendo destruído
        if (instance == GetComponent<T>())
        {
            instance = null;
            Debug.Log($"[Singleton] {typeof(T).Name} destruído, instância clearada");
        }
    }

    /// <summary>
    /// Limpa a instância do singleton (usado ao descarregar a cena)
    /// </summary>
    public static void ClearInstance()
    {
        if (instance != null)
        {
            Debug.Log($"[Singleton] Limpando instância de {typeof(T).Name}");
            Destroy(instance.gameObject);
            instance = null;
        }
    }

    /// <summary>
    /// Verifica se há múltiplas instâncias do singleton (para debug)
    /// </summary>
    public static void AuditInstances()
    {
        T[] allInstances = FindObjectsByType<T>(FindObjectsSortMode.None);
        if (allInstances.Length > 1)
        {
            Debug.LogError($"[Singleton] {typeof(T).Name} tem {allInstances.Length} instâncias! Duplicação detectada!");
            for (int i = 0; i < allInstances.Length; i++)
            {
                Debug.LogError($"  [{i}] {allInstances[i].gameObject.name}");
            }
        }
        else if (allInstances.Length == 1)
        {
            Debug.Log($"[Singleton] {typeof(T).Name} tem 1 instância correta");
        }
        else
        {
            Debug.LogWarning($"[Singleton] {typeof(T).Name} não tem nenhuma instância!");
        }
    }
}
