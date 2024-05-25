using System.Collections;
using UnityEngine;

public class ObjectTroll : MonoBehaviour
{
    public GameObject objectPrefab; // O objeto que será criado
    public GameObject effectPrefab; // A prefab de efeito que será criada
    public GameObject help; // Referência ao GameObject "Help"
    public bool spawnObject = false; // Se verdadeiro, o objeto será criado

    void Start()
    {
        StartCoroutine(LoopAtivarDesativar());
    }

    void Update()
    {
        if (spawnObject)
        {
            // Cria o objeto um pouco atrás do carro inimigo
            Vector3 spawnPosition = transform.position - transform.forward;
            GameObject spawnedObject = Instantiate(objectPrefab, spawnPosition, Quaternion.identity);

            // Cria a prefab de efeito no mesmo local
            GameObject effectObject = Instantiate(effectPrefab, spawnPosition, Quaternion.identity);

            // Destroi o objeto após 20 segundos
            Destroy(spawnedObject, 20f);
        }
    }

    IEnumerator LoopAtivarDesativar()
    {
        while (true) // Loop infinito
        {
            help.SetActive(true); // Ativa o objeto
            yield return new WaitForSeconds(10); // Espera 10 segundos
            help.SetActive(false); // Desativa o objeto
            yield return new WaitForSeconds(25); // Espera 25 segundos
            help.SetActive(true); // Ativa o objeto novamente
            yield return new WaitForSeconds(5); // Espera 5 segundos
            help.SetActive(false); // Desativa o objeto novamente
        }
    }
}
