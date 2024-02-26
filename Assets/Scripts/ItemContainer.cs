using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]

public class ItemContainer : MonoBehaviour
{
    private MainWeapon item;

    public void SetItem(MainWeapon newItem)
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = newItem.ItemSprite;
        item = newItem;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        WeaponSystem player;

        if(collision.gameObject.TryGetComponent<WeaponSystem>(out player))
        {
            player.EquipWeapon(item) ;
        }
        //Container should be destroyed/consumed in the future. Left as is for testing.
    }
}
