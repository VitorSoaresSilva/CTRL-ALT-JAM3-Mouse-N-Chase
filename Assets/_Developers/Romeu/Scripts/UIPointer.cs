using UnityEngine;

public class UIPointer : MonoBehaviour
{
    [SerializeField] UIButton FirstButtonSelected;
    [SerializeField] float moveOffset = 20f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private MoveAxis moveAxis = MoveAxis.X;

    private RectTransform rt;
    private float currentOffset = 0f;
    private bool moving = true;
    private float bobBase;
    public enum MoveAxis { X, Y }

    private void OnEnable()
    {
        rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        if (rt == null)
            rt = GetComponent<RectTransform>();

        if (FirstButtonSelected != null)
        {
            if (FirstButtonSelected.TryGetComponent<UnityEngine.UI.Button>(out var btn))
                btn.Select();
        }

        SyncBobBaseFromTransform();
    }

    void Update()
    {
        if (rt == null)
            return;

        if (moving)
        {
            currentOffset += moveSpeed * Time.deltaTime;
            if (currentOffset >= moveOffset)
                moving = false;
        }
        else
        {
            currentOffset -= moveSpeed * Time.deltaTime;
            if (currentOffset <= -moveOffset)
                moving = true;
        }

        // Always bob via anchoredPosition � localPosition fights MoveTo when the hand is rotated.
        Vector2 pos = rt.anchoredPosition;
        if (moveAxis == MoveAxis.X)
            pos.x = bobBase + currentOffset;
        else
            pos.y = bobBase + currentOffset;
        rt.anchoredPosition = pos;
    }

    public void MoveTo(Vector3 anchoredPosition, Quaternion rotation)
    {
        if (rt == null)
            rt = GetComponent<RectTransform>();
        if (rt == null)
            return;

        rt.anchoredPosition = anchoredPosition;
        rt.localRotation = rotation;
        currentOffset = 0f;
        SyncBobBaseFromTransform();
    }

    void SyncBobBaseFromTransform()
    {
        if (rt == null)
            return;

        bobBase = moveAxis == MoveAxis.X ? rt.anchoredPosition.x : rt.anchoredPosition.y;
    }
}
