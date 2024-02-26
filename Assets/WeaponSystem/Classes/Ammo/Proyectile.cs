using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Proyectile : MonoBehaviour
{
    [SerializeField] private float power = 1;
    [SerializeField] private float speed = 25;
    [SerializeField] private float maxDistance = 30;

    private Vector2 startPosition;
    private IObjectPool<Proyectile> shotPool;
    
    protected virtual void OnEnable()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        Move();
        CheckDistanceLimit();
    }

    public void Initialize(IObjectPool<Proyectile> weaponShotPool)
    {
        shotPool = weaponShotPool;
    }

    protected virtual void Move()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);        
    }

    protected virtual void Explode() 
    {
        ReturnToPool();
    }
    
    private void ReturnToPool()
    {
        gameObject.SetActive(false);
        if (shotPool == null) { Destroy(gameObject); }
        else shotPool.Release(this);
    }

    private void CheckDistanceLimit()
    {
        if (Vector2.Distance(transform.position, startPosition) > maxDistance) Explode();
    }

    protected virtual void Damage(/*Recieve Damageable/atackable class to send attack data*/)
    {

    }

    //OnTrigger/CollisionEnter -> Damage (if possible), then explode(always).

    public void OnWeaponDestroyed(object sender, System.EventArgs e) => shotPool = null;    
}
