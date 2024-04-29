using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class ChargeWeapon : MainWeapon
{
    [Header("Charge Properties")]
    [SerializeField] private float chargedPower = 5;
    [SerializeField] private float baseShotSpeed = 25;
    [SerializeField] private float chargedShotSpeed = 70;
    [SerializeField] private float chargeTime = 2;
    [SerializeField] private bool chargelostGradually;
    [SerializeField] [Range(0, 1)] private float chargeToShoot = 0.4f;

    [Header("Weapon Properties")]
    [SerializeField] [Range(0, 1)] private float activeAfterCancel = 0.1f;
    [SerializeField] private ProyectileCharged ammoPrefab = null;

    [Header("Shot Pool Properties")]
    [SerializeField] [Min(5)] private int shotPoolDefaultSize = 5;
    [SerializeField] [Min(10)] private int shotPoolMaxSize = 10;

    private ProyectileCharged heldShot;
    private Coroutine cancelCoroutine;
    private Coroutine chargeCoroutine;
    private Coroutine dischargeCoroutine;
    private IObjectPool<Proyectile> shotPool;

    public event EventHandler WeaponDisabled;

    private void Update()
    {
        if (heldShot != null) { heldShot.transform.rotation = transform.rotation * shotRotation; }
    }

    public override void Initialize()
    {
        gameObject.SetActive(true);

        if (shotPool != null) return;
        shotPool = new ObjectPool<Proyectile>(ShotInstance, OnTakeShotFromPool, OnReturnShotToPool,
            OnDestroyShotInstance, true, shotPoolDefaultSize, shotPoolMaxSize);
    }

    public override void Fire()
    {
        if(heldShot == null) { PrepareChargeShot(); }

        if(cancelCoroutine != null)
        { 
            StopCoroutine(cancelCoroutine);
            cancelCoroutine = null;
        }
        else { StartCharge(); }        
    }

    public override void StopFire()
    {
        if (heldShot == null || cancelCoroutine != null) { return; }
        cancelCoroutine = StartCoroutine(CancelCountdown());
    }

    public override void Deactivate()
    {
        HaltCoroutines();

        if (heldShot != null)
        {
            if (heldShot.ChargeValue >= chargeToShoot) { ShootCharge(); }
            else { heldShot.DisipateCharge(); }
            shooting = false;
        }   
        
        gameObject.SetActive(false);
    }

    public override void Discard()
    {
        Deactivate();
        OnWeaponDestroyed();
        shotPool?.Clear();
        Destroy(gameObject);
    }

    private void OnWeaponDestroyed() => WeaponDisabled?.Invoke(this, EventArgs.Empty);

    private void StartCharge()
    {
        if (chargeCoroutine != null) { return; }

        if (dischargeCoroutine != null)
        {
            StopCoroutine(dischargeCoroutine);
            dischargeCoroutine = null;
        }

        chargeCoroutine = StartCoroutine(Charge());
        shooting = true;
    }        

    public void ReleaseCharge()
    {
        if (heldShot == null) { return; }

        if (chargeCoroutine != null)
        {
            StopCoroutine(chargeCoroutine);
            chargeCoroutine = null;
        }

        if (heldShot.ChargeValue >= chargeToShoot) { ShootCharge(); }
        else
        {
            if (chargelostGradually) { dischargeCoroutine = StartCoroutine(Discharge()); }
            else 
            { 
                heldShot.DisipateCharge();
                shooting = false;
            }
        }
    }

    private void ShootCharge()
    {
        heldShot.transform.parent = null;
        heldShot.Deploy();
        heldShot.DestroyedWhileHeld -= HeldShotDestroyed;
        heldShot = null;
        CancelRotation();
        OnAmmoExpended();
        shooting = false;
    }

    private void UpdateChargeValues()
    {
        float power = Mathf.Lerp(basePower, chargedPower, heldShot.ChargeValue);
        float speed = Mathf.Lerp(baseShotSpeed, chargedShotSpeed, heldShot.ChargeValue);
        heldShot.UpdateShotProperties(power, speed);
    }

    private IEnumerator Charge()
    {
        if (chargeTime <= 0)
        {
            heldShot.AddCharge(1);
        }

        else
        {
            while (heldShot.ChargeValue < 1)
            {
                heldShot.AddCharge(Time.deltaTime / chargeTime);
                UpdateChargeValues();
                yield return null;
            }
        }

        chargeCoroutine = null;
    }

    private IEnumerator Discharge()
    {
        if (chargeTime <= 0)
        {
            heldShot.AddCharge(-1);
        }

        else
        {
            while (heldShot.ChargeValue > 0)
            {
                heldShot.AddCharge(-Time.deltaTime / chargeTime);
                UpdateChargeValues();
                yield return null;
            }
        }

        CancelRotation();
        shotRotation = targetRotation;
        shooting = false;
        dischargeCoroutine = null;
    }

    private IEnumerator CancelCountdown()
    {
        yield return new WaitForSeconds(activeAfterCancel);
        ReleaseCharge();
        cancelCoroutine = null;
    }

    protected override void HaltCoroutines()
    {
        base.HaltCoroutines();
        
        StopAllCoroutines();
        dischargeCoroutine = null;
        chargeCoroutine = null;
        cancelCoroutine = null;
        shooting = false;
    }

    private void HeldShotDestroyed(object sender, System.EventArgs e)
    {
        HaltCoroutines();
        heldShot.transform.parent = null;
        heldShot.DestroyedWhileHeld -= HeldShotDestroyed;
        heldShot = null;
        shooting = false;
        OnAmmoExpended();
    }

    private void PrepareChargeShot()
    {
        if (shotPool == null) return;
        shotPool.Get();
    }

    #region Shot Pooling
    ProyectileCharged ShotInstance()
    {
        ProyectileCharged shot = Instantiate(ammoPrefab, transform.position, Quaternion.identity);
        shot.Initialize(shotPool, basePower, baseShotSpeed);
        shot.gameObject.SetActive(false);
        return shot;
    }

    void OnReturnShotToPool(Proyectile shot)
    {
        shot.transform.position = Vector2.zero;
        WeaponDisabled -= shot.OnWeaponDestroyed;
    }

    void OnTakeShotFromPool(Proyectile shot)
    {
        shot.transform.SetPositionAndRotation(transform.position, transform.rotation * shotRotation);
        shot.transform.parent = transform;
        (shot as ProyectileCharged).DestroyedWhileHeld += HeldShotDestroyed;
        WeaponDisabled += shot.OnWeaponDestroyed;
        heldShot = (shot as ProyectileCharged);
        shot.gameObject.SetActive(true);
    }

    void OnDestroyShotInstance(Proyectile shot)
    {
        Destroy(shot.gameObject);
    }
    #endregion
}
