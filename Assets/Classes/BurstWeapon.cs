using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstWeapon : MainWeapon
{
    [SerializeField] private float fireRate = 0.3f;
    [SerializeField] private float shotSpeed = 12f;
    [SerializeField] public Proyectile ammoPrefab = null;

    private bool shootRequested;
    private bool coolingDown;
    private Coroutine shootCoroutine;

    public override void Fire()
    {
        if (shootCoroutine != null) { return; }
        
        shootRequested = true;
        shootCoroutine = StartCoroutine(BurstShot());
    }

    public override void StopFire()
    {
        shootRequested = false;
        //StopCoroutine(shootCoroutine);
        //shootCoroutine = null;
    }

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
        Proyectile shot = GameObject.Instantiate(ammoPrefab, transform.position, Quaternion.identity);
        shot.gameObject.SetActive(false);
        shot.SetSpeed(shotSpeed);
        shot.SetDirection(shotDirection);
        shot.gameObject.SetActive(true);
        OnAmmoExpended();
    }
}
