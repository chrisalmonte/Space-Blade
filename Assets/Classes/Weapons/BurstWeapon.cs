using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BurstWeapon : MainWeapon
{
    [Header("Shot Properties")]
    [SerializeField] private float fireRate = 0.3f;
    [SerializeField] private float shotSpeed = 12f;
    [SerializeField] private float shotDistance = 50f;
    [SerializeField] private Proyectile ammoPrefab = null;

    [Header("Shot Pool Properties")]
    [SerializeField] [Min(10)] private int shotPoolDefaultSize = 50;
    [SerializeField] [Min(200)] private int shotPoolMaxSize = 400;

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

    public override void Fire()
    {
        if (shootCoroutine != null) { return; }

        shooting = true;
        shootCoroutine = StartCoroutine(BurstShot());
    }

    public override void StopFire() => shooting = false;

    public override void Deactivate()
    {
        StopFire();
        HaltCRoutines();
        if (shotPool != null) { shotPool.Clear(); }
        OnWeaponDisabled();
        gameObject.SetActive(false);
    }

    public override void Discard()
    {
        StopFire();
        HaltCRoutines();
        if (shotPool != null) { shotPool.Clear(); }
        OnWeaponDisabled();
        Destroy(gameObject);
    }

    private void OnWeaponDisabled() => WeaponDisabled?.Invoke(this, EventArgs.Empty);

    private IEnumerator BurstShot()
    {
        while (shooting)
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

    private void DeployShot()
    {
        if (shotPool == null) return;
        shotPool.Get();
        OnAmmoExpended();
    }

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
        shot.transform.SetPositionAndRotation(transform.position, shotRotation);
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
