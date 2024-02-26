using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ChargedProyectile : MonoBehaviour
{
    [Header("Charge Properties")]
    [SerializeField] [Min(0)] private float startPower = 1;
    [SerializeField] [Min(1)] private float endPower = 5;
    [SerializeField] [Min(0)] private float startSpeed = 15;
    [SerializeField] [Min(0)] private float endSpeed = 25;
    [SerializeField] private float chargeTime = 2;
    [SerializeField] private bool chargelostGradually;
    [SerializeField] [Range(0,1)] private float minChargeValue = 0.4f;
    [SerializeField] private float maxDistance = 50;

    private float power;
    private float speed;
    private float chargeValue;
    private bool shot;
    private Vector2 startPosition;
    private IObjectPool<ChargedProyectile> shotPool;

    public EventHandler DestroyedWhileHeld;
    private void OnHitWhileHeld() => DestroyedWhileHeld?.Invoke(this, EventArgs.Empty);

    public float ChargeValue => chargeValue;
    public float ChargeTime => chargeTime;
    public float MinCharge => minChargeValue;
    public bool ChargeLostGradually => chargelostGradually;

    private void Update()
    {
        if (shot)
        { 
            Move();
            CheckDistanceLimit();
        }
    }

    public void OnWeaponDisabled(object sender, System.EventArgs e) => shotPool = null;

    public void Initialize(IObjectPool<ChargedProyectile> newShotPool)
    {
        shotPool = newShotPool;
        ResetChargeValues();
    }

    public void AddCharge(float value)
    {
        chargeValue = Mathf.Clamp01(chargeValue + value);
        UpdateChargeValues();
    }

    public void Deploy()
    {
        shot = true;
        startPosition = transform.position;
    }

    public void DisipateCharge()
    {
        ResetChargeValues();
    }    

    protected virtual void UpdateChargeValues()
    {
        power = Mathf.Lerp(startPower, endPower, chargeValue);
        speed = Mathf.Lerp(startSpeed, endSpeed, chargeValue);
    }

    protected virtual void Move()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    } 

    protected virtual void Explode()
    {
        ReturnToPool();
    }

    protected void ReturnToPool()
    {
        gameObject.SetActive(false);
        ResetChargeValues();

        if (shotPool == null) { Destroy(gameObject); }
        else { shotPool.Release(this); }
    }

    private void CheckDistanceLimit()
    {
        if (Vector2.Distance(transform.position, startPosition) > maxDistance) Explode();
    }

    private void ResetChargeValues()
    {
        chargeValue = 0;
        power = startPower;
        speed = startSpeed;
        shot = false;
    }
}
