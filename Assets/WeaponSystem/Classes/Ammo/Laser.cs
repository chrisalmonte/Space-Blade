using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class Laser : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject laserTip;

    [Header("Damage Raycast Type")]
    [SerializeField] private CastType castType;
    [Tooltip("Thickness of box or distance between rays in spread mode")]
    [SerializeField] [Min(0)] private float spreadAmount = 0.05f;
    [Tooltip("Extra pairs of spread raycasts")]
    [SerializeField] [Range(1, 6)] private byte spreadPairs = 1;

    private float damagePerSecond;
    private float maxLength;
    private float laserLength;
    private Vector3 laserEndPoint;
    private Vector2 laserShape;
    private ContactFilter2D enemyFilter = new ContactFilter2D();
    private LineRenderer lineRenderer;
    private List<Collider2D> hitList = new List<Collider2D>();
    private List<Collider2D> hitListSpread = new List<Collider2D>();
    
    public EventHandler LaserRoutineEnded;
    public EventHandler LaserReady;
    private void OnLaserRoutineEnded() => LaserRoutineEnded?.Invoke(this, EventArgs.Empty);
    private void OnLaserReady() => LaserReady?.Invoke(this, EventArgs.Empty);

    enum CastType
    {
        Ray,
        Box,
        SpreadRays
    }

    private void FixedUpdate()
    {
        UpdateLaserLength();
        CheckHits();
        
        if(castType == CastType.SpreadRays)
        {
            for (int i = 0; i < spreadPairs; i++)
            {
                CheckHitsSpread(i + 1);
                CheckHitsSpread(-(i + 1));
            }
        }
        
        DamageHitList(damagePerSecond * Time.fixedDeltaTime);
    }

    public void InitializeParameters(float damage, float length)
    {
        damagePerSecond = damage;
        maxLength = length;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        enemyFilter.SetLayerMask(LayerMask.GetMask("Enemies"));

        if (castType != CastType.Ray && spreadAmount <= 0)
        {
            Debug.LogWarning("Cast type set to box/spread but has no spread amount, so it will act as a ray.", this);
            castType = CastType.Ray;
        }

        laserShape = new Vector2(0,castType == CastType.Box ? spreadAmount : 0.01f);
        gameObject.SetActive(false);
        if (laserTip != null) { laserTip.SetActive(false); }
    }

    public virtual void Activate()
    {
        UpdateLaserLength();
        gameObject.SetActive(true);
        if (laserTip != null) { laserTip.SetActive(true); }
        OnLaserReady();
    }

    public virtual void Deactivate()
    {
        OnLaserRoutineEnded();
        StopAllCoroutines();
        if (laserTip != null) { laserTip.SetActive(false); }
        gameObject.SetActive(false);
    }

    private void UpdateLaserLength()
    {
        LayerMask onlyObstacles = LayerMask.GetMask("Obstacles");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, maxLength, onlyObstacles);
        laserLength = hit.collider != null ? hit.distance : maxLength;
        laserEndPoint = transform.position + (transform.right*laserLength);
        laserShape.x = laserLength;
        lineRenderer.SetPosition(0, Vector2.zero);
        lineRenderer.SetPosition(1, Vector2.right * laserLength);

        if (laserTip != null) { laserTip.transform.position = laserEndPoint; }
    }

    private void CheckHits()
    {
        Physics2D.OverlapBox(Vector2.Lerp(transform.position, laserEndPoint, 0.5f), laserShape,
                    Vector2.SignedAngle(Vector2.right, transform.right), enemyFilter, hitList);
    }

    private void CheckHitsSpread(int multiplier)
    {
        Vector3 spreadEnd = (transform.position + (transform.right * laserLength)) + (transform.up * spreadAmount * multiplier);

        Physics2D.OverlapBox(Vector2.Lerp(transform.position, spreadEnd, 0.5f), laserShape,
                    Vector2.SignedAngle(Vector2.right, spreadEnd - transform.position), enemyFilter, hitListSpread);

        foreach (Collider2D hit in hitListSpread)
        {
            if (!hitList.Contains(hit)) hitList.Add(hit);
        }
    }

    private void DamageHitList(float damage)
    {
        foreach (var hit in hitList)
        {
            if (hit.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.Damage(damage);
            }
        }
    }
}
