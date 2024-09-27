using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour {
    public GameObject groupButtonPrefab;
    public GameObject levelButtonPrefab;
    public GameObject groupCanvas;
    public GameObject levelCanvas;
    public TMP_Text groupTitle;

    private int _showingGroup = -1;
    private Transform _levelContainer;
    private List<GameObject> _levelButtons = new List<GameObject>();
    private Transform _groupButtonContainer;
    private GameObject _customGroupButton;

    void Start() {
        AppGlobals.LoadLevels();

        _groupButtonContainer = groupCanvas.transform.Find("Scroll View/Viewport/Content");
        for (var groupIdx = 0; groupIdx < AppGlobals.LevelGroups.Count; ++groupIdx) {
            var button = CreateButton(groupIdx);
            if (groupIdx == AppGlobals.LevelGroups.Count - 1) {
                _customGroupButton = button;
            }
        }

        _levelContainer = levelCanvas.transform.Find("Scroll View/Viewport/Content");
    }

    private GameObject CreateButton(int groupIdx) {
        var button = GameObject.Instantiate(groupButtonPrefab);
        var controller = button.GetComponent<LevelGroupButtonController>();
        controller.groupIdx = groupIdx;
        button.transform.SetParent(_groupButtonContainer);
        return button;
    }

    void Update() {
        if (_showingGroup == -1 && levelCanvas.activeSelf) {
            levelCanvas.SetActive(false);
        }
        UpdateShowingGroup();

        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (_showingGroup == -1) {
                ReturnToStartButton();
            }
            else {
                CloseShowingGroupButton();
            }
        }
    }

    private void UpdateShowingGroup() {
        if (_showingGroup == AppGlobals.LevelSelectGroupIndex) {
            return;
        }

        if (AppGlobals.LevelSelectGroupIndex == -1) {
            levelCanvas.SetActive(false);
            groupCanvas.SetActive(true);
            _levelButtons.ForEach(obj => GameObject.Destroy(obj));
            _levelButtons.Clear();
            _showingGroup = -1;

            return;
        }

        groupCanvas.SetActive(false);
        var groupIdx = AppGlobals.LevelSelectGroupIndex;
        var group = AppGlobals.LevelGroups[groupIdx];
        groupTitle.text = group.Name;
        for (var levelIdx = 0; levelIdx < group.Levels.Count; ++levelIdx) {
            var obj = GameObject.Instantiate(levelButtonPrefab);
            var controller = obj.GetComponent<LevelSelectButtonController>();
            controller.groupIdx = groupIdx;
            controller.groupPath = group.Path;
            controller.levelIdx = levelIdx;
            controller.levelPath = group.LevelFileNames[levelIdx];
            obj.transform.SetParent(_levelContainer);
            _levelButtons.Add(obj);
        }
        _showingGroup = groupIdx;
        levelCanvas.SetActive(true);
    }

    public void CloseShowingGroupButton() {
        AppGlobals.LevelSelectGroupIndex = -1;
    }

    public void ReturnToStartButton() {
        SceneManager.LoadScene("StartScene");
    }

    public void ReloadCustomButton() {
        AppGlobals.ReloadCustomLevels();
        GameObject.Destroy(_customGroupButton);
        _customGroupButton = CreateButton(AppGlobals.LevelGroups.Count - 1);
    }
}
