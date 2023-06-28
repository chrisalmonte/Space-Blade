using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D))]

public class RigController : MonoBehaviour
{
    private EdgeCollider2D screenCollider;
    private Vector2[] edgePoints = new Vector2[5];
    private Camera mainCam;

    private void Awake()
    {
        screenCollider = GetComponent<EdgeCollider2D>();
        mainCam = Camera.main;
        SetEdgeColliderSize();
    }

    private void SetEdgeColliderSize()
    {
        edgePoints[0] = new Vector2(-1.78f * mainCam.orthographicSize, mainCam.orthographicSize);
        edgePoints[1] = new Vector2(1.78f * mainCam.orthographicSize, mainCam.orthographicSize);
        edgePoints[2] = new Vector2(1.78f * mainCam.orthographicSize, -mainCam.orthographicSize);
        edgePoints[3] = new Vector2(-1.78f * mainCam.orthographicSize, -mainCam.orthographicSize);
        edgePoints[4] = new Vector2(-1.78f * mainCam.orthographicSize, mainCam.orthographicSize);

        screenCollider.points = edgePoints;
    }
}
