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
        shoot = input.actions["Shoot"];
        InitializeDeafultMWeapon();
    }

    public void EquipSpecialMWeapon(MainWeapon newWeapon)
    {
        UnequipCurrentMWeapon();
        //maybe attach to spawn point transform
        currentWeapon = GameObject.Instantiate(newWeapon, transform);
        remainingAmmo = newWeapon.InitialAmmo;
        currentWeapon.AmmoExpended += DecreaseAmmo;
    }

    private void Shoot()
    {
        currentWeapon.Fire();
    }

    private void CancelShoot()
    {
        currentWeapon.StopFire();
    }    

    public void IncreaseAmmo(int amount)
    {
        remainingAmmo += amount;
    }

    private void DecreaseAmmo(object sender, System.EventArgs e)
    {
        remainingAmmo -= 1;
        if (remainingAmmo < 1) { SwitchToDefaultMWeapon(); }
    }

    private void SwitchToDefaultMWeapon()
    {
        if (currentWeapon == defaultWeapon) { return; }

        UnequipCurrentMWeapon();
        currentWeapon = defaultWeapon;
        remainingAmmo = currentWeapon.InitialAmmo;
    }

    private void UnequipCurrentMWeapon()
    {
        if (currentWeapon == null) { return; }

        currentWeapon.AmmoExpended -= DecreaseAmmo;
        CancelShoot();
        if (currentWeapon != defaultWeapon) { currentWeapon.Discard(); }
        currentWeapon = null;
    }

    private void InitializeDeafultMWeapon()
    {
        defaultWeapon = GameObject.Instantiate(defaultWeapon, transform);
        SwitchToDefaultMWeapon();
    }
}
