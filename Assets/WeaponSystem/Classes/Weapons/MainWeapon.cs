using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PrefabIDComponent))]

public abstract class MainWeapon : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private string weaponName = "New Weapon Name";
    [SerializeField] [Min(1)] private int initialAmmo = 100;

    [Header("Shot Direction Rotation")]
    [Tooltip("If false, direction changes will be instant")]
    [SerializeField]  private bool rotateGradually;
    [SerializeField] [Min(0)] private float rotationSpeed = 0;
    [SerializeField] [Range(1, 4)] private int rotationSmoothLvl = 1;

    public event EventHandler AmmoExpended;
    protected bool shooting;
    protected Quaternion shotRotation;
    protected Quaternion targetRotation;
    protected Coroutine rotationCoroutine;
    private Vector2 directionCache;

    public string WeaponName => weaponName;
    public string ID => GetComponent<PrefabIDComponent>().ObjectID;
    public int InitialAmmo => initialAmmo;
    public Sprite ItemSprite => itemSprite;

    protected void OnAmmoExpended() => AmmoExpended?.Invoke(this, EventArgs.Empty);

    public virtual void UpdateShotDirection(Vector2 newDirection) 
    {
        if (Vector2.Equals(newDirection, directionCache)) { return; }

        directionCache = newDirection;

        if (rotateGradually && shooting)
        {
            targetRotation = Quaternion.LookRotation(Vector3.forward, newDirection) * Quaternion.Euler(0, 0, 90);
            if (rotationCoroutine != null) { StopCoroutine(rotationCoroutine); }
            rotationCoroutine = StartCoroutine(RotateShot());
        }
        else { shotRotation = Quaternion.FromToRotation(Vector3.right, newDirection); }
    }

    protected virtual IEnumerator RotateShot()
    {
        if (rotationSpeed <= 0 ) { shotRotation = targetRotation; }
        else
        {
            float t = 0;
            Quaternion fromRotation = shotRotation;

            while (t < 1)
            {
                t = Mathf.Clamp01(t + (rotationSpeed * Time.deltaTime));
                shotRotation = Quaternion.Lerp(fromRotation, targetRotation, Mathf.Pow(t, rotationSmoothLvl));
                yield return null;
            }
        }        

        rotationCoroutine = null;
    }

    protected void CancelRotation()
    {
        if (rotationCoroutine == null) { return; }

        StopCoroutine(rotationCoroutine);
        rotationCoroutine = null;
        shotRotation = targetRotation;
        directionCache = Vector2.zero;
    }

    protected virtual void HaltCoroutines()
    {
        CancelRotation();
    }    

    public abstract void Initialize();
    public abstract void Fire();
    public abstract void StopFire();
    public abstract void Deactivate();
    public abstract void Discard();    
}
