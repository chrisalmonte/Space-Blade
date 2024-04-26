using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]

public class WeaponSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MainWeapon defaultWeapon;
    [SerializeField] private PlayerInput input;

    [Header("Internal Properties")]
    [SerializeField] private float atkDirectionInputDelay = 0.02f;
    [SerializeField] private int maxAmmoValue = 9999;

    private int remainingAmmo;
    private Vector2 atkDirection;
    private MainWeapon currentWeapon;
    private Coroutine atkDirectionDelayCoroutine;
    private InputAction shoot;

    public EventHandler AmmoChanged;
    private void OnAmmoChanged() 
    {
        AmmoChanged?.Invoke(this, EventArgs.Empty);
    } 

    private void Awake()
    {
        shoot = input.actions["SetAttackDirection"];
        InitializeDeafultWeapon();
    }

    private void OnEnable()
    {
        shoot.performed += SetAttackDirection;
        shoot.canceled += SetAttackDirection;
        currentWeapon.Initialize();
    }

    private void OnDisable()
    {
        currentWeapon.Deactivate();
        shoot.performed -= SetAttackDirection;
        shoot.canceled -= SetAttackDirection;
    }

    public void SetAttackDirection(InputAction.CallbackContext ctx) 
    {
        if (ctx.canceled) 
        { 
            CancelShoot();
            return;
        }

        atkDirection = shoot.ReadValue<Vector2>();

        if (atkDirectionDelayCoroutine == null)
        {
            atkDirectionDelayCoroutine = StartCoroutine(InputRegisterDelay());
        }
    }

    private IEnumerator InputRegisterDelay()
    {
        yield return new WaitForSeconds(atkDirectionInputDelay);
        currentWeapon.UpdateShotDirection(atkDirection);
        Shoot();
        atkDirectionDelayCoroutine = null;
    }

    private void Shoot()
    {
        if (currentWeapon == null) return;
        currentWeapon.Fire();
    }

    private void CancelShoot()
    {
        if (currentWeapon == null) return;

        if (atkDirectionDelayCoroutine != null)
        {
            StopCoroutine(atkDirectionDelayCoroutine);
            atkDirectionDelayCoroutine = null;
        }

        atkDirection = Vector2.zero;
        currentWeapon.StopFire();
    }

    public void IncreaseAmmo(int amount)
    {
        remainingAmmo = Mathf.Min(remainingAmmo + amount, maxAmmoValue);
        OnAmmoChanged();
    }

    private void DecreaseAmmo(object sender, System.EventArgs e)
    {
        remainingAmmo -= 1;
        OnAmmoChanged();
        if (remainingAmmo < 1) { SwitchToDefaultWeapon(); }
    }

    public void EquipWeapon(MainWeapon obtainedWeapon)
    {
        if (currentWeapon != null) 
        {
            if (currentWeapon.ID.Equals(obtainedWeapon.ID))
            {
                IncreaseAmmo(obtainedWeapon.InitialAmmo);
                return;
            }

            if (defaultWeapon.ID.Equals(obtainedWeapon.ID))
            {
                SwitchToDefaultWeapon();
                return;
            }

            CancelShoot();
            if (currentWeapon == defaultWeapon) { currentWeapon.Deactivate(); }
            else { currentWeapon.Discard(); }
        }

        //maybe attach to spawn point transform
        currentWeapon = GameObject.Instantiate(obtainedWeapon, transform);
        SetWeaponProperties();
        currentWeapon.AmmoExpended += DecreaseAmmo;
        CheckResumeShoot();
    }

    private void SwitchToDefaultWeapon()
    {
        if (currentWeapon == defaultWeapon) { return; }

        if (currentWeapon != null)
        {
            CancelShoot();
            currentWeapon.AmmoExpended -= DecreaseAmmo;
            currentWeapon.Discard();
        }

        currentWeapon = defaultWeapon;
        SetWeaponProperties();
        CheckResumeShoot();
    }

    private void SetWeaponProperties()
    {
        remainingAmmo = currentWeapon.InitialAmmo;
        OnAmmoChanged();
        currentWeapon.Initialize();
    }

    private void CheckResumeShoot()
    {
        if (shoot.ReadValue<Vector2>() == Vector2.zero) { return; }

        atkDirection = shoot.ReadValue<Vector2>();
        if (atkDirectionDelayCoroutine == null) { atkDirectionDelayCoroutine = StartCoroutine(InputRegisterDelay()); }
    }
    
    private void InitializeDeafultWeapon()
    {
        defaultWeapon = GameObject.Instantiate(defaultWeapon, transform);
        SwitchToDefaultWeapon();
    }
}
