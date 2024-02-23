using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ChargedProyectile : MonoBehaviour
{
    //Transfer charge behavior to weapon. Proyectile should mostly just consist of SetChargeLevel() and other properties
    //Also, make it so that "firing" is true when charge is decreasing gradually but player is not pressing fire.


    [SerializeField] [Min(0)] private float startPower = 5;
    [SerializeField] [Min(1)] private float endPower = 100;
    [SerializeField] [Min(0)] private float startSpeed = 15;
    [SerializeField] [Min(0)] private float endSpeed = 25;
    [SerializeField] private float chargeTime = 2;
    [SerializeField] private bool chargelostGradually;
    [SerializeField] [Range(0,1)] private float minChargeValue = 0.4f;
    [SerializeField] private float maxDistance = 50;

    private float power;
    private float speed;
    private float charge;
    private bool canBeShot;
    private bool shot;
    private Coroutine chargeCoroutine;
    private Coroutine dischargeCoroutine;
    private Vector2 startPosition;
    private IObjectPool<ChargedProyectile> shotPool;

    public EventHandler ChargeWasShot;
    private void OnChargeWasShot() => ChargeWasShot?.Invoke(this, EventArgs.Empty);

    public float ChargeValue() => charge;

    private void Update()
    {
        if (shot) { Move(); }
    }

    public void Initialize(IObjectPool<ChargedProyectile> newShotPool)
    {
        shotPool = newShotPool;
        ResetChargeValues();
    }    

    public void StartCharge()
    {
        if (chargeCoroutine != null) { return; }
        
        if (dischargeCoroutine != null)
        {
            StopCoroutine(dischargeCoroutine);
            dischargeCoroutine = null;
        }

        chargeCoroutine = StartCoroutine(Charge());
    }

    public void ReleaseCharge()
    {
        if (chargeCoroutine != null) 
        {
            StopCoroutine(chargeCoroutine);
            chargeCoroutine = null;
        }        

        if (canBeShot) { ShootCharge(); }
        else
        {
            if (chargelostGradually) { dischargeCoroutine = StartCoroutine(Discharge()); }
            else ResetChargeValues();
        }
    }

    public void HaltCharge()
    {
        HaltCoroutines();    
        ResetChargeValues();
    }

    private void ShootCharge()
    {
        OnChargeWasShot();
        shot = true;
    }

    public void ResetChargeValues()
    {
        charge = 0;
        power = startPower;
        speed = startSpeed;
        canBeShot = (charge >= minChargeValue);
        shot = false;
    }

    private void UpdateChargeValues()
    {
        power = Mathf.Lerp(startPower, endPower, charge);
        speed = Mathf.Lerp(startSpeed, endSpeed, charge);
        canBeShot = (charge >= minChargeValue);
    }

    protected virtual void Move()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, startPosition) > maxDistance) { Explode(); }
    }

    public void OnWeaponDisabled(object sender, System.EventArgs e) => shotPool = null;

    private IEnumerator Charge()
    {
        while (charge < 1)
        {
            charge = Mathf.Clamp01(charge + (Time.deltaTime / chargeTime));
            UpdateChargeValues();
            yield return null;
        }

        chargeCoroutine = null;
    }

    private IEnumerator Discharge()
    {
        while (charge > 0)
        {
            charge = Mathf.Clamp01(charge - (Time.deltaTime / chargeTime));
            UpdateChargeValues();
            yield return null;
        }

        dischargeCoroutine = null;
    }

    private void HaltCoroutines()
    {
        if (chargeCoroutine != null) 
        {
            StopCoroutine(chargeCoroutine);
            chargeCoroutine = null;
        }

        if (dischargeCoroutine != null)
        {
            StopCoroutine(dischargeCoroutine);
            dischargeCoroutine = null;
        }
    }

    public void Explode()
    {
        HaltCoroutines();
        gameObject.SetActive(false);

        if (shotPool == null) { Destroy(gameObject); }
        else { shotPool.Release(this); }
    }
}
