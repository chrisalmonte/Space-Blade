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
    private MainWeapon currentWeapon;
    private Coroutine atkDirectionDelayCoroutine;
    private Vector2 atkDirection;
    private InputAction shoot;

    private void Awake()
    {
        shoot = input.actions["SetAttackDirection"];
        InitializeDeafultMWeapon();
    }

    private void OnEnable()
    {
        shoot.performed += SetAttackDirection;
        shoot.canceled += SetAttackDirection;
    }

    private void OnDisable()
    {
        CancelShoot();
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
        currentWeapon.Fire();
    }

    private void CancelShoot()
    {
        if (atkDirectionDelayCoroutine != null)
        {
            StopCoroutine(atkDirectionDelayCoroutine);
            atkDirectionDelayCoroutine = null;
        }

        atkDirection = Vector2.zero;
        currentWeapon.StopFire();
    }

    public void EquipSpecialMWeapon(MainWeapon newWeapon)
    {
        //if holding down shoot, stop and restart shot
        //maybe attach to spawn point transform
        UnequipCurrentMWeapon();
        currentWeapon = GameObject.Instantiate(newWeapon, transform);
        SetMWeaponProperties();
        currentWeapon.AmmoExpended += DecreaseAmmo;
    }
    
    private void SetMWeaponProperties()
    {
        remainingAmmo = currentWeapon.InitialAmmo;
        currentWeapon.Initialize();
    }

    public void IncreaseAmmo(int amount)
    {
        remainingAmmo =  Mathf.Min(remainingAmmo + amount,maxAmmoValue);
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
        SetMWeaponProperties();
    }

    private void UnequipCurrentMWeapon()
    {
        if (currentWeapon == null) { return; }

        CancelShoot();

        if (currentWeapon != defaultWeapon) {
            currentWeapon.AmmoExpended -= DecreaseAmmo;
            currentWeapon.Discard();
        }

        else currentWeapon.Deactivate();

        currentWeapon = null;
    }

    private void InitializeDeafultMWeapon()
    {
        defaultWeapon = GameObject.Instantiate(defaultWeapon, transform);
        SwitchToDefaultMWeapon();
    }
}
