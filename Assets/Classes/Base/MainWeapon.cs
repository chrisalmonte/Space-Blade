using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MainWeapon : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private string weaponName = "New Weapon Name";
    [SerializeField] [Min(1)] private int power = 1;
    [SerializeField] [Min(1)] private int initialAmmo = 100;

    protected Vector2 shotDirection;

    public string WeaponName => weaponName;
    public int Power => power;
    public int InitialAmmo => initialAmmo;

    public void UpdateShotDirection(Vector2 newDirection) => shotDirection = newDirection;
    public abstract void Fire();
    public abstract void StopFire();
}
