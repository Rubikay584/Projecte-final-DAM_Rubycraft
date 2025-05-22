using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public int damage = 1;
    public float attackRange = 3f;
    public LayerMask zombieLayer;
    
    public ParticleSystem hitParticles;

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Attack();
        }
    }

    void Attack() {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin, direction, out hit, attackRange, zombieLayer)) {
            ZombieHealth zombie = hit.collider.GetComponent<ZombieHealth>();
            if (zombie != null) {
                zombie.TakeDamage(damage);
                
                if (hitParticles != null) {
                    hitParticles.transform.position = hit.point;
                    hitParticles.Play();
                    Debug.Log("Partículas emitidas en: " + hit.point);
                }
            }
        }
    }
}