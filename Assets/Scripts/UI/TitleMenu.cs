using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour {
    public GameObject mainMenuObject;
    public GameObject settingsObject;

    [Header("Main Menu UI Elements")]
    public TextMeshProUGUI seedField;
    
    [Header("Settings Menu UI Elements")]
    public Slider viewDistanceSlider;
    public TextMeshProUGUI viewDistanceText;
    public Slider mouseSensitivitySlider;
    public TextMeshProUGUI mouseSensitivityText;
    public Toggle threadingToggle;

    Settings settings;

    private void Awake() {
        if (!File.Exists(Application.dataPath + "/settings.json")) {
            Debug.Log("No settings file found, creating a new one.");
            settings = new Settings();
            
            string jsonString = JsonUtility.ToJson(settings);
            File.WriteAllText(Application.dataPath + "/settings.json", jsonString);
        } else {
            Debug.Log("Settings file found, loading settings.");
            
            string jsonImport = File.ReadAllText(Application.dataPath + "/settings.json");
            settings = JsonUtility.FromJson<Settings>(jsonImport);
        }
    }

    public void StartGame() {
        VoxelData.seed = Mathf.Abs(seedField.text.GetHashCode()) / VoxelData.WorldSizeInChunks;
        SceneManager.LoadScene("Minecraft", LoadSceneMode.Single);
    }

    public void EnterSettings() {
        viewDistanceSlider.value = settings.viewDistance;
        UpdateViewDistanceSlider();
        mouseSensitivitySlider.value = settings.mouseSensitivity;
        UpdateMouseSensitivitySlider();
        threadingToggle.isOn = settings.enableThreading;
        
        mainMenuObject.SetActive(false);
        settingsObject.SetActive(true);
    }

    public void LeaveSettings() {
        settings.viewDistance = (int)viewDistanceSlider.value;
        settings.mouseSensitivity = mouseSensitivitySlider.value;
        settings.enableThreading = threadingToggle.isOn;
        
        string jsonExport = JsonUtility.ToJson(settings);
        File.WriteAllText(Application.dataPath + "/settings.json", jsonExport);
        
        mainMenuObject.SetActive(true);
        settingsObject.SetActive(false);
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void UpdateViewDistanceSlider() {
        viewDistanceText.text = "View distance - " + viewDistanceSlider.value;
    }
    
    public void UpdateMouseSensitivitySlider() {
        mouseSensitivityText.text = "Mouse sensitivity - " + mouseSensitivitySlider.value.ToString("F2");
    }
}
