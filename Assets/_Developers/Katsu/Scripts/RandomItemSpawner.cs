using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomItemSpawner : MonoBehaviour
{
    public List<GameObject> items; // Lista de itens que você quer instanciar
    public int maxItems; // Número máximo de itens que você quer instanciar

    void OnEnable()
    {
        int numItems = Random.Range(0, maxItems + 1); // Número aleatório de itens para instanciar

        for (int i = 0; i < numItems; i++)
        {
            int itemIndex = Random.Range(0, items.Count); // Escolhe um item aleatório da lista
            Instantiate(items[itemIndex], transform.position, Quaternion.identity); // Instancia o item
        }
    }
}
