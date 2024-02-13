using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Proyectile : MonoBehaviour
{
    private float speed;
    private float maxDistance;
    private bool destroyAfterUse;
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

        if (destroyAfterUse || shotPool == null) { Destroy(gameObject); }
        else shotPool.Release(this);
    }

    private void Move()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, startPosition) > maxDistance) Deactivate();
    }

    public void OnWeaponDisabled(object sender, System.EventArgs e) => destroyAfterUse = true;
}
