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
    public int pathDistance = 10; // Tamanho do caminho desejado
    public GameObject pathPrefab; // Prefab a ser usado para criar o caminho
    public Transform pathParent; // Parent object para os objetos do caminho

    private List<Vector2Int> path = new List<Vector2Int>(); // Lista para armazenar o caminho
    private List<Transform> pathObjects = new List<Transform>();
    
    [SerializeField] private PathCreator _pathCreator;
    public Transform[] waypoints;
    public PathCreator pathCreatorInstance;
    public CarFollowPath carFollowPath;
    void Start()
    {
        // pathCreatorInstance = Instantiate (_pathCreator, Vector3.zero, Quaternion.identity);
        GeneratePath();
        DrawPath();
        SetPath();
        carFollowPath.pathCreator = pathCreatorInstance;
        carFollowPath.enabled = true;
        
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
        path.Clear();
        foreach (var pathObject in pathObjects)
        {
            Destroy(pathObject.gameObject);
        }
        pathObjects.Clear();
    }

    void GeneratePath()
    {
        ClearPath();
        path.Add(Vector2Int.zero); // Adiciona o ponto inicial ao caminho

        Vector2Int currentPosition = Vector2Int.zero;

        while (path.Count < pathLength)
        {
            List<Vector2Int> validMoves = new List<Vector2Int>();

            // Verifica os movimentos possíveis
            if (!path.Contains(currentPosition + Vector2Int.right))
                validMoves.Add(currentPosition + Vector2Int.right);
            if (!path.Contains(currentPosition + Vector2Int.left))
                validMoves.Add(currentPosition + Vector2Int.left);
            if (!path.Contains(currentPosition + Vector2Int.up))
                validMoves.Add(currentPosition + Vector2Int.up);
            if (!path.Contains(currentPosition + Vector2Int.down))
                validMoves.Add(currentPosition + Vector2Int.down);

            if (validMoves.Count > 0)
            {
                // Escolhe um movimento aleatório dentre os possíveis
                Vector2Int nextMove = validMoves[Random.Range(0, validMoves.Count)];
                // Atualiza a posição atual e adiciona ao caminho
                currentPosition = nextMove;
                path.Add(nextMove);
            }
            else
            {
                break;
            }

        }
    }

    public void SetPath()
    {
        
        BezierPath bezierPath = new BezierPath (pathObjects, false, PathSpace.xyz);
        pathCreatorInstance.bezierPath = bezierPath;
        // pathCreatorInstance.TriggerPathUpdate();
    }

    void DrawPath()
    {
        foreach (Vector2Int point in path)
        {
            GameObject pathObject = Instantiate(pathPrefab, new Vector3(point.x, 0,point.y) * pathDistance, Quaternion.identity);
            pathObject.transform.SetParent(pathParent);
            pathObjects.Add(pathObject.transform);
        }
    }
}