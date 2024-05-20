using UnityEngine;

public class PoliceTunnel : MonoBehaviour
{
    public GameObject controlledObject; 

    public void SetActive(bool isActive)
    {
        if (controlledObject != null)
        {
            controlledObject.SetActive(isActive);
        }
    }
}
