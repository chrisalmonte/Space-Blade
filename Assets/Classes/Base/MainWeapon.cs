using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MainWeapon : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private string weaponName = "New Weapon Name";
    [SerializeField] [Min(1)] private int initialAmmo = 100;
    [SerializeField] private Sprite itemSprite;

    public event EventHandler AmmoExpended;
    protected Quaternion shotRotation;

    public string WeaponName => weaponName;
    public int InitialAmmo => initialAmmo;
    public Sprite ItemSprite => itemSprite;

    protected void OnAmmoExpended() => AmmoExpended?.Invoke(this, EventArgs.Empty);
    public virtual void UpdateShotDirection(Vector2 newDirection) => shotRotation = Quaternion.FromToRotation(Vector3.right, newDirection);
    public abstract void Initialize();
    public abstract void Fire();
    public abstract void StopFire();
    public abstract void Deactivate();
    public abstract void Discard();
}
