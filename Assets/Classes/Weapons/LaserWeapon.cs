using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserWeapon : MainWeapon
{
    [SerializeField] private float damageRate = 0.2f;
    [SerializeField] private float ammoExpendRate = 0.05f;
    [SerializeField] private float shotDistance = 50f;
    [SerializeField] private float turnSpeed = 2.1f;
    [SerializeField] [Range(1, 4)] private int turnSmoothLvl = 3;
    [SerializeField] private float cooldownTime = 2;
    [SerializeField] private Laser laser = null;

    private bool firing;
    private Coroutine ammoExpendCoroutine;
    private Coroutine rotateCoroutine;

    public override void UpdateShotDirection(Vector2 newDirection)
    {
        base.UpdateShotDirection(newDirection);
        
        if (!firing) return;

        if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
        rotateCoroutine = StartCoroutine(RotateLaser());
    }

    public override void Fire()
    {
        if (laser == null || ammoExpendCoroutine != null) { return; } //|| laswerInstance.IsActve

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

        Vector2.SignedAngle(laser.transform.rotation.eulerAngles, shotRotation.eulerAngles);

        while (firing)
        {
            t += turnSpeed * Time.deltaTime;
            laser.transform.rotation = Quaternion.Lerp(fromRot, shotRotation, Mathf.Pow(t, turnSmoothLvl));
            
            yield return null;
        }
    }
}
