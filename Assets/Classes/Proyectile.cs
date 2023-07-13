using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Proyectile : Shootable
{
    private float speed;
    private float maxDistance;
    private Vector2 startPosition;
    private IObjectPool<Proyectile> shotPool;

    void Update()
    {
        Move();
    }

    public void Initialize(IObjectPool<Proyectile> weaponShotPool, float newSpeed, float newMaxDistance)
    {
        shotPool = weaponShotPool;
        speed = newSpeed;
        maxDistance = newMaxDistance;
    }

    public void Deactivate() {
        gameObject.SetActive(false);
        direction = Vector2.zero;

        if (shotPool != null) { shotPool.Release(this); }
        else Destroy(gameObject);
    }

    private void Move()
    {
        transform.Translate(direction * speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, startPosition) > maxDistance) Deactivate();
    }
}
