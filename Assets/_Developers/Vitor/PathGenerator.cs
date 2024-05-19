using System;
using System.Collections;
using System.Collections.Generic;
using _Developers.Vitor;
using PathCreation;
using PathCreation.Examples;
using PathCreation.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

public class PathGenerator : Singleton<PathGenerator>
{
    [Tooltip("Quantidade de pontos na rua")] [SerializeField] 
    private int pathLength = 10;
    [Tooltip("Distancia entre os pontos")] [SerializeField] 
    private float pathDistance = 10;
    [Tooltip("Angulo minimo entre os pontos gerados")] [SerializeField] 
    private float minAngle = -30;
    [Tooltip("Angulo máximo entre os pontos gerados")] [SerializeField]
    private float maxAngle = 30;

    private Transform pathParent;
    private List<Vector3> pathPoints = new List<Vector3>();
    public PathCreator pathCreatorInstance;
    public CarFollowPath carFollowPath;
    private RoadMeshCreator _roadMeshCreator;
    public Vector3 centerPosition;
    public GameObject tunnelPrefab;
    private ConnectObjectSpawn[] connectObjectSpawns;
    private MultipleObjectSpawner[] multipleObjectSpawners;

    void Start()
    {
        connectObjectSpawns = FindObjectsByType<ConnectObjectSpawn>(FindObjectsSortMode.None);
        multipleObjectSpawners = FindObjectsByType<MultipleObjectSpawner>(FindObjectsSortMode.None);

        _roadMeshCreator = pathCreatorInstance.GetComponent<RoadMeshCreator>();
        GeneratePath();
        SetPath();
        carFollowPath.SetPathCreator(pathCreatorInstance);
        carFollowPath.enabled = true;

        foreach(ConnectObjectSpawn con in connectObjectSpawns)
        {
            con.SetPathGenerator(this);
        }

        foreach (MultipleObjectSpawner mult in multipleObjectSpawners)
        {
            mult.SetPathGenerator(this);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GeneratePath();
            SetPath();
        }
    }

    public void ResetPath()
    {
        GeneratePath();
        SetPath();
    }

    void GeneratePath()
    {
        pathPoints.Clear();
        // path.Add(Vector2Int.zero); // Adiciona o ponto inicial ao caminho
        pathPoints.Add(Vector3.zero);
        // Vector3 nextPoint = Vector3.zero;
        Vector3 currentPosition = Vector3.zero;
        // bool validPoint = false;
        while (pathPoints.Count < pathLength)
        {
            Vector3 nextPoint = Vector3.zero;

            // Gera um ponto candidato
            bool validPoint = false;
            while (!validPoint)
            {
                nextPoint = currentPosition + Random.insideUnitSphere * pathDistance;
                nextPoint.y = 0;
                // Verifica se o próximo ponto está dentro dos limites de ângulo em relação ao ponto anterior
                if (pathPoints.Count > 1)
                {
                    Vector3 lastDirection = (pathPoints[pathPoints.Count - 1] - pathPoints[pathPoints.Count - 2]).normalized;
                    Vector3 nextDirection = (nextPoint - currentPosition).normalized;
                    float angle = Vector3.Angle(lastDirection, nextDirection);

                    if (angle >= minAngle && angle <= maxAngle)
                    {
                        validPoint = true;
                    }
                }
                else
                {
                    validPoint = true;
                }
            }

            // Adiciona o ponto ao caminho se for válido
            pathPoints.Add(nextPoint);
            currentPosition = nextPoint;
        }

        // Cria o caminho com curvas suaves usando Bezier
        // pathCreator.bezierPath = new BezierPath(pathPoints, false, PathSpace.xyz);
    }
    
    public void SetPath()
    {
        
        BezierPath bezierPath = new BezierPath (pathPoints, false, PathSpace.xyz);
        pathCreatorInstance.bezierPath = bezierPath;
        CenterPath();
        pathCreatorInstance.gameObject.transform.position = centerPosition;
        _roadMeshCreator.TriggerUpdate();
        pathCreatorInstance.TriggerPathUpdate();
        carFollowPath.ResetPosition();

        // Instancia tunel no final do caminho
        if(tunnelPrefab != null)
        {
            VertexPath path = pathCreatorInstance.path;
            GameObject tunnel = Instantiate(tunnelPrefab, path.GetPointAtDistance(path.length -1, EndOfPathInstruction.Stop), Quaternion.identity);

            tunnel.transform.Rotate(0, path.GetRotationAtDistance(path.length - 1, EndOfPathInstruction.Stop).eulerAngles.y, 0);
        }
    }

    public void CenterPath()
    {
        Vector3 worldCentre = pathCreatorInstance.bezierPath.CalculateBoundsWithTransform (pathCreatorInstance.transform).center;
        Vector3 transformPos = pathCreatorInstance.transform.position;
        if (pathCreatorInstance.bezierPath.Space == PathSpace.xy) {
            transformPos = new Vector3 (transformPos.x, transformPos.y, 0);
        } else if (pathCreatorInstance.bezierPath.Space == PathSpace.xz) {
            transformPos = new Vector3 (transformPos.x, 0, transformPos.z);
        }
        Vector3 worldCentreToTransform = transformPos - worldCentre;

        if (worldCentre != pathCreatorInstance.transform.position) {
            //Undo.RecordObject (creator, "Centralize Transform");
            if (worldCentreToTransform != Vector3.zero) {
                Vector3 localCentreToTransform = MathUtility.InverseTransformVector (worldCentreToTransform, pathCreatorInstance.transform, pathCreatorInstance.bezierPath.Space);
                for (int i = 0; i < pathCreatorInstance.bezierPath.NumPoints; i++) {
                    pathCreatorInstance.bezierPath.SetPoint (i, pathCreatorInstance.bezierPath.GetPoint (i) + localCentreToTransform, true);
                }
            }

            pathCreatorInstance.transform.position = worldCentre;
            pathCreatorInstance.bezierPath.NotifyPathModified();
        }
    }
}