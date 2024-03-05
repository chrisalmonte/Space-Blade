using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugEnemy : MonoBehaviour, IDamageable
{
    [SerializeField] private float currentHP;

    public float HP => currentHP;

    public void Damage(float attackPower)
    {
        currentHP = Mathf.Max(currentHP - attackPower, 0);
        Debug.Log(currentHP);

        if(currentHP <= 0)
        {
            gameObject.SetActive(false);
            Invoke("Respawn", 5);
        }
    }

    private void Respawn()
    {
        gameObject.SetActive(true);
        currentHP = 10;
    }
}
