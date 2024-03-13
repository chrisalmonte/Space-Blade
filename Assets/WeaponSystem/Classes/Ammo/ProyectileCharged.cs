using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ProyectileCharged : Proyectile
{
    [Header("Charged Properties")]
    [SerializeField] private float chargedPower = 5;
    [SerializeField] private float chargedSpeed = 70;
    [SerializeField] private float chargeTime = 2;
    [SerializeField] private bool chargelostGradually;
    [SerializeField] [Range(0, 1)] private float minChargeValue = 0.4f;

    private bool shot;
    private float currentSpeed;
    private float currentPower;
    private float chargeValue;

    public EventHandler DestroyedWhileHeld;
    private void OnHitWhileHeld() => DestroyedWhileHeld?.Invoke(this, EventArgs.Empty);
    public float ChargeValue => chargeValue;
    public float ChargeTime => chargeTime;
    public float MinCharge => minChargeValue;
    public bool ChargeLostGradually => chargelostGradually;

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
        UpdateChargeValues();
    }

    protected virtual void UpdateChargeValues()
    {
        currentPower = Mathf.Lerp(power, chargedPower, chargeValue);
        currentSpeed = Mathf.Lerp(speed, chargedSpeed, chargeValue);
    }

    private void ResetChargeValues()
    {
        chargeValue = 0;
        currentPower = power;
        currentSpeed = speed;
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

    protected override void Move() => transform.Translate(Vector3.right * currentSpeed * Time.deltaTime);
    protected override void Damage(IDamageable target) => target.Damage(currentPower);
    protected override void OnInitialized() => ResetChargeValues();
    protected override void OnReturnToPool() => ResetChargeValues();
}
