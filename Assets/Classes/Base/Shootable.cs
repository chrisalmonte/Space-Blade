using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Shootable : MonoBehaviour
{
    protected Vector2 direction = Vector2.right;

    public void SetDirection(Vector2 newDirection) => direction = newDirection == Vector2.zero ? Vector2.right : newDirection;

    //public abstract void Destroy();
}
