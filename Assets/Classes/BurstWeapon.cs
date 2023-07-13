using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BurstWeapon : MainWeapon
{
    [SerializeField] private float fireRate = 0.3f;
    [SerializeField] private float shotSpeed = 12f;
    [SerializeField] private float shotDistance = 50f;
    [SerializeField] private Proyectile ammoPrefab = null;

    [Header("Technical")]
    [SerializeField] [Min(10)] private int shotPoolDefaultSize = 50;
    [SerializeField] [Min(200)] private int shotPoolMaxSize = 400;

    private bool shootRequested;
    private bool coolingDown;
    private Coroutine shootCoroutine;
    private IObjectPool<Proyectile> shotPool;

    public override void Initialize()
    {
        gameObject.SetActive(true);
        if (shotPool != null) return;
        shotPool = new ObjectPool<Proyectile>(ShotInstance, OnTakeShotFromPool, OnReturnShotToPool, OnDestroyShotInstance, true, shotPoolDefaultSize, shotPoolMaxSize);
    }

    public override void Deactivate()
    {
        if (shotPool != null) shotPool.Clear();
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
            if (!coolingDown)
            {
                DeployShot();
                StartCoroutine(WeaponCooldown());
            }
            yield return null;
        }

        shootCoroutine = null;
    }

    private IEnumerator WeaponCooldown()
    {
        coolingDown = true;
        yield return new WaitForSeconds(fireRate);
        coolingDown = false;
    }

    private void DeployShot()
    {
        shotPool.Get();
        OnAmmoExpended();        
    }
    
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
    }

    void OnTakeShotFromPool(Proyectile shot)
    {
        shot.SetDirection(shotDirection);
        shot.transform.position = transform.position;
        shot.gameObject.SetActive(true);
        OnAmmoExpended();
    }

    void OnDestroyShotInstance(Proyectile shot)
    {
        if (!shot.gameObject.activeSelf) { Destroy(shot.gameObject); }
    }

    public override void Discard()
    {
        shotPool.Clear();
        Destroy(gameObject);
    }
}
