using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ProyectileCharged : Proyectile
{
    private bool shot;
    private float chargeValue;

    public event EventHandler DestroyedWhileHeld;
    private void OnHitWhileHeld() { DestroyedWhileHeld?.Invoke(this, EventArgs.Empty); }
    public float ChargeValue => chargeValue;

    protected override void Update()
    {
        if (shot)
        {
            Move();
            CheckDistanceLimit();
        }
    }

    public void AddCharge(float value)
    {
        chargeValue = Mathf.Clamp01(chargeValue + value);
        OnChargeChanged();
    }

    public void UpdateShotProperties(float newPower, float newSpeed)
    {
        power = newPower;
        speed = newSpeed;
    }

    protected virtual void OnChargeChanged() { }

    private void ResetChargeValues()
    {
        chargeValue = 0;
        OnChargeChanged();
        shot = false;
    }

    public override void Deploy()
    {
        base.Deploy();
        shot = true;
    }

    public void DisipateCharge()
    {
        ResetChargeValues();
    }

    protected override void OnInitialized() => ResetChargeValues();
    protected override void OnReturnToPool() => ResetChargeValues();

    protected override void OnShotCollided()
    {
        if (!shot)
        {
            OnHitWhileHeld();
            DisipateCharge();
        }

        base.OnShotCollided();
    }
}
