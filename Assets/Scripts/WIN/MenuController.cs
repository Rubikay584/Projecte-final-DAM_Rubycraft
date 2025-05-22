using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {
    public void RestartGame() {
        SceneManager.LoadScene("MainMenu");
    }
}
