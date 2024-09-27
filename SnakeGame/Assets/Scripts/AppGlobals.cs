using GameModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class AppGlobals {
    public static List<LevelGroup> LevelGroups { get; } = new List<LevelGroup>();

    public static int LevelSelectGroupIndex { get; set; } = -1;
    public static int LevelGroupIndex { get; set; } = 0;
    public static int LevelIndex { get; set; } = 0;

    public static UserData UserData { get; set; }
    public static float MusicVolume { get; set; } = 1f;
    public static float SoundVolume { get; set; } = 1f;
    public static bool IsConfigOpen { get; set; } = false;

    #region Debug
    public static string LevelOverride { get; set; } = "";
    public static string Command { get; set; } = "";
    #endregion

    private static bool _loadedLevels = false;

    static AppGlobals() {
        UserData = UserData.Load();
        UserData.Save();
    }

    public static void LoadLevels() {
        if (_loadedLevels) {
            return;
        }

        foreach (var info in AppConsts.BuiltInLevelGroups) {
            var assets = Resources.LoadAll<TextAsset>($"Levels/{info.Path}");
            var group = new LevelGroup { Path = info.Path, Name = info.Name };
            LevelGroups.Add(group);

            foreach (var asset in assets) {
                try {
                    group.Levels.Add(JsonHelpers.Deserialize<Level>(asset.text));
                    group.LevelFileNames.Add(asset.name);
                    group.LevelJsons.Add(asset.text);
                }
                catch (Exception ex) {
                    Debug.LogError($"Failed to load {group.Name}/{asset.name}: {ex.Message}\n{ex.StackTrace}");
                }
            }

            Debug.Log($"Group {info.Name} ({info.Path}): {group.Levels.Count} levels");
        }

        LevelGroups.Add(new LevelGroup { Path = "Custom", Name = "Custom" });
        ReloadCustomLevels();


        _loadedLevels = true;
    }

    public static void ReloadCustomLevels() {
        try {
            var customLevelPath = Path.Combine(Application.persistentDataPath, "Custom");
            var dir = Directory.CreateDirectory(customLevelPath);
            var group = LevelGroups[^1];
            group.Levels.Clear();
            group.LevelFileNames.Clear();
            group.LevelJsons.Clear();

            foreach (var file in dir.GetFiles("*.json")) {
                try {
                    var text = File.ReadAllText(file.FullName, Encoding.UTF8);
                    group.Levels.Add(JsonHelpers.Deserialize<Level>(text));
                    group.LevelFileNames.Add(Path.GetFileNameWithoutExtension(file.Name));
                    group.LevelJsons.Add(text);
                }
                catch (Exception ex) {
                    Debug.LogError($"Failed to load {file}: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
        catch (Exception ex) {
            Debug.LogError(ex);
        }
    }

    public static Level GetLevel() {
        var json = String.IsNullOrEmpty(LevelOverride) ? LevelGroups[LevelGroupIndex].LevelJsons[LevelIndex] : LevelOverride;
        return JsonHelpers.Deserialize<Level>(json);
    }

    public static string GetLevelKey() {
        if (String.IsNullOrEmpty(LevelOverride)) {
            var group = LevelGroups[LevelGroupIndex];
            return $"{group.Path}/{group.LevelFileNames[LevelIndex]}";
        }

        return null;
    }

    public static void SetSoundVolume() {
        SoundVolume = PlayerPrefs.GetInt(PrefKeys.SoundVolume, 10) / 10f;
    }

    public static void SetMusicVolume() {
        MusicVolume = PlayerPrefs.GetInt(PrefKeys.MusicVolume, 10) / 10f;
    }

    public static void SetResolutionAndWindowMode() {
        if (Application.isEditor) {
            return;
        }

        var resolution = PlayerPrefs.GetString(PrefKeys.Resolution, null);
        var width = Screen.currentResolution.width;
        var height = Screen.currentResolution.height;
        if (resolution != null) {
            width = Int32.Parse(resolution.Split("*")[0]);
            height = Int32.Parse(resolution.Split("*")[1]);
        }
        var fullScreenMode = PlayerPrefs.GetInt(PrefKeys.FullscreenMode, Convert.ToInt32(Screen.fullScreenMode));

        Screen.SetResolution(width, height, (FullScreenMode)fullScreenMode);
    }

    public static void SetQuality() {
        if (Application.isEditor) {
            return;
        }

        var name = PlayerPrefs.GetString(PrefKeys.QualityName, null);
        if (name == null) {
            return;
        }

        var idx = Array.FindIndex(QualitySettings.names, n => n == name);
        if (idx >= 0) {
            QualitySettings.SetQualityLevel(idx);
        }
    }
}
