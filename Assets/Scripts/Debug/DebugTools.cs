using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTools : MonoBehaviour
{
    public MainWeapon testWeapon;

    private WeaponSystem weaponSystem;

    // Start is called before the first frame update
    void Start()
    {
        weaponSystem = GetComponent<WeaponSystem>();
        Invoke("giveWeapon", 5);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void giveWeapon()
    {
        if (weaponSystem != null) weaponSystem.EquipSpecialMWeapon(testWeapon);
    }

}
