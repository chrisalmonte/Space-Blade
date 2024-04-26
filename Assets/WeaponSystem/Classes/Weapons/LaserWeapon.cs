using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserWeapon : MainWeapon
{
    [Header("Laser Properties")]
    [SerializeField] private float ammoExpendRate = 0.05f;
    [SerializeField] private float activeAfterCancel = 0.4f;
    [SerializeField] private float maxLength = 25;
    [SerializeField] private Laser laser = null;

    private bool expendingAmmo;
    private bool deactivateAfterOff;
    private bool discardAfterOff;
    private Coroutine ammoExpendCoroutine;
    private Coroutine cancelCoroutine;

    private void Update()
    {
        if (laser != null) { laser.transform.rotation = shotRotation; }
    }

    public override void Initialize()
    {
        gameObject.SetActive(true);
        deactivateAfterOff = false;
        laser.InitializeParameters(basePower, maxLength);
        laser.LaserReady += OnLaserReady;
        laser.LaserRoutineEnded += OnLaserShutDown;
    }

    public override void Fire()
    {
        if(deactivateAfterOff) { return; }

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

    public override void Deactivate()
    {
        deactivateAfterOff = true;
        TurnLaserOff();
    }

    public override void Discard()
    {
        discardAfterOff = true;
        TurnLaserOff();
    }

    private void TurnLaserOff()
    {
        expendingAmmo = false;
        laser.Deactivate();
    }

    private void OnLaserReady(object sender, System.EventArgs e) => expendingAmmo = true;

    private void OnLaserShutDown(object sender, System.EventArgs e)
    {
        shooting = false;
        HaltCoroutines();

        if (discardAfterOff) { Destroy(gameObject); }
        else if (deactivateAfterOff)
        {
            laser.LaserReady -= OnLaserReady;
            laser.LaserRoutineEnded -= OnLaserShutDown;
            gameObject.SetActive(false);
        }
    }

    private IEnumerator ExpendEnergy()
    {
        while (expendingAmmo)
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

        StopAllCoroutines();
        ammoExpendCoroutine = null;
        cancelCoroutine = null;
    }
}
