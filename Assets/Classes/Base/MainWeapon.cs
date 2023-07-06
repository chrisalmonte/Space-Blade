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
    protected Vector2 shotDirection;

    public string WeaponName => weaponName;
    public int Power => power;
    public int InitialAmmo => initialAmmo;

    public void UpdateShotDirection(Vector2 newDirection) => shotDirection = newDirection;
    public virtual void Discard() => Destroy(gameObject);
    public abstract void Fire();
    public abstract void StopFire();
    protected void OnAmmoExpended() => AmmoExpended?.Invoke(this, EventArgs.Empty);
}
