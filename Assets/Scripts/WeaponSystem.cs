using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]

public class WeaponSystem : MonoBehaviour
{
    [SerializeField] private MainWeapon defaultWeapon;
    [SerializeField] private PlayerInput input;

    private InputAction shoot;
    private MainWeapon currentWeapon;
    private int remainingAmmo;

    public void OnSetAttackDirection(InputValue value) => currentWeapon.UpdateShotDirection(value.Get<Vector2>());

    private void OnEnable()
    {
        shoot.performed += ctx => Shoot();
        shoot.canceled += ctx => CancelShoot();
    }

    private void OnDisable()
    {
        shoot.performed -= ctx => Shoot();
        shoot.canceled -= ctx => CancelShoot();
    }

    private void Awake()
    {
        defaultWeapon = GameObject.Instantiate(defaultWeapon, transform);
        shoot = input.actions["Shoot"];
        SwitchToDefaultMWeapon();
    }

    public void SetSpecialMWeapon(MainWeapon newWeapon)
    {
        //Stop current shot and resume if it was being held
        //destroy last weapon if not null
        //maybe attach to spawn point transform
        currentWeapon = GameObject.Instantiate(newWeapon, transform);
        remainingAmmo = newWeapon.InitialAmmo;
    }

    private void Shoot()
    {
        currentWeapon.Fire();
    }

    private void CancelShoot()
    {
        currentWeapon.StopFire();
    }

    private void SwitchToDefaultMWeapon()
    {
        currentWeapon = defaultWeapon;
        remainingAmmo = 999;
    }

    private void DecreaseAmmo()
    {
        remainingAmmo -= 1;

        if (remainingAmmo < 1) {
            CancelShoot();
            SwitchToDefaultMWeapon();
        }
    }

    public void IncreaseAmmo(int amount)
    {
        remainingAmmo += amount;
    } 
}
