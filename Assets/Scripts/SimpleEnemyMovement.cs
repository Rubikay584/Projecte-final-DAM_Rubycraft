using UnityEngine;

public class SimpleEnemyMovement : MonoBehaviour {
    public float moveRadius = 5f;
    public float moveSpeed = 2f;

    Vector3 startPos;
    Vector3 targetPos;

    private void Start() {
        startPos = transform.position;
        SetNewTarget();
    }

    private void Update() {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
            SetNewTarget();
    }

    void SetNewTarget() {
        Vector2 randomCircle = Random.insideUnitCircle * moveRadius;
        targetPos = startPos + new Vector3(randomCircle.x, 0, randomCircle.y);
    }
}