using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTools : MonoBehaviour
{
    public MainWeapon testWeapon;
    public MainWeapon testWeapon2;
    public MainWeapon testWeapon3;

    private WeaponSystem weaponSystem;

    // Start is called before the first frame update
    void Start()
    {
        weaponSystem = GetComponent<WeaponSystem>();
        Invoke("giveWeapon", 5);
        Invoke("giveWeapon2", 20);
        Invoke("giveWeapon3", 50);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void giveWeapon()
    {
        if (weaponSystem != null) weaponSystem.EquipWeapon(testWeapon);
    }
    private void giveWeapon2()
    {
        if (weaponSystem != null) weaponSystem.EquipWeapon(testWeapon2);
    }
    
    private void giveWeapon3()
    {
        if (weaponSystem != null) weaponSystem.EquipWeapon(testWeapon3);
    }

}
