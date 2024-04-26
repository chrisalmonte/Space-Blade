using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugChargeShot : MonoBehaviour
{
    [SerializeField] private ProyectileCharged shot;

    private void OnEnable()
    {
        transform.localScale = Vector2.one * shot.ChargeValue;
    }

    private void Update()
    {
        transform.localScale = Vector2.one * shot.ChargeValue;
    }
}
