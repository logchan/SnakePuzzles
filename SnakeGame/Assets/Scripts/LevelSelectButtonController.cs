using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectButtonController : MonoBehaviour {
    public int groupIdx;
    public int levelIdx;
    public string groupPath;
    public string levelPath;

    void Start() {
        var title = transform.Find("Title").GetComponent<TMP_Text>();
        title.text = $"{levelIdx+1:D2}";
        if (groupPath == "Custom") {
            title.text = levelPath;
            title.fontSize = 20;
            title.enableWordWrapping = true;
        }

        if (AppGlobals.UserData.ClearedLevels.Contains($"{groupPath}/{levelPath}")) {
            transform.Find("Circle").gameObject.SetActive(true);
        }
    }

    public void ButtonClick() {
        AppGlobals.LevelGroupIndex = groupIdx;
        AppGlobals.LevelIndex = levelIdx;
        SceneManager.LoadScene("GamePlay");
    }
}
