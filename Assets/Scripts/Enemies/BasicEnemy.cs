using System;
using UnityEngine;

public class BasicEnemy : MonoBehaviour, IDamagable
{
    [SerializeField] private float maxHealth = 100f;
    private float _currentHealth;
    private Animator _animator;

    private void Start()
    {
        _currentHealth = maxHealth;
    }
    
    public void OnHit(float damage)
    {
        if (_currentHealth - damage >= 0)
        {
            OnDead();
        }
        else
        {
            _currentHealth -= damage;
        }
    }

    public void OnDead()
    {
        Destroy(gameObject);
    }
}
