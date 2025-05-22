using UnityEngine;

public class ZombieMovement : MonoBehaviour{
    public Transform playerTransform;

    void Update() {
        if (playerTransform == null) return;

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer.sqrMagnitude > 0.001f) {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);
        }
    }
}