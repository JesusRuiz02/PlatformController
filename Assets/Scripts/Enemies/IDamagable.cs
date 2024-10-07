using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public void OnHit(float damage);
    public void OnDead();
}
