using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class Laser : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 2;
    [SerializeField] private float chargeTime = 0.7f;
    [SerializeField] private float maxLength = 25;
    [SerializeField] private bool penetrates = true;

    private LineRenderer lineRenderer;
    private float laserLength;
    
    public EventHandler LaserRoutineEnded;
    public EventHandler LaserReady;
    private void OnLaserRoutineEnded() => LaserRoutineEnded?.Invoke(this, EventArgs.Empty);
    private void OnLaserReady() => LaserReady?.Invoke(this, EventArgs.Empty);

    private void FixedUpdate()
    {
        UpdateLaserLength();

        LayerMask onlyEnemies = LayerMask.GetMask("Enemies");
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, transform.right, laserLength, onlyEnemies);
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.Damage(damagePerSecond * Time.fixedDeltaTime);
            }
        }
    }

    public void InitializeParameters()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        gameObject.SetActive(false);
    }

    public virtual void Activate()
    {
        UpdateLaserLength();
        gameObject.SetActive(true);
        OnLaserReady();
    }

    public virtual void Deactivate()
    {
        OnLaserRoutineEnded();
        StopAllCoroutines();
        gameObject.SetActive(false);
    }

    private void UpdateLaserLength()
    {
        LayerMask onlyObstacles = LayerMask.GetMask("Obstacles");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, maxLength, onlyObstacles);
        laserLength = hit.collider != null ? hit.distance : maxLength;
        lineRenderer.SetPosition(0, Vector2.zero);
        lineRenderer.SetPosition(1, Vector2.right * laserLength);
    }
}
