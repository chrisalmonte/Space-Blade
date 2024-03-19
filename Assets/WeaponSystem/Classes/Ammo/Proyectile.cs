using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Proyectile : MonoBehaviour
{
    [SerializeField] private float maxDistance = 30;

    private bool hasCollided;
    protected float power;
    protected float speed;
    private IObjectPool<Proyectile> shotPool;
    protected Vector2 startPosition;

    public virtual void Deploy()
    {
        startPosition = transform.position;
    }

    protected virtual void Update()
    {
        Move();
        CheckDistanceLimit();
    }

    public void Initialize(IObjectPool<Proyectile> weaponShotPool, float basePower, float baseSpeed)
    {
        shotPool = weaponShotPool;
        power = basePower;
        speed = baseSpeed;
        OnInitialized();
    }

    protected virtual void Move()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);        
    }

    protected virtual void OnShotCollided() 
    {
        hasCollided = true;
        ReturnToPool();
    }
    
    private void ReturnToPool()
    {
        OnReturnToPool();
        hasCollided = false;
        gameObject.SetActive(false);
        if (shotPool == null) { Destroy(gameObject); }
        else shotPool.Release(this);
    }

    protected void CheckDistanceLimit()
    {
        if (Vector2.Distance(transform.position, startPosition) > maxDistance) OnShotCollided();
    }

    protected virtual void Damage(IDamageable target)
    {
        target.Damage(power);
    }

    public void OnWeaponDestroyed(object sender, System.EventArgs e) => shotPool = null;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable target)) { Damage(target); }
        if (!hasCollided) { OnShotCollided(); }
    }

    protected virtual void OnReturnToPool() { }
    protected virtual void OnInitialized() { }
}
