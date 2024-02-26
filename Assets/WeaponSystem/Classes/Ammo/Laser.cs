using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class Laser : MonoBehaviour
{
    [SerializeField] private float maxLength = 25;
    [SerializeField] private float damageRate = 0.2f;
    [SerializeField] private float chargeTime = 0.7f;
    [SerializeField] private bool penetrates = true;

    private LineRenderer lineRenderer;

    public EventHandler LaserRoutineEnded;
    public EventHandler LaserReady;
    private void OnLaserRoutineEnded() => LaserRoutineEnded?.Invoke(this, EventArgs.Empty);
    private void OnLaserReady() => LaserReady?.Invoke(this, EventArgs.Empty);

    private void Update()
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + (transform.right * maxLength));
    }

    public void InitializeParameters()
    {
        lineRenderer = GetComponent<LineRenderer>();
        gameObject.SetActive(false);
    }

    public virtual void Activate()
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.right * maxLength);
        gameObject.SetActive(true);
        OnLaserReady();
    }

    public virtual void Deactivate()
    {
        OnLaserRoutineEnded();
        gameObject.SetActive(false);
    }
}
