using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
  [SerializeField] private PlayerStats playerStats;

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.GetComponent<IDamagable>() != null)
    {
      other.GetComponent<IDamagable>().OnHit(playerStats.BasicDamageAttack);
    }
  }
}
