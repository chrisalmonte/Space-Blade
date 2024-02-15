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

    public void Activate()
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.right * maxLength);
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
