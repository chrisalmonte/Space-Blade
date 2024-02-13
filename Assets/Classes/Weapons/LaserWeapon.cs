using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserWeapon : MainWeapon
{
    [SerializeField] private float ammoExpendRate = 0.05f;
    [SerializeField] private float activeAfterCancel = 0.4f;
    [SerializeField] private float turnSpeed = 2.1f;
    [SerializeField] [Range(1, 4)] private int turnSmoothLvl = 3;
    [SerializeField] private Laser laser = null;

    private bool firing = false;
    private Coroutine ammoExpendCoroutine;
    private Coroutine rotateCoroutine;
    private Coroutine cancelCoroutine;

    public override void Fire()
    {
        if (firing)
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
        firing = true;
        ammoExpendCoroutine = StartCoroutine(ExpendEnergy());
    }

    public override void StopFire()
    {
        if (!firing || cancelCoroutine != null) return;
        cancelCoroutine = StartCoroutine(CancelCountdown());
    }

    public override void UpdateShotDirection(Vector2 newDirection)
    {
        shotRotation = Quaternion.LookRotation(Vector3.forward, newDirection) * Quaternion.Euler(0, 0, 90);

        if (!firing) return;

        if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
        rotateCoroutine = StartCoroutine(RotateLaser());
    }

    public override void Initialize()
    {
        laser.InitializeParameters();
    }   

    public override void Discard()
    {
        TurnLaserOff();
    }

    private void TurnLaserOff()
    {
        HaltCoroutines();
        laser.Deactivate();
        firing = false;
    }    

    private IEnumerator ExpendEnergy()
    {
        while (firing) //laserinstance.IsActive
        {
            yield return new WaitForSeconds(ammoExpendRate);
            OnAmmoExpended();
        }

        ammoExpendCoroutine = null;
    }

    private IEnumerator RotateLaser()
    {
        float t = 0;
        Quaternion fromRot = laser.transform.rotation;

        while (firing && t < 1)
        {
            t += turnSpeed * Time.deltaTime;
            laser.transform.rotation = Quaternion.Lerp(fromRot, shotRotation, Mathf.Pow(t, turnSmoothLvl));
            
            yield return null;
        }
    }

    private IEnumerator CancelCountdown()
    {
        yield return new WaitForSeconds(activeAfterCancel);
        TurnLaserOff();
        cancelCoroutine = null;
    }

    private void HaltCoroutines()
    {
        if (ammoExpendCoroutine != null)
        {
            StopCoroutine(ammoExpendCoroutine);
            ammoExpendCoroutine = null;
        }

        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
            rotateCoroutine = null;
        }

        if (cancelCoroutine != null)
        {
            StopCoroutine(cancelCoroutine);
            cancelCoroutine = null;
        }
    }
}
