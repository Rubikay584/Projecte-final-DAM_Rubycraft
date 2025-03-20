using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {
    public void ComenzarJuego() {
        SceneManager.LoadScene("Minecraft");
    }
}
