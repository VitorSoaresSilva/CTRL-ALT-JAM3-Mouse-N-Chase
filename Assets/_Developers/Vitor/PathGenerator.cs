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
    private GameObject startTunnel;
    private GameObject endTunnel;

    // Propriedades públicas para acessar os tunnels
    public GameObject StartTunnel => startTunnel;
    public GameObject EndTunnel => endTunnel;

    // Flags para validação de timing
    private bool isPathValid = false;
    private bool isMeshReady = false;
    private int lastValidatedPointCount = 0; // Cache para evitar validações repetidas
    private bool enablePathValidation = true; // Flag para desabilitar validação se necessário
    public bool IsPathValid => isPathValid;
    public bool IsMeshReady => isMeshReady;
    public bool EnablePathValidation { get => enablePathValidation; set => enablePathValidation = value; }

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
        pathPoints.Add(Vector3.zero);
        Vector3 currentPosition = Vector3.zero;
        int maxAttempts = 100; // Timeout para evitar loop infinito

        while (pathPoints.Count < pathLength)
        {
            Vector3 nextPoint = Vector3.zero;
            bool validPoint = false;
            int attempts = 0;

            // Gera um ponto candidato
            while (!validPoint && attempts < maxAttempts)
            {
                nextPoint = currentPosition + Random.insideUnitSphere * pathDistance;
                nextPoint.y = 0;

                // Verifica se o próximo ponto está dentro dos limites de ângulo em relação ao ponto anterior
                if (pathPoints.Count > 1)
                {
                    Vector3 lastDirection = (pathPoints[pathPoints.Count - 1] - pathPoints[pathPoints.Count - 2]).normalized;
                    Vector3 nextDirection = (nextPoint - currentPosition).normalized;
                    float angle = Vector3.Angle(lastDirection, nextDirection);

                    // Validar ângulo: aceitar mudanças de direção suaves (0 a 90 graus é bom)
                    if (angle >= minAngle && angle <= maxAngle)
                    {
                        validPoint = true;
                    }
                }
                else
                {
                    validPoint = true;
                }

                attempts++;
            }

            // Se encontrou ponto válido, adiciona; se não, tenta com ângulo mais permissivo
            if (validPoint)
            {
                pathPoints.Add(nextPoint);
                currentPosition = nextPoint;
            }
            else if (pathPoints.Count > 1)
            {
                // Fallback: gerar ponto com ângulo mais permissivo (curvas mais suaves)
                Vector3 lastDirection = (pathPoints[pathPoints.Count - 1] - pathPoints[pathPoints.Count - 2]).normalized;
                nextPoint = currentPosition + lastDirection * pathDistance * 0.8f;
                nextPoint += Random.insideUnitSphere * (pathDistance * 0.3f);
                nextPoint.y = 0;

                pathPoints.Add(nextPoint);
                currentPosition = nextPoint;
            }
        }

        isPathValid = false; // Path ainda não foi validado após geração
        Debug.Log($"PathGenerator: Caminho gerado com {pathPoints.Count} pontos");
    }
    
    public void SetPath()
    {

        BezierPath bezierPath = new BezierPath (pathPoints, false, PathSpace.xyz);
        pathCreatorInstance.bezierPath = bezierPath;
        CenterPath();
        pathCreatorInstance.gameObject.transform.position = centerPosition;

        // Validar continuidade do path apenas se a validação estiver ativada e o número de pontos mudou
        if (enablePathValidation && lastValidatedPointCount != pathPoints.Count)
        {
            if (!ValidatePathContinuity())
            {
                Debug.LogWarning("Path contém buracos/desconexões! Regenerando uma vez...");
                pathPoints.Clear();
                lastValidatedPointCount = 0;
                GeneratePath();
                // Reconstruir bezier com os novos pontos
                bezierPath = new BezierPath (pathPoints, false, PathSpace.xyz);
                pathCreatorInstance.bezierPath = bezierPath;
                CenterPath();
                pathCreatorInstance.gameObject.transform.position = centerPosition;
            }
            lastValidatedPointCount = pathPoints.Count;
        }

        isPathValid = true;

        _roadMeshCreator.TriggerUpdate();
        pathCreatorInstance.TriggerPathUpdate();

        // Pequeno delay para garantir que a mesh foi criada antes de marcar como pronta
        StartCoroutine(MarkMeshReady());

        carFollowPath.ResetPosition();

        if(tunnelPrefab != null)
        {
            VertexPath path = pathCreatorInstance.path;
            // Instancia tunel no final do caminho
            if(startTunnel == null)
                startTunnel = Instantiate(tunnelPrefab, path.GetPointAtDistance(path.length -1, EndOfPathInstruction.Stop), Quaternion.identity);
            else startTunnel.transform.position = path.GetPointAtDistance(path.length - 1, EndOfPathInstruction.Stop);

            startTunnel.transform.rotation = Quaternion.Euler(0, path.GetRotationAtDistance(path.length - 1, EndOfPathInstruction.Stop).eulerAngles.y, 0);

            // Instancia tunel no inicio do caminho
            if(endTunnel == null)
                endTunnel = Instantiate(tunnelPrefab, path.GetPointAtDistance(0, EndOfPathInstruction.Stop), Quaternion.identity);
            else endTunnel.transform.position = path.GetPointAtDistance(0, EndOfPathInstruction.Stop);

            endTunnel.transform.rotation = Quaternion.Euler(0, path.GetRotationAtDistance(0, EndOfPathInstruction.Stop).eulerAngles.y, 0);
        }
    }

    /// <summary>
    /// Valida a continuidade do path verificando se não há buracos entre segmentos
    /// Validação otimizada: apenas verifica, sem logs excessivos
    /// </summary>
    private bool ValidatePathContinuity()
    {
        if (pathPoints.Count < 2) return false;

        // Verificar distância máxima permitida entre pontos consecutivos
        float maxDistanceBetweenPoints = pathDistance * 2.5f; // Tolerância aumentada para evitar regeneração desnecessária

        int holesDetected = 0;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            float distance = Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
            if (distance > maxDistanceBetweenPoints)
            {
                holesDetected++;
                // Log apenas do primeiro buraco para não poluir console
                if (holesDetected == 1)
                {
                    Debug.LogWarning($"Buraco detectado no path entre pontos {i} e {i + 1}");
                }
            }
        }

        if (holesDetected == 0)
        {
            Debug.Log("Path validado com sucesso");
            return true;
        }
        else
        {
            Debug.LogWarning($"Path contém {holesDetected} buracos - regenerando");
            return false;
        }
    }

    IEnumerator MarkMeshReady()
    {
        // Esperar um frame para garantir que a mesh foi renderizada
        yield return null;
        isMeshReady = true;
        Debug.Log("Mesh pronta para o gameplay");
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