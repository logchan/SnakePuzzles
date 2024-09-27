using System.Linq;
using TMPro;
using UnityEngine;

public class LevelGroupButtonController : MonoBehaviour {
    public int groupIdx;

    void Start() {
        var group = AppGlobals.LevelGroups[groupIdx];
        transform.Find("WorldName").gameObject.GetComponent<TMP_Text>().text = group.Name;

        var count = AppGlobals.UserData.ClearedLevels.Count(l => l.StartsWith($"{group.Path}/"));
        transform.Find("Status").gameObject.GetComponent<TMP_Text>().text = $"{count}/{group.Levels.Count}";
    }

    public void ButtonClick() {
        AppGlobals.LevelSelectGroupIndex = groupIdx;
    }
}
