using UnityEngine;

public class FenceBuilder : MonoBehaviour
{
    public GameObject fencePrefab;
    public Transform[] fencePoints;

    void Start()
    {
        BuildFence();
    }

    void BuildFence()
    {
        for (int i = 0; i < fencePoints.Length - 1; i++)
        {
            Vector3 start = fencePoints[i].position;
            Vector3 end = fencePoints[i + 1].position;

            float distance = Vector3.Distance(start, end);
            int segments = Mathf.RoundToInt(distance / fencePrefab.transform.localScale.z);

            if (segments > 0)
            {
                for (int j = 0; j <= segments; j++)
                {
                    Vector3 position = Vector3.Lerp(start, end, (float)j / segments);
                    position.y = Terrain.activeTerrain.SampleHeight(position);
                    GameObject fenceSegment = Instantiate(fencePrefab, position, Quaternion.LookRotation(end - start));
                    fenceSegment.transform.rotation = Quaternion.Euler(-90, fenceSegment.transform.rotation.eulerAngles.y, fenceSegment.transform.rotation.eulerAngles.z);
                    fenceSegment.transform.position = new Vector3(fenceSegment.transform.position.x, Terrain.activeTerrain.SampleHeight(fenceSegment.transform.position), fenceSegment.transform.position.z);
                }
            }
        }
    }
}
