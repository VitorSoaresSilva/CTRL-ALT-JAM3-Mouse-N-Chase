using System;
using System.Collections;
using System.Collections.Generic;
using _Developers.Vitor;
using PathCreation;
using UnityEngine;
using Random = UnityEngine.Random;

public class PathGenerator : MonoBehaviour
{
    public int pathLength = 10; // Tamanho do caminho desejado
    public float pathDistance = 10; // Tamanho do caminho desejado
    public float minAngle = -30; // Tamanho do caminho desejado
    public float maxAngle = 30; // Tamanho do caminho desejado
    public GameObject pathPrefab; // Prefab a ser usado para criar o caminho
    public Transform pathParent; // Parent object para os objetos do caminho

    // private List<Vector2Int> path = new List<Vector2Int>(); // Lista para armazenar o caminho
    private List<Vector3> pathPoints = new List<Vector3>(); // Lista para armazenar o caminho
    private List<Transform> pathObjects = new List<Transform>();
    
    [SerializeField] private PathCreator _pathCreator;
    public Transform[] waypoints;
    public PathCreator pathCreatorInstance;
    public CarFollowPath carFollowPath;
    public CarFollowPath enemyCarFollowPath;
    void Start()
    {
        GeneratePath();
        DrawPath();
        SetPath();
        carFollowPath.pathCreator = pathCreatorInstance;
        carFollowPath.enabled = true;
        enemyCarFollowPath.pathCreator = pathCreatorInstance;
        enemyCarFollowPath.enabled = true;
    }
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GeneratePath();
            DrawPath();
            SetPath();
        }
    }
    private void ClearPath()
    {
        pathPoints.Clear();
        foreach (var pathObject in pathObjects)
        {
            Destroy(pathObject.gameObject);
        }
        pathObjects.Clear();
    }

    void GeneratePath()
    {
        ClearPath();
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
        // pathCreatorInstance.TriggerPathUpdate();
    }

    void DrawPath()
    {
        foreach (Vector3 point in pathPoints)
        {
            GameObject pathObject = Instantiate(pathPrefab, new Vector3(point.x, 0,point.y) * pathDistance, Quaternion.identity);
            pathObject.transform.SetParent(pathParent);
            pathObjects.Add(pathObject.transform);
        }
    }
}