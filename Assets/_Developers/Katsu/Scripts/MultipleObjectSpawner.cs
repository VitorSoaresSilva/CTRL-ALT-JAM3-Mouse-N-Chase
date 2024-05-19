using PathCreation;
using PathCreation.Examples;
using UnityEngine;

//[ExecuteInEditMode]
public class MultipleObjectSpawner : PathSceneTool
{
    public GameObject[] prefabs;
    public Vector3[] prefabRotations;
    public float[] prefabHeights;
    public GameObject holder;
    public float spacing = 3;
    public float lateralOffset = 1.0f;
    public float heightOffset = 0.0f;
    public Vector3 rotationOffset = Vector3.zero;
    public bool spawnOnBothSides = true;
    public bool invertXOnOtherSide = true;
    public bool randomOrderOnOtherSide = false;
    public int spawnInterval = 3;
    public int instancesPerObject = 1;
    public bool useCustomScale = false;
    public Vector3 customScale = new Vector3(1, 1, 1);
    public bool alternateDistance = false;
    public float alternateDistanceValue = 5.0f;

    const float minSpacing = .1f;

    void Start()
    {
        if (pathCreator == null)
        {
            pathCreator = PathGenerator.instance.pathCreatorInstance;
        }
        PathGenerator.instance.OnPathUpdated += Activate;
    }

    public void Activate()
    {
        Generate();
    }

    private void Generate()
    {
        if (pathCreator != null && holder != null && prefabs.Length > 0)
        {
            DestroyObjects();

            VertexPath path = pathCreator.path;

            spacing = Mathf.Max(minSpacing, spacing);
            float dst = 0;
            int spawnCount = 0;
            int prefabIndex = 0; // Índice para rastrear qual prefab instanciar
            int instanceCount = 0; // Contador para rastrear quantas instâncias de um prefab foram criadas

            while (dst < path.length)
            {
                Vector3 point = path.GetPointAtDistance(dst);
                Quaternion rot = path.GetRotationAtDistance(dst);
                Vector3 normal = path.GetNormalAtDistance(dst);

                Vector3 offset = normal * lateralOffset;

                // Use a altura do prefab se disponível, caso contrário use a altura geral
                float currentHeightOffset = prefabHeights.Length > prefabIndex ? prefabHeights[prefabIndex] : heightOffset;
                point.y += currentHeightOffset;

                if (spawnCount % spawnInterval == 0)
                {
                    // Use a rotação do prefab se disponível, caso contrário use a rotação geral
                    Vector3 currentRotationOffset = prefabRotations.Length > prefabIndex ? prefabRotations[prefabIndex] : rotationOffset;
                    Quaternion rotation = rot * Quaternion.Euler(currentRotationOffset);

                    // Instancie o prefab atual da matriz
                    GameObject obj = Instantiate(prefabs[prefabIndex], point + offset, rotation, holder.transform);
                    obj.transform.localScale = useCustomScale ? customScale : new Vector3(1, 1, 1);

                    if (spawnOnBothSides)
                    {
                        // Inverta a rotação no eixo X para o objeto no outro lado, se invertXOnOtherSide for verdadeiro
                        Vector3 otherSideRotationOffset = currentRotationOffset;
                        if (invertXOnOtherSide)
                        {
                            otherSideRotationOffset.x = -otherSideRotationOffset.x;
                        }

                        Quaternion otherSideRotation = rot * Quaternion.Euler(otherSideRotationOffset);

                        GameObject obj2 = Instantiate(prefabs[prefabIndex], point - offset, otherSideRotation, holder.transform);
                        obj2.transform.localScale = useCustomScale ? customScale : new Vector3(1, 1, 1);
                    }

                    instanceCount++;

                    // Se instanciamos o objeto suficientes vezes, passe para o próximo objeto
                    if (instanceCount >= instancesPerObject)
                    {
                        prefabIndex = (prefabIndex + 1) % prefabs.Length;
                        instanceCount = 0;
                    }
                }

                // Se alternateDistance estiver ativo, alterne a distância para o próximo spawn
                if (alternateDistance)
                {
                    dst += alternateDistanceValue;
                }
                else
                {
                    dst += spacing;
                }
                spawnCount++;
            }
        }
    }

    void DestroyObjects()
    {
        int numChildren = holder.transform.childCount;
        for (int i = numChildren - 1; i >= 0; i--)
        {
            DestroyImmediate(holder.transform.GetChild(i).gameObject, false);
        }
    }
    //protected override void OnDestroy()
    //{
    //    base.OnDestroy();
    //    PathGenerator.instance.OnPathUpdated -= Activate;
    //}

    protected override void PathUpdated()
    {
        if (pathCreator != null)
        {
            DestroyObjects();
            Generate();
        }
    }
}
