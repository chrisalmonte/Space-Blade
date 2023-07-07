using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]

public class WeaponSystem : MonoBehaviour
{
    [SerializeField] private MainWeapon defaultWeapon;
    [SerializeField] private PlayerInput input;
    [SerializeField] private float atkDirectionInputDelay = 0.02f;

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
        shoot.Enable();
    }

    private void OnDisable()
    {
        CancelShoot();
        shoot.Disable();
    }

    public void OnSetAttackDirection(InputValue value) 
    {
        atkDirection = value.Get<Vector2>();

        if (atkDirection == Vector2.zero) { CancelShoot(); }
        
        else
        {
            if (atkDirectionDelayCoroutine == null) 
            {
                atkDirectionDelayCoroutine = StartCoroutine(InputRegisterDelay()); 
            }
        }        
    }    

    public void EquipSpecialMWeapon(MainWeapon newWeapon)
    {
        UnequipCurrentMWeapon();
        //if holding down shoot, stop and restart shot
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
        if (atkDirectionDelayCoroutine != null) 
        {
            StopCoroutine(atkDirectionDelayCoroutine);
            atkDirectionDelayCoroutine = null;
        }
        
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

    private IEnumerator InputRegisterDelay()
    {
        yield return new WaitForSeconds(atkDirectionInputDelay);
        currentWeapon.UpdateShotDirection(atkDirection);
        Shoot();
        atkDirectionDelayCoroutine = null;
    }
}
