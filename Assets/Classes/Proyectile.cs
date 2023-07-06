using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Proyectile : Shootable
{
    private float speed;

    void Update() => transform.Translate(direction * speed * Time.deltaTime);
    public void SetSpeed(float newSpeed) => speed = newSpeed;
}
