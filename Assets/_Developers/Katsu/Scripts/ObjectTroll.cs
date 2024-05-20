using UnityEngine;

public class ObjectTroll : MonoBehaviour
{
    public GameObject objectPrefab; // O objeto que ser� criado
    public GameObject effectPrefab; // A prefab de efeito que ser� criada
    public GameObject help; // Refer�ncia ao GameObject "Help"
    public bool spawnObject = false; // Se verdadeiro, o objeto ser� criado

    void Update()
    {
        if (spawnObject)
        {
            // Cria o objeto um pouco atr�s do carro inimigo
            Vector3 spawnPosition = transform.position - transform.forward;
            GameObject spawnedObject = Instantiate(objectPrefab, spawnPosition, Quaternion.identity);

            // Cria a prefab de efeito no mesmo local
            GameObject effectObject = Instantiate(effectPrefab, spawnPosition, Quaternion.identity);

            // Destroi o objeto ap�s 20 segundos
            Destroy(spawnedObject, 20f);
        }
    }
}
