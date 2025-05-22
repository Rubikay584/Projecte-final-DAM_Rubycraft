using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyManager : MonoBehaviour {
    public static EnemyManager Instance;

    private int enemiesAlive = 0;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterEnemy() {
        enemiesAlive++;
    }

    public void EnemyKilled() {
        enemiesAlive--;

        if (enemiesAlive <= 0) {
            SceneManager.LoadScene("WIN");
        }
    }
}