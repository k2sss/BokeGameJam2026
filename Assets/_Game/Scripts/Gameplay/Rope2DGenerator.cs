using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Rope2DGenerator : MonoBehaviour
{
    [Header("Endpoints")]
    [SerializeField] private Rigidbody2D startBody;
    [SerializeField] private Rigidbody2D endBody;

    [Header("Shape")]
    [Min(2)]
    [SerializeField] private int segmentCount = 12;
    [Min(0.1f)]
    [SerializeField] private float ropeLength = 6f;
    [SerializeField] private float sagAmount = 1f;
    [SerializeField] private float ropeWidth = 0.12f;

    [Header("Physics")]
    [Min(0.001f)]
    [SerializeField] private float segmentMass = 0.05f;
    [Min(0.001f)]
    [SerializeField] private float segmentLinearDrag = 0.1f;
    [Min(0.001f)]
    [SerializeField] private float segmentAngularDrag = 0.1f;
    [SerializeField] private bool enableSegmentCollision;

    [Header("Rendering")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Color lineColor = new Color(0.28f, 0.2f, 0.1f, 1f);
    [SerializeField] private int lineSortingOrder = 10;

    [SerializeField] private List<Transform> segments = new List<Transform>();

    private LineRenderer lineRenderer;

    public void GenerateRope()
    {
        if (startBody == null || endBody == null)
        {
            Debug.LogWarning("Rope2DGenerator requires both Start Body and End Body.");
            return;
        }

        ClearRope();
        EnsureLineRenderer();

        float segmentLength = ropeLength / segmentCount;
        Vector3 startPosition = startBody.position;
        Vector3 endPosition = endBody.position;

        Rigidbody2D previousBody = startBody;

        for (int i = 0; i < segmentCount; i++)
        {
            GameObject segment = new GameObject($"RopeSegment_{i:00}");
            segment.transform.SetParent(transform, false);
            segment.transform.position = GetInitialSegmentPosition(i, startPosition, endPosition);

            Rigidbody2D segmentBody = segment.AddComponent<Rigidbody2D>();
            segmentBody.mass = segmentMass;
            segmentBody.drag = segmentLinearDrag;
            segmentBody.angularDrag = segmentAngularDrag;
            segmentBody.interpolation = RigidbodyInterpolation2D.Interpolate;
            segmentBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            BoxCollider2D collider = segment.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(segmentLength, ropeWidth);
            collider.enabled = enableSegmentCollision;

            HingeJoint2D hingeJoint = segment.AddComponent<HingeJoint2D>();
            hingeJoint.autoConfigureConnectedAnchor = false;
            hingeJoint.connectedBody = previousBody;
            hingeJoint.anchor = new Vector2(-segmentLength * 0.5f, 0f);
            hingeJoint.connectedAnchor = previousBody == startBody ? Vector2.zero : new Vector2(segmentLength * 0.5f, 0f);
            hingeJoint.enableCollision = enableSegmentCollision;

            previousBody = segmentBody;
            segments.Add(segment.transform);
        }

        HingeJoint2D endHingeJoint = endBody.gameObject.AddComponent<HingeJoint2D>();
        endHingeJoint.autoConfigureConnectedAnchor = false;
        endHingeJoint.connectedBody = previousBody;
        endHingeJoint.anchor = Vector2.zero;
        endHingeJoint.connectedAnchor = new Vector2(segmentLength * 0.5f, 0f);
        endHingeJoint.enableCollision = enableSegmentCollision;

        UpdateVisual();
    }

    public void ClearRope()
    {
        RemoveEndpointJoints(startBody);
        RemoveEndpointJoints(endBody);

        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] != null)
            {
                DestroyImmediateSafe(segments[i].gameObject);
            }
        }

        segments.Clear();

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
    }

    private void LateUpdate()
    {
        UpdateVisual();
    }

    private void OnValidate()
    {
        segmentCount = Mathf.Max(2, segmentCount);
        ropeLength = Mathf.Max(0.1f, ropeLength);
        ropeWidth = Mathf.Max(0.01f, ropeWidth);
        EnsureLineRenderer();
        UpdateLineStyle();
        UpdateVisual();
    }

    private void EnsureLineRenderer()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.useWorldSpace = true;
        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.numCapVertices = 4;
        lineRenderer.numCornerVertices = 2;
        UpdateLineStyle();
    }

    private void UpdateLineStyle()
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.sortingOrder = lineSortingOrder;

        if (lineMaterial != null)
        {
            lineRenderer.sharedMaterial = lineMaterial;
        }
        else if (lineRenderer.sharedMaterial == null)
        {
            lineRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }
    }

    private void UpdateVisual()
    {
        if (lineRenderer == null || startBody == null || endBody == null)
        {
            return;
        }

        int activeSegmentCount = 0;
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] != null)
            {
                activeSegmentCount++;
            }
        }

        lineRenderer.positionCount = activeSegmentCount + 2;
        lineRenderer.SetPosition(0, startBody.position);

        int lineIndex = 1;
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == null)
            {
                continue;
            }

            lineRenderer.SetPosition(lineIndex, segments[i].position);
            lineIndex++;
        }

        lineRenderer.SetPosition(lineIndex, endBody.position);
    }

    private void RemoveEndpointJoints(Rigidbody2D body)
    {
        if (body == null)
        {
            return;
        }

        Joint2D[] joints = body.GetComponents<Joint2D>();
        for (int i = 0; i < joints.Length; i++)
        {
            Rigidbody2D connectedBody = joints[i].connectedBody;
            if (connectedBody != null && connectedBody.transform.IsChildOf(transform))
            {
                DestroyImmediateSafe(joints[i]);
            }
        }
    }

    private Vector3 GetInitialSegmentPosition(int index, Vector3 startPosition, Vector3 endPosition)
    {
        float t = (index + 1f) / (segmentCount + 1f);
        Vector3 point = Vector3.Lerp(startPosition, endPosition, t);

        Vector3 perpendicular = Vector3.Cross((endPosition - startPosition).normalized, Vector3.forward);
        float sag = Mathf.Sin(t * Mathf.PI) * sagAmount;
        point += perpendicular * -sag;

        return point;
    }

    private void DestroyImmediateSafe(Object target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }
}

