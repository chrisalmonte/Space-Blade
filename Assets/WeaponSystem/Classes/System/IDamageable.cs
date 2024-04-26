using System;

public interface IDamageable
{
    float HP { get;}
    void Damage(float attackPower);
}
