using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Splines;

[RequireComponent(typeof(EdgeCollider2D))]

public class RigController : MonoBehaviour
{
    private EdgeCollider2D screenCollider;
    private Camera mainCam;
    private Vector2[] edgePoints = new Vector2[5];

    [SerializeField] private SplineContainer levelRail;
    [SerializeField] private bool followRail;
    [SerializeField] private int splineI;
    private float splineT;
    [SerializeField] private float splineSeconds;
    [SerializeField] private bool followSplineRot;
    [SerializeField] private bool splineLoop;
    private float3 position;
    private float3 tangent;
    private float3 upVector;

    private void Update()
    {
        if (followRail) MoveAlongRail();
    }

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

    private void MoveAlongRail()
    {
        levelRail.Evaluate(splineI, splineT, out position, out tangent, out upVector);

        transform.position = position;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, 
            followSplineRot ? Vector2.Perpendicular(new Vector2(tangent.x,tangent.y)) : Vector3.up);

        splineT += Time.deltaTime / (splineSeconds > 0 ? splineSeconds : 1);

        if (splineT > 1) 
        {
            if (splineI >= levelRail.Splines.Count - 1 && !splineLoop) { followRail = false; }
            else
            {
                splineI += splineLoop ? 0 : 1;
                splineT = 0;
            }
        }
    }
}
