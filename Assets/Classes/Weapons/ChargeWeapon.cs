using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class ChargeWeapon : MainWeapon
{
    [Header("Weapon Properties")]
    [SerializeField] [Range(0, 1)] private float activeAfterCancel = 0.1f;
    [SerializeField] private ChargedProyectile ammoPrefab = null;

    [Header("Shot Pool Properties")]
    [SerializeField] [Min(5)] private int shotPoolDefaultSize = 5;
    [SerializeField] [Min(10)] private int shotPoolMaxSize = 10;

    private ChargedProyectile heldShot;
    private Coroutine cancelCoroutine;
    private Coroutine chargeCoroutine;
    private Coroutine dischargeCoroutine;
    private IObjectPool<ChargedProyectile> shotPool;

    public event EventHandler WeaponDisabled;

    private void Update()
    {
        if (heldShot != null) { heldShot.transform.rotation = shotRotation; }
    }

    public override void Initialize()
    {
        gameObject.SetActive(true);

        if (shotPool != null) return;
        shotPool = new ObjectPool<ChargedProyectile>(ShotInstance, OnTakeShotFromPool, OnReturnShotToPool,
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
            if (heldShot.ChargeValue() >= heldShot.MinCharge()) { ShootCharge(); }
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

        if (heldShot.ChargeValue() >= heldShot.MinCharge()) { ShootCharge(); }
        else
        {
            if (heldShot.ChargeLostGradually()) { dischargeCoroutine = StartCoroutine(Discharge()); }
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

    private IEnumerator Charge()
    {
        float chargeTime = heldShot.ChargeTime();

        if (heldShot.ChargeTime() <= 0)
        {
            heldShot.AddCharge(1);
        }

        else
        {
            while (heldShot.ChargeValue() < 1)
            {
                heldShot.AddCharge(Time.deltaTime / chargeTime);
                yield return null;
            }
        }

        chargeCoroutine = null;
    }

    private IEnumerator Discharge()
    {
        float chargeTime = heldShot.ChargeTime();

        if (heldShot.ChargeTime() <= 0)
        {
            heldShot.AddCharge(-1);
        }

        else
        {
            while (heldShot.ChargeValue() > 0)
            {
                heldShot.AddCharge(-Time.deltaTime / chargeTime);
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
    ChargedProyectile ShotInstance()
    {
        ChargedProyectile shot = GameObject.Instantiate(ammoPrefab, transform.position, Quaternion.identity);
        shot.Initialize(shotPool);
        shot.gameObject.SetActive(false);
        return shot;
    }

    void OnReturnShotToPool(ChargedProyectile shot)
    {
        shot.transform.position = Vector2.zero;
        WeaponDisabled -= shot.OnWeaponDisabled;
    }

    void OnTakeShotFromPool(ChargedProyectile shot)
    {
        shot.transform.SetPositionAndRotation(transform.position, shotRotation);
        shot.transform.parent = transform;
        shot.DestroyedWhileHeld += HeldShotDestroyed;
        WeaponDisabled += shot.OnWeaponDisabled;
        heldShot = shot;
        shot.gameObject.SetActive(true);
    }

    void OnDestroyShotInstance(ChargedProyectile shot)
    {
        Destroy(shot.gameObject);
    }
    #endregion
}
