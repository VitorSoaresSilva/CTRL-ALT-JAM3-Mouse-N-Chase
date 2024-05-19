using System;
using System.Collections;
using System.Collections.Generic;
using _Developers.Vitor;
using PathCreation;
using PathCreation.Examples;
using UnityEngine;
using Random = UnityEngine.Random;

public class PathGenerator : MonoBehaviour
{
    public static PathGenerator instance { get; private set; }

    [Tooltip("Quantidade de pontos na rua")] [SerializeField] 
    private int pathLength = 10;
    [Tooltip("Distancia entre os pontos")] [SerializeField] 
    private float pathDistance = 10;
    [Tooltip("Angulo minimo entre os pontos gerados")] [SerializeField] 
    private float minAngle = -30;
    [Tooltip("Angulo máximo entre os pontos gerados")] [SerializeField]
    private float maxAngle = 30;

    public event Action OnPathUpdated;

    private Transform pathParent;
    private List<Vector3> pathPoints = new List<Vector3>();
    public PathCreator pathCreatorInstance;
    public CarFollowPath carFollowPath;
    private RoadMeshCreator _roadMeshCreator;
    //private ConnectObjectSpawn[] connectObjectSpawns;
    //private MultipleObjectSpawner[] multipleObjectSpawners;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this; 
        }
    }

    void Start()
    {
        //connectObjectSpawns = FindObjectsOfType<ConnectObjectSpawn>();
        //multipleObjectSpawners = FindObjectsOfType<MultipleObjectSpawner>();
        _roadMeshCreator = pathCreatorInstance.GetComponent<RoadMeshCreator>();
        GeneratePath();
        SetPath();
        carFollowPath.pathCreator = pathCreatorInstance;
        carFollowPath.enabled = true;
    }
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GeneratePath();
            SetPath();
        }
    }

    void GeneratePath()
    {
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
        _roadMeshCreator.TriggerUpdate();

        OnPathUpdated?.Invoke();
    }
}