using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BurstWeapon : MainWeapon
{
    [Header("Firing Properties")]
    [SerializeField] private float fireRate = 0.3f;
    [SerializeField] private Proyectile ammoPrefab = null;

    [Header("Shot Pool Properties")]
    [SerializeField] [Min(10)] private int shotPoolDefaultSize = 50;
    [SerializeField] [Min(200)] private int shotPoolMaxSize = 400;

    private Coroutine shootCoroutine;
    private Coroutine coolDownCoroutine;
    private IObjectPool<Proyectile> shotPool;

    public event EventHandler WeaponDestroyed;
    private void OnWeaponDestroyed() => WeaponDestroyed?.Invoke(this, EventArgs.Empty);

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
        HaltCoroutines();
        gameObject.SetActive(false);
    }

    public override void Discard()
    {
        Deactivate();
        OnWeaponDestroyed();
        if (shotPool != null) { shotPool.Clear(); }
        Destroy(gameObject);
    }

    private IEnumerator BurstShot()
    {
        while (shooting)
        {
            if (coolDownCoroutine == null)
            {
                DeployShot();
                if (!shooting) { break; }
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

    protected override void HaltCoroutines()
    {
        base.HaltCoroutines();

        StopAllCoroutines();
        shootCoroutine = null;
        coolDownCoroutine = null;
    }

    private void DeployShot()
    {
        if (shotPool == null) return;
        shotPool.Get();
    }

    #region Shot Pooling
    Proyectile ShotInstance()
    {
        Proyectile shot = Instantiate(ammoPrefab, transform.position, Quaternion.identity);
        shot.Initialize(shotPool);
        shot.gameObject.SetActive(false);        
        return shot;
    }

    void OnReturnShotToPool(Proyectile shot)
    {
        shot.transform.position = Vector2.zero;
        WeaponDestroyed -= shot.OnWeaponDestroyed;
    }

    void OnTakeShotFromPool(Proyectile shot)
    {
        shot.transform.SetPositionAndRotation(transform.position, shotRotation);
        WeaponDestroyed += shot.OnWeaponDestroyed;
        shot.gameObject.SetActive(true);
        shot.Deploy();
        OnAmmoExpended();
    }

    void OnDestroyShotInstance(Proyectile shot)
    {
        Destroy(shot.gameObject);
    }
    #endregion
}
