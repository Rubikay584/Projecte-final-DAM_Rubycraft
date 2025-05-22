using UnityEngine;

public class ZombieHealth : MonoBehaviour {
    public int maxHealth = 5;
    private int currentHealth;

    void Start() {
        currentHealth = maxHealth;
        if (EnemyManager.Instance != null) {
            EnemyManager.Instance.RegisterEnemy();
        }
    }

    public void TakeDamage(int amount) {
        currentHealth -= amount;
        if (currentHealth <= 0) {
            Die();
        }
    }

    void Die() {
        if (EnemyManager.Instance != null) {
            EnemyManager.Instance.EnemyKilled();
        }
        Destroy(gameObject);
    }
}
