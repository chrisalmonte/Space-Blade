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

    [SerializeField] private SplineContainer rail;
    [SerializeField] private SplineContainerData railParameters;
    [SerializeField] private bool followRail;

    private int splineI;
    private float splineT;
    private float timeInSpline;
    private bool followSplineRot;
    private bool splineLoop;

    private float3 position;
    private float3 tangent;

    private void Update()
    {
        if (followRail) MoveAlongRail();
    }

    private void Awake()
    {
        screenCollider = GetComponent<EdgeCollider2D>();
        mainCam = Camera.main;
        SetEdgeColliderSize();

        //Debug
        LoadSplineParameters();
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
        rail.Evaluate(splineI, splineT, out position, out tangent, out _);

        transform.position = position;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, 
            followSplineRot ? Vector2.Perpendicular(new Vector2(tangent.x,tangent.y)) : Vector3.up);

        splineT += Time.deltaTime / (timeInSpline > 0 ? timeInSpline : 1);

        if (splineT > 1) 
        {
            if (splineI >= rail.Splines.Count - 1 && !splineLoop) 
            { 
                followRail = false;
                return;
            }

            if (!splineLoop)
            {
                splineI += 1;
                LoadSplineParameters();
            }

            splineT = 0;
        }
    }

    private void LoadSplineParameters()
    {
        if (railParameters == null) { return; }

        timeInSpline = rail.Splines[splineI].GetLength() / (railParameters.Data(splineI).speed == 0 ? 1 : railParameters.Data(splineI).speed);
        followSplineRot = railParameters.Data(splineI).followRotation;
        splineLoop = railParameters.Data(splineI).loop;
    }

    public void BreakRailLoop() { splineLoop = false; }
    public void SetMoveAlongRail(bool move) { followRail = move; }

    public void LoadRail(SplineContainer container, SplineContainerData data)
    {
        rail = container;
        railParameters = data;
        splineI = 0;
        splineT = 0;
        LoadSplineParameters();
    }
}
