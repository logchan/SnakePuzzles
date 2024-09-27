using GameModel;
using Mechanism;
using UnityEngine;

public class TileController : MonoBehaviour {
    public TileProcessor processor;
    public bool isWitness;

    private Material _material;
    private bool _hasTexture = false;
    private bool _isPlayingError = false;
    private float _playErrorTime = 0f;

    private const float PlayErrorCycles = 2f;
    private const float PlayErrorTotalTime = 1.5f;

    private GameObject _plane;
    private GameObject _obstacle;

    void Start() {
        _plane = transform.Find("Plane").gameObject;
        _obstacle = transform.Find("Obstacle").gameObject;
        if (processor.Tile.TileType == TileType.Wall) {
            _obstacle.SetActive(true);
        }
        
        SetTexture();
    }

    void Update() {
        PlayFailureAnimation();
    }

    private void SetTexture() {
        _material = _plane.GetComponent<MeshRenderer>().material;
        if (isWitness && processor.Tile.X % 2 == 1 && processor.Tile.Y % 2 == 1) {
            var color = _material.GetColor("_Color");
            color.r *= 0.5f;
            color.g *= 0.5f;
            color.b *= 0.5f;

            _material.SetColor("_Color", color);
        }

        var textureName = processor.Tile.TileType.ToString();
        if (processor.Tile.TileType == TileType.Triangle) {
            textureName = $"Triangle_{processor.Tile.TileArgs[0]}";
        }
        else if (processor.Tile.TileType == TileType.Wall) {
            textureName = "Plain";
        }

        var texture = Resources.Load<Texture2D>($"Texture/{textureName}");
        if (texture != null) {
            _material.SetTexture("_MainTex", texture);
            _hasTexture = true;
        }
    }

    private void PlayFailureAnimation() {
        if (!_hasTexture) {
            return;
        }

        if (processor.HasFailure) {
            processor.HasFailure = false;
            _isPlayingError = true;
            _playErrorTime = 0f;
        }

        if (_isPlayingError) {
            var t = _playErrorTime / PlayErrorTotalTime * PlayErrorCycles;
            t = Mathf.Abs(Mathf.Sin(t * Mathf.PI));

            _material.SetFloat("_ErrorT", t);

            _playErrorTime += Time.deltaTime;
            if (_playErrorTime > PlayErrorTotalTime) {
                _material.SetFloat("_ErrorT", 0);
                _isPlayingError = false;
            }
        }
    }
}
