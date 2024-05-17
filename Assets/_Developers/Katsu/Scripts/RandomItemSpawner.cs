using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomItemSpawner : MonoBehaviour
{
    public List<GameObject> items; // Lista de itens que voc� quer instanciar
    public int maxItems; // N�mero m�ximo de itens que voc� quer instanciar

    void OnEnable()
    {
        int numItems = Random.Range(0, maxItems + 1); // N�mero aleat�rio de itens para instanciar

        for (int i = 0; i < numItems; i++)
        {
            int itemIndex = Random.Range(0, items.Count); // Escolhe um item aleat�rio da lista
            Instantiate(items[itemIndex], transform.position, Quaternion.identity); // Instancia o item
        }
    }
}
