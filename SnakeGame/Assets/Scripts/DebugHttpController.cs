using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class DebugHttpController : MonoBehaviour {
    private bool _enabled = false;
    private float _elapsedTime = 0.0f;
    private bool _makingRequest = false;
    private int _port = 5000;

    public TMP_Text httpButtonText;
    public TMP_InputField httpPortInput;

    void Start() {
    }

    void Update() {
        ProcessHttp();
    }

    private void ProcessHttp() {
        if (!_enabled) {
            return;
        }

        if (!_makingRequest) {
            _elapsedTime += Time.deltaTime;
        }
        if (_elapsedTime < 1) {
            return;
        }

        _elapsedTime = 0;
        StartCoroutine(ExecuteHttp());
    }

    IEnumerator ExecuteHttp() {
        var www = UnityWebRequest.Get($"http://127.0.0.1:{_port}/api/get-command");
        _makingRequest = true;
        yield return www.SendWebRequest();

        _makingRequest = false;
        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError(www.error);
            SetEnabled(false);
        }
        else {
            var text = www.downloadHandler.text;
            try {
                var resp = JsonHelpers.Deserialize<ApiResponse>(text);
                if (resp.Command == "loadLevel") {
                    AppGlobals.Command = "LoadLevel";
                    AppGlobals.LevelOverride = resp.Payload;
                }
            }
            catch (System.Exception ex) {
                Debug.LogError(text);
                Debug.LogError(ex);
            }
        }
    }

    public void ToggleEnable() {
        SetEnabled(!_enabled);
    }

    public void SetEnabled(bool enabled) {
        if (enabled) {
            if (int.TryParse(httpPortInput.text, out int port) && port > 0 && port < 65536) {
                _enabled = true;
                _port = port;
                httpButtonText.text = "HTTP is ON";
            }
        }
        else {
            _enabled = false;
            _elapsedTime = 0;
            httpButtonText.text = "HTTP is OFF";
        }
    }
}
