using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class ChargeWeapon : MainWeapon
{
    [Header("Weapon Properties")]
    [SerializeField] private float coolDownTime = 0.3f;
    [SerializeField] private float turnSpeed = 6.5f;
    [SerializeField] [Range(1, 4)] private int turnSmoothLvl = 1;
    [SerializeField] private ChargedProyectile ammoPrefab = null;

    [Header("Internal Properties")]
    [SerializeField] [Min(5)] private int shotPoolDefaultSize = 5;
    [SerializeField] [Min(10)] private int shotPoolMaxSize = 10;

    private Vector2 directionCache;
    private ChargedProyectile currentCharge;    
    private Coroutine coolDownCoroutine;
    private Coroutine rotateCoroutine;
    private IObjectPool<ChargedProyectile> shotPool;

    public event EventHandler WeaponDisabled;

    public override void UpdateShotDirection(Vector2 newDirection)
    {  
        if (Vector2.Equals(newDirection, directionCache)) { return; }

        shotRotation = Quaternion.LookRotation(Vector3.forward, newDirection) * Quaternion.Euler(0, 0, 90);
        directionCache = newDirection;

        if (currentCharge != null) 
        {
            if (currentCharge.ChargeValue() == 0) { currentCharge.transform.rotation = shotRotation; }
            else
            {
                if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
                rotateCoroutine = StartCoroutine(RotateCharge());
            }            
        }
    }

    public override void Initialize()
    {
        gameObject.SetActive(true);

        if (shotPool != null) return;
        shotPool = new ObjectPool<ChargedProyectile>(ShotInstance, OnTakeShotFromPool, OnReturnShotToPool,
            OnDestroyShotInstance, true, shotPoolDefaultSize, shotPoolMaxSize);

        PrepareChargeShot();
    }    

    public override void Fire()
    {
        if(currentCharge == null) { return; }
        currentCharge.StartCharge();
    }

    public override void StopFire()
    {
        if (currentCharge == null) { return; }
        currentCharge.ReleaseCharge();
    }

    public override void Deactivate()
    {
        StopFire();
        HaltCoroutines();
        if (shotPool != null) { shotPool.Clear(); }
        OnWeaponDisabled();
        gameObject.SetActive(false);
    }

    public override void Discard()
    {
        StopFire();
        HaltCoroutines();
        if (shotPool != null) { shotPool.Clear(); }
        OnWeaponDisabled();
        Destroy(gameObject);
    }

    private void OnShotDeployed(object sender, System.EventArgs e)
    {
        if(rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
            rotateCoroutine = null;
        }

        currentCharge.ChargeWasShot -= OnShotDeployed;
        currentCharge.transform.parent = null;
        currentCharge = null;
        OnAmmoExpended();
        PrepareChargeShot();
    }

    private void OnWeaponDisabled() => WeaponDisabled?.Invoke(this, EventArgs.Empty);

    private IEnumerator WeaponCooldown()
    {
        yield return new WaitForSeconds(coolDownTime);
        coolDownCoroutine = null;
    }

    private IEnumerator RotateCharge()
    {
        float t = 0;
        Quaternion fromRot = currentCharge.transform.rotation;

        while (t < 1)
        {
            t += turnSpeed * Time.deltaTime;
            currentCharge.transform.rotation = Quaternion.Lerp(fromRot, shotRotation, Mathf.Pow(t, turnSmoothLvl));

            yield return null;
        }
    }

    private void HaltCoroutines()
    {
        if (coolDownCoroutine != null)
        {
            StopCoroutine(coolDownCoroutine);
            coolDownCoroutine = null;
        }

        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
            rotateCoroutine = null;
        }
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
        shot.ResetChargeValues();
        shot.transform.position = Vector2.zero;
        shot.gameObject.SetActive(false);
        WeaponDisabled -= shot.OnWeaponDisabled;
    }

    void OnTakeShotFromPool(ChargedProyectile shot)
    {
        shot.transform.rotation = shotRotation;
        shot.transform.position = transform.position;
        shot.transform.parent = transform;
        shot.ChargeWasShot += OnShotDeployed;
        WeaponDisabled += shot.OnWeaponDisabled;
        currentCharge = shot;
        shot.gameObject.SetActive(true);
    }

    void OnDestroyShotInstance(ChargedProyectile shot)
    {
        Destroy(shot.gameObject);
    }
    #endregion
}
