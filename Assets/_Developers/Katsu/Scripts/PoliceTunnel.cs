using UnityEngine;
using _Developers.Vitor;

public class PoliceTunnel : MonoBehaviour
{
    public GameObject controlledObject; 
    private bool isActivated = false;

    public void SetActive(bool isActive)
    {
        if (controlledObject != null)
        {
            controlledObject.SetActive(isActive);

            // Se está ativando, otimizar componentes pesados
            if (isActive && !isActivated)
            {
                isActivated = true;
                OptimizePoliceCarForPerformance(controlledObject);
            }
        }
    }

    public void ActivatePolice()
    {
        // Garante que as polícias fiquem visíveis mesmo após o túnel ser reposicionado
        if (controlledObject != null && !controlledObject.activeSelf)
            isActivated = false;

        SetActive(true);
    }

    private void OptimizePoliceCarForPerformance(GameObject policeObject)
    {
        // Desativar scripts desnecessários para reduzir lag
        var scripts = policeObject.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in scripts)
        {
            // Manter apenas scripts essenciais para movimento
            if (!(script is CarFollowPath))
            {
                script.enabled = false;
            }
        }

        // Desativar colliders desnecessários (manter apenas os essenciais)
        var colliders = policeObject.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            // Se não for um collider de trigger para detecção, desativar
            if (!collider.isTrigger)
            {
                collider.enabled = false;
            }
        }

        // Ajustar escala para ficar similar ao player (tipicamente 1x1x1)
        policeObject.transform.localScale = Vector3.one;
    }
}

