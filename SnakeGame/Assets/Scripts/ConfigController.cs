using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfigController : MonoBehaviour {
    public Slider musicVolumeSlider;
    public Slider soundVolumeSlider;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown windowModeDropdown;
    public TMP_Dropdown qualityDropdown;

    private bool _isReady = false;

    private readonly List<FullScreenMode> supportedFullScreenModes = new() {
        FullScreenMode.FullScreenWindow,
        FullScreenMode.ExclusiveFullScreen,
        FullScreenMode.Windowed,
    };

    public ConfigController() {
        if (Application.platform == RuntimePlatform.OSXPlayer) {
            supportedFullScreenModes.Add(FullScreenMode.MaximizedWindow);
        }
    }

    void Start() {
        AppGlobals.IsConfigOpen = true;

        musicVolumeSlider.value = PlayerPrefs.GetInt(PrefKeys.MusicVolume, 10);
        soundVolumeSlider.value = PlayerPrefs.GetInt(PrefKeys.SoundVolume, 10);
        SetupResolutions();
        SetupWindowModes();
        SetupQualityModes();

        _isReady = true;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            CloseConfig();
        }
    }

    private void SetupResolutions() {
        // TODO: different options when window mode is not ExclusiveFullScreen
        var selection = 0;
        var idx = 0;
        var currWidth = Screen.width;
        var currHeight = Screen.height;
        var added = new HashSet<string>();
        foreach (var resolution in Screen.resolutions.OrderBy(r => -r.width).ThenBy(r => -r.height)) {
            var key = $"{resolution.width}*{resolution.height}";
            if (added.Contains(key)) {
                continue;
            }
            added.Add(key);

            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData {
                text = key,
            });
            if (resolution.width == currWidth && resolution.height == currHeight) {
                selection = idx;
            }
            idx += 1;
        }
        resolutionDropdown.value = selection;
    }

    private void SetupWindowModes() {
        var selection = 0;
        var idx = 0;
        var curr = Screen.fullScreenMode;
        foreach (var mode in supportedFullScreenModes) {
            windowModeDropdown.options.Add(new TMP_Dropdown.OptionData {
                text = mode.ToString(),
            });
            if (curr == mode) {
                selection = idx;
            }
            idx += 1;
        }
        windowModeDropdown.value = selection;
    }

    private void SetupQualityModes() {
        foreach (var name in QualitySettings.names) {
            qualityDropdown.options.Add(new TMP_Dropdown.OptionData {
                text = name
            });
        }
        qualityDropdown.value = QualitySettings.GetQualityLevel();
    }

    public void ChangeMusic() {
        if (!_isReady) {
            return;
        }

        PlayerPrefs.SetInt(PrefKeys.MusicVolume, (int)musicVolumeSlider.value);
        AppGlobals.SetMusicVolume();
    }

    public void ChangeSound() {
        if (!_isReady) {
            return;
        }

        PlayerPrefs.SetInt(PrefKeys.SoundVolume, (int)soundVolumeSlider.value);
        AppGlobals.SetSoundVolume();
    }

    public void ChangeResolution(int idx) {
        if (!_isReady) {
            return;
        }

        PlayerPrefs.SetString(PrefKeys.Resolution, resolutionDropdown.options[idx].text);
        AppGlobals.SetResolutionAndWindowMode();
    }

    public void ChangeWindowMode(int idx) {
        if (!_isReady) {
            return;
        }

        PlayerPrefs.SetInt(PrefKeys.FullscreenMode, Convert.ToInt32(supportedFullScreenModes[idx]));
        AppGlobals.SetResolutionAndWindowMode();
    }

    public void ChangeQuality(int idx) {
        if (!_isReady) {
            return;
        }

        PlayerPrefs.SetString(PrefKeys.QualityName, QualitySettings.names[idx]);
        AppGlobals.SetQuality();
    }

    public void CloseConfig() {
        AppGlobals.IsConfigOpen = false;
        GameObject.Destroy(this.gameObject);
    }
}
