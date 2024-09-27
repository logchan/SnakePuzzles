using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneManager : MonoBehaviour {
    public GameObject canvas;
    public GameObject aboutCanvas;
    public GameObject configPrefab;
    public TMP_Text versionText;

    void Start() {
        versionText.text = AppConsts.VersionText;
    }

    void Update() {
        HandleInput();
        if (AppGlobals.IsConfigOpen && canvas.activeSelf) {
            canvas.SetActive(false);
        }
        if (!AppGlobals.IsConfigOpen && !aboutCanvas.activeSelf && !canvas.activeSelf) {
            canvas.SetActive(true);
        }
    }

    public void HandleInput() {
        if (AppGlobals.IsConfigOpen) {
            return;
        }

        if (aboutCanvas.activeSelf && Input.GetKeyDown(KeyCode.Escape)) {
            AboutBackButton();
        }
    }

    public void StartButton() {
        if (AppGlobals.UserData.ClearedLevels.Count > 0) {
            SceneManager.LoadScene("LevelSelectScene");
        }
        else {
            AppGlobals.LevelGroupIndex = 0;
            AppGlobals.LevelIndex = 0;
            SceneManager.LoadScene("GamePlay");
        }
    }

    public void ConfigButton() {
        GameObject.Instantiate(configPrefab);
    }

    public void AboutButton() {
        canvas.SetActive(false);
        aboutCanvas.SetActive(true);
    }

    public void AboutBackButton() {
        aboutCanvas.SetActive(false);
        canvas.SetActive(true);
    }

    public void QuitButton() {
        if (!Application.isEditor) {
            Application.Quit();
        }
    }
}
