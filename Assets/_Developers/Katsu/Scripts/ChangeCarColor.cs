namespace TurnTheGameOn.SimpleTrafficSystem
{
    using System.Collections.Generic;
    using UnityEngine;

    public enum BodyMaterialType { Metal, Plastic }

    [DisallowMultipleComponent]
    public class ChangeCarColor : MonoBehaviour
    {
        [SerializeField] public Color[] newColors;
        [SerializeField] private CarBody carBody;
        [SerializeField] private BodyPart[] bodyParts;

        [HideInInspector] public Color carColor;

        void Start()
        {
            // Sorteia uma cor aleatória
            int randomNumber = Random.Range(0, newColors.Length);
            carColor = newColors[randomNumber];

            // Muda a cor da lataria (carBody)
            foreach(MeshRenderer renderer in carBody.meshRenderers)
            {
                Material[] currentMaterials = renderer.materials;

                foreach (Material material in currentMaterials)
                {
                    material.color = carColor;
                }
            }

            // Escolhe um material aleatório para cada parte
            foreach (BodyPart part in bodyParts)
            {
                Material partMaterial;
                int variation = (!part.varyMateriais)
                    ? 0
                    : Random.Range(0, part.targetMaterials.Length);

                partMaterial = part.targetMaterials[variation];

                foreach (MeshRenderer renderer in part.meshRenderers)
                {
                    renderer.material = partMaterial;
                    if (!part.varyMateriais || variation == 0)
                        renderer.material.color = carColor;
                }

            }
        }
    }

    [System.Serializable]
    public class BodyPart : CarBody
    {
        [SerializeField] public bool varyMateriais = true;
    }

    [System.Serializable]
    public class CarBody
    {
        [SerializeField] public MeshRenderer[] meshRenderers;
        [Tooltip("Keep the body material in the first element")]
        [SerializeField] public Material[] targetMaterials;
    }
}
