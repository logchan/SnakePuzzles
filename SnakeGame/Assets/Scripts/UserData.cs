using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public sealed class UserData {
    public List<string> ClearedLevels { get; } = new List<string>();

    public static UserData Load() {
        if (!File.Exists(UserDataFile)) {
            return new UserData();
        }

        try {
            var text = File.ReadAllText(UserDataFile, Encoding.UTF8);
            return JsonHelpers.Deserialize<UserData>(text);
        }
        catch (Exception ex) {
            Debug.LogError(ex);
            return new UserData();
        }
    }

    public void Save() {
        try {
            var text = JsonHelpers.Serialize(this);
            File.WriteAllText(UserDataFile, text, Encoding.UTF8);
        }
        catch (Exception ex) {
            Debug.LogError(ex);
        }
    }

    private static string UserDataFile => Path.Combine(Application.persistentDataPath, "UserData.json");
}
