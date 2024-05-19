using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraControl : MonoBehaviour
{
    [field: SerializeField, Header("Target")] public Transform LookAt { get; set; }
    [field: SerializeField] public Vector3 lookOffset { get; private set; }
    [field: SerializeField] public Transform Follow { get; set; }

    [field: SerializeField, Header("Follow")] public float FollowDistance { get; set; } = 10.0f;
    [field: SerializeField] public float FollowHeight { get; set; } = 5.0f;
    [field: SerializeField, Range(0, 1)] public float Smoothing { get; set; } = 0.25f;

    public enum UpdateMode { Update, LateUpdate, FixedUpdate }
    [field: SerializeField] private UpdateMode updateMode { get; set; } = UpdateMode.LateUpdate;

    public Camera Camera { get; private set; }

    void Awake()
    {
        Camera = GetComponent<Camera>();
    }

    private void Start()
    {
        if(CareerPoints.instance != null)
        {
            if (CareerPoints.instance.Points > 100000)
                if (Camera.TryGetComponent(out Volume vol))
                    vol.enabled = false;
        }
    }

    void UpdateCamera()
    {
        if(LookAt != null)
        {
            Vector3 lookPos = Vector3.Lerp(transform.position, LookAt.position + lookOffset, Smoothing);
            transform.LookAt(lookPos);
        }

        if(Follow != null)
        {
            Vector3 desiredPos = Follow.position - Follow.forward * FollowDistance + Vector3.up * FollowHeight;
            transform.position = Vector3.Lerp(transform.position, desiredPos, Smoothing);
        }
    }

    public void SetTarget(Transform target)
    {
        LookAt = target;
        Follow = target;
    }

    void Update()
    {
        if(updateMode == UpdateMode.Update)
            UpdateCamera();
    }

    void LateUpdate()
    {
        if(updateMode == UpdateMode.LateUpdate)
            UpdateCamera();
    }

    void FixedUpdate()
    {
        if(updateMode == UpdateMode.FixedUpdate)
            UpdateCamera();
    }
}
