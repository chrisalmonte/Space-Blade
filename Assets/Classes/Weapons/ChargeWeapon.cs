using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class ChargeWeapon : MainWeapon
{
    [Header("Weapon Properties")]
    [SerializeField] private float coolDownTime = 0.3f;
    [SerializeField] [Range(0, 1)] private float activeAfterCancel = 0.1f;
    [SerializeField] private ChargedProyectile ammoPrefab = null;

    [Header("Shot Pool Properties")]
    [SerializeField] [Min(5)] private int shotPoolDefaultSize = 5;
    [SerializeField] [Min(10)] private int shotPoolMaxSize = 10;

    private ChargedProyectile currentCharge;    
    private Coroutine coolDownCoroutine;
    private Coroutine cancelCoroutine;
    private IObjectPool<ChargedProyectile> shotPool;

    public event EventHandler WeaponDisabled;

    private void Update()
    {
        if (currentCharge != null) { currentCharge.transform.rotation = shotRotation; }
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
        if(currentCharge == null) { PrepareChargeShot(); }

        if(cancelCoroutine != null)
        { 
            StopCoroutine(cancelCoroutine);
            cancelCoroutine = null;
        }
        else
        {
            if (currentCharge.ChargeValue() == 0 && rotationCoroutine != null) 
            {
                StopCoroutine(rotationCoroutine);
                rotationCoroutine = null;
                shotRotation = targetRotation;
            }

            currentCharge.StartCharge();
            shooting = true;
        }        
    }

    public override void StopFire()
    {
        if (currentCharge == null || cancelCoroutine != null || !gameObject.activeSelf) { return; }
        cancelCoroutine = StartCoroutine(CancelCountdown());
    }

    public override void Deactivate()
    {
        currentCharge?.ReleaseCharge();
        HaltCoroutines();
        shotPool?.Clear();
        OnWeaponDisabled();
        gameObject.SetActive(false);
    }

    public override void Discard()
    {
        currentCharge?.ReleaseCharge();
        HaltCoroutines();
        shotPool?.Clear();
        OnWeaponDisabled();
        Destroy(gameObject);
    }

    private void OnShotDeployed(object sender, System.EventArgs e)
    {
        if(rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }

        currentCharge.ChargeWasShot -= OnShotDeployed;
        currentCharge.transform.parent = null;
        currentCharge = null;
        OnAmmoExpended();
    }

    private void OnWeaponDisabled() => WeaponDisabled?.Invoke(this, EventArgs.Empty);

    private IEnumerator WeaponCooldown()
    {
        yield return new WaitForSeconds(coolDownTime);
        coolDownCoroutine = null;
    }

    private IEnumerator CancelCountdown()
    {
        yield return new WaitForSeconds(activeAfterCancel);
        currentCharge.ReleaseCharge();
        shooting = false;
        cancelCoroutine = null;
    }

    protected override void HaltCoroutines()
    {
        base.HaltCoroutines();

        if (coolDownCoroutine != null)
        {
            StopCoroutine(coolDownCoroutine);
            coolDownCoroutine = null;
        }

        if (cancelCoroutine != null)
        {
            StopCoroutine(cancelCoroutine);
            cancelCoroutine = null;
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
