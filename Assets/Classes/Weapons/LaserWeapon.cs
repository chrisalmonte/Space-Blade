using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserWeapon : MainWeapon
{
    [SerializeField] private float ammoExpendRate = 0.05f;
    [SerializeField] private float activeAfterCancel = 0.4f;
    [SerializeField] private Laser laser = null;

    private Coroutine ammoExpendCoroutine;
    private Coroutine cancelCoroutine;

    private void Update()
    {
        if (laser != null) { laser.transform.rotation = shotRotation; }
    }

    public override void Initialize()
    {
        gameObject.SetActive(true);
        laser.InitializeParameters();
    }

    public override void Fire()
    {
        if (shooting)
        {
            if (cancelCoroutine != null)
            {
                StopCoroutine(cancelCoroutine);
                cancelCoroutine = null;
            }

            return; 
        }
        
        laser.transform.rotation = shotRotation;
        laser.Activate();
        shooting = true;
        ammoExpendCoroutine = StartCoroutine(ExpendEnergy());
    }

    public override void StopFire()
    {
        if (!shooting || cancelCoroutine != null) { return; }
        cancelCoroutine = StartCoroutine(CancelCountdown());
    }

    private void TurnLaserOff()
    {
        HaltCoroutines();
        laser.Deactivate();
        shooting = false;
    }

    public override void Deactivate()
    {
        TurnLaserOff();
        gameObject.SetActive(false);
    }

    public override void Discard()
    {
        TurnLaserOff();
        Destroy(gameObject);
    }            

    private IEnumerator ExpendEnergy()
    {
        while (shooting)
        {
            yield return new WaitForSeconds(ammoExpendRate);
            OnAmmoExpended();
        }

        ammoExpendCoroutine = null;
    }

    private IEnumerator CancelCountdown()
    {
        yield return new WaitForSeconds(activeAfterCancel);
        TurnLaserOff();
        cancelCoroutine = null;
    }

    protected override void HaltCoroutines()
    {
        base.HaltCoroutines();

        if (ammoExpendCoroutine != null)
        {
            StopCoroutine(ammoExpendCoroutine);
            ammoExpendCoroutine = null;
        }

        if (cancelCoroutine != null)
        {
            StopCoroutine(cancelCoroutine);
            cancelCoroutine = null;
        }
    }
}
