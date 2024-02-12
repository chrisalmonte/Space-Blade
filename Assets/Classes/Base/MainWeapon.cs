using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MainWeapon : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private string weaponName = "New Weapon Name";
    [SerializeField] [Min(1)] private int power = 1;
    [SerializeField] [Min(1)] private int initialAmmo = 100;

    public event EventHandler AmmoExpended;
    protected Quaternion shotRotation;

    public string WeaponName => weaponName;
    public int Power => power;
    public int InitialAmmo => initialAmmo;

    protected void OnAmmoExpended() => AmmoExpended?.Invoke(this, EventArgs.Empty);
    public virtual void UpdateShotDirection(Vector2 newDirection) => shotRotation = Quaternion.FromToRotation(Vector3.right, newDirection);
    public virtual void Deactivate() => gameObject.SetActive(false);
    public abstract void Initialize();
    public abstract void Discard();
    public abstract void Fire();
    public abstract void StopFire();
}
