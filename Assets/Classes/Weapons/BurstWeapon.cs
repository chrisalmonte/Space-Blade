using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BurstWeapon : MainWeapon
{
    [Header("Weapon Properties")]
    [SerializeField] private float fireRate = 0.3f;
    [SerializeField] private float shotSpeed = 12f;
    [SerializeField] private float shotDistance = 50f;
    [SerializeField] private Proyectile ammoPrefab = null;

    [Header("Internal Properties")]
    [SerializeField] [Min(10)] private int shotPoolDefaultSize = 50;
    [SerializeField] [Min(200)] private int shotPoolMaxSize = 400;

    private bool shootRequested;
    private Coroutine shootCoroutine;
    private Coroutine coolDownCoroutine;
    private IObjectPool<Proyectile> shotPool;

    public event EventHandler WeaponDisabled;

    public override void Initialize()
    {
        gameObject.SetActive(true);

        if (shotPool != null) return;
        shotPool = new ObjectPool<Proyectile>(ShotInstance, OnTakeShotFromPool, OnReturnShotToPool, 
            OnDestroyShotInstance, true, shotPoolDefaultSize, shotPoolMaxSize);
    }

    public override void Deactivate()
    {
        StopFire();
        HaltCRoutines();
        if (shotPool != null) { shotPool.Clear(); }
        OnWeaponDisabled();
        gameObject.SetActive(false);
    }

    public override void Fire()
    {
        if (shootCoroutine != null) { return; }

        shootRequested = true;
        shootCoroutine = StartCoroutine(BurstShot());
    }

    public override void StopFire() => shootRequested = false;

    private IEnumerator BurstShot()
    {
        while (shootRequested)
        {
            if (coolDownCoroutine == null)
            {
                DeployShot();
                coolDownCoroutine = StartCoroutine(WeaponCooldown());
            }
            yield return null;
        }

        shootCoroutine = null;
    }

    private IEnumerator WeaponCooldown()
    {
        yield return new WaitForSeconds(fireRate);
        coolDownCoroutine = null;
    }

    private void DeployShot()
    {
        if (shotPool == null) return;
        shotPool.Get();
        OnAmmoExpended();        
    }

    private void HaltCRoutines()
    {
        if (coolDownCoroutine != null)
        {
            StopCoroutine(coolDownCoroutine);
            coolDownCoroutine = null;
        }

        if (shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
            shootCoroutine = null;
        }
    }

    public override void Discard()
    {
        StopFire();
        HaltCRoutines();
        shotPool.Clear();
        OnWeaponDisabled();
        Destroy(gameObject);
    }

    protected virtual void OnWeaponDisabled() => WeaponDisabled.Invoke(this, EventArgs.Empty);

    #region Shot Pooling
    Proyectile ShotInstance()
    {
        Proyectile shot = GameObject.Instantiate(ammoPrefab, transform.position, Quaternion.identity);
        shot.Initialize(shotPool, shotSpeed, shotDistance);
        shot.gameObject.SetActive(false);        
        return shot;
    }

    void OnReturnShotToPool(Proyectile shot)
    {
        shot.transform.position = Vector2.zero;
        WeaponDisabled -= shot.OnWeaponDisabled;
    }

    void OnTakeShotFromPool(Proyectile shot)
    {
        shot.transform.rotation = shotRotation;
        shot.transform.position = transform.position;
        shot.gameObject.SetActive(true);
        WeaponDisabled += shot.OnWeaponDisabled;
        OnAmmoExpended();
    }

    void OnDestroyShotInstance(Proyectile shot)
    {
        Destroy(shot.gameObject);
    }
    #endregion
}
