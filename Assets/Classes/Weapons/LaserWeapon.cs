using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserWeapon : MainWeapon
{
    [SerializeField] private float ammoExpendRate = 0.05f;
    [SerializeField] private float activeAfterBtnUp = 0.4f;
    [SerializeField] private float turnSpeed = 2.1f;
    [SerializeField] [Range(1, 4)] private int turnSmoothLvl = 3;
    [SerializeField] private Laser laser = null;

    private bool firing;
    private Coroutine ammoExpendCoroutine;
    private Coroutine rotateCoroutine;

    public override void UpdateShotDirection(Vector2 newDirection)
    {
        shotRotation = Quaternion.LookRotation(Vector3.forward, newDirection) * Quaternion.Euler(0, 0, 90);

        if (!firing) return;

        if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
        rotateCoroutine = StartCoroutine(RotateLaser());
    }

    public override void Fire()
    {
        if (laser == null || ammoExpendCoroutine != null) { return; } //|| laserInstance.IsActve

        laser.transform.rotation = shotRotation;
        laser.Activate();
        
        firing = true;
        ammoExpendCoroutine = StartCoroutine(ExpendEnergy());
    }

    public override void StopFire()
    {
        //if (laserPrefab == null) { return; } // || laserInstance.IsActive
        
        laser.Deactivate();

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

        firing = false;
    }

    public override void Initialize()
    {
        laser.Activate();
    }   

    public override void Discard()
    {
        //Give directive to destroy laser when ending animation
        laser.Deactivate();
    }

    private IEnumerator ExpendEnergy()
    {
        while (gameObject.activeSelf) //laserinstance.IsActive
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
}
