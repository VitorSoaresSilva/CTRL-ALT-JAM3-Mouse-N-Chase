using PathCreation;
using UnityEngine;

namespace PathCreation.Examples
{

    [ExecuteInEditMode]
    public class ConnectObjectSpawn : PathSceneTool
    {
        public GameObject prefab;
        public GameObject holder;
        public float spacing = 3;
        public float lateralOffset = 1.0f;
        public float heightOffset = 0.0f; 
        public Vector3 rotationOffset = Vector3.zero;
        public bool spawnOnBothSides = true;

        const float minSpacing = .1f;

        void Generate()
        {
            if (pathCreator != null && holder != null)
            {
                DestroyObjects();

                VertexPath path = pathCreator.path;

                //spacing = Mathf.Max(minSpacing, spacing); // original
                spacing = Random.Range(minSpacing, spacing); // aleatório by Romeu_, validar

                float dst = 0;

                while (dst < path.length)
                {
                    Vector3 point = path.GetPointAtDistance(dst);
                    Quaternion rot = path.GetRotationAtDistance(dst);
                    Vector3 normal = path.GetNormalAtDistance(dst);

                    Vector3 offset = normal * lateralOffset;

                    point.y += heightOffset;

                    Quaternion rotation = rot * Quaternion.Euler(rotationOffset);

                    GameObject obj = Instantiate(prefab, point + offset, rotation, holder.transform);
                    obj.transform.localScale = new Vector3(1, 1, 1);

                    if (spawnOnBothSides)
                    {
                        GameObject obj2 = Instantiate(prefab, point - offset, rotation, holder.transform);
                        obj2.transform.localScale = new Vector3(1, 1, 1);
                    }

                    dst += spacing;
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

        protected override void PathUpdated()
        {
            if (pathCreator != null)
            {
                Generate();
            }
        }
    }
}