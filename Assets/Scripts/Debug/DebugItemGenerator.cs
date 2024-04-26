using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugItemGenerator : MonoBehaviour
{
    public Vector2 startPosition;
    public ItemContainer containerPrefab;
    public MainWeapon[] spawnables;

    private void Start()
    {
        foreach (var item in spawnables)
        {
            ItemContainer container = GameObject.Instantiate(containerPrefab, startPosition, Quaternion.identity);
            container.SetItem(item);

            startPosition.x += 3f;
        }
    }
}
