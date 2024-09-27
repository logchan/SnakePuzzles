using GameModel;
using Mechanism;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    private const float InputHoldWaitTime = 0.3f;
    private const float ToastHoldTime = 1f;
    private const float ToastFadeTime = 0.5f;

    private static Dictionary<TileType, Type> TileProcessorTypes { get; } = new Dictionary<TileType, Type> {
        { TileType.Occupy, typeof(OccupyTileProcessor) },
        { TileType.Empty, typeof(EmptyTileProcessor) },
        { TileType.Triangle, typeof(TriangleTileProcessor) },
    };

    private static KeyCode[] AllowHoldKeys = new[] { KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S, KeyCode.Z };
    private static Dictionary<KeyCode, Tuple<int, int>> KeyToMovement { get; } = new Dictionary<KeyCode, Tuple<int, int>> {
        { KeyCode.A, new Tuple<int, int>(-1, 0) },
        { KeyCode.D, new Tuple<int, int>(1, 0) },
        { KeyCode.W, new Tuple<int, int>(0, 1) },
        { KeyCode.S, new Tuple<int, int>(0, -1) },
    };

    private enum UIState {
        Load,
        Play,
        Win,
    }

    public GameObject tilePrefab;
    public GameObject snakeBlockPrefab;
    public GameObject snakeHeadPrefab;
    public GameObject fruitPrefab;
    public GameObject configPrefab;
    public GameObject tileContainer;
    public GameObject snakeContainer;
    public GameObject fruitContainer;
    public TMP_Dropdown levelDropdown;
    public AudioClip levelPassSound;
    public AudioClip levelFailSound;
    public AudioClip cantMoveSound;
    public AudioSource audioSource;
    public TMP_Text levelNameText;
    public GameObject mainCanvas;
    public GameObject winCanvas;
    public GameObject debugCanvas;
    public TMP_Text helpText;
    public GameObject helpTextBg;
    public TMP_Text toastHelpText;
    public GameObject toastHelpTextBg;
    public TMP_Text undoStackText;
    public AudioSource musicSource;

    private readonly GameState _state = new GameState();

    private List<HistoryState> _history = new List<HistoryState>();
    private List<int[]> _dropdownData = new List<int[]>();
    private UIState _uiState = UIState.Load;
    private float _sinceLastHold = 0f;
    private float _toastShownTime = 0f;
    private bool _showUndoStack = false;

    void Start() {
        AppGlobals.SetMusicVolume();
        AppGlobals.SetSoundVolume();
        AppGlobals.LoadLevels();
        PopulateLevelDropdown();
        ReloadLevel();
    }

    void Update() {
        musicSource.volume = AppGlobals.MusicVolume;
        HandleInput();
        UpdateHelpTextToast();
        if (AppGlobals.IsConfigOpen && mainCanvas.activeSelf) {
            mainCanvas.SetActive(false);
        }
        if (!AppGlobals.IsConfigOpen && !mainCanvas.activeSelf) {
            mainCanvas.SetActive(true);
        }
    }

    void FixedUpdate() {
        if (AppGlobals.Command == "LoadLevel") {
            AppGlobals.Command = "";
            ReloadLevel();
        }
    }

    private void HandleInput() {
        if (AppGlobals.IsConfigOpen) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F12)) {
            debugCanvas.SetActive(!debugCanvas.activeSelf);
        }

        if (_uiState == UIState.Play) {
            var stepSize = _state.level.IsWitness ? 2 : 1;

            var handled = false;
            foreach (var key in AllowHoldKeys) {
                var (held, stop) = GetKey(key);
                if (held) {
                    if (key == KeyCode.Z) {
                        PopHistory();
                    }
                    else {
                        var (dx, dy) = KeyToMovement[key];
                        MoveSnake(dx * stepSize, dy * stepSize);
                    }
                }

                handled = held || stop;
                if (handled) {
                    break;
                }
            }

            if (!handled) {
                if (Input.GetKeyDown(KeyCode.R)) {
                    ReloadLevel(false);
                }
            }
        }
        else if (_uiState == UIState.Win) {
            if (Input.GetKeyDown(KeyCode.R)) {
                ReloadLevel();
            }
            else if (Input.GetKeyDown(KeyCode.Space)) {
                GoNextLevel();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            AppGlobals.LevelSelectGroupIndex = AppGlobals.LevelGroupIndex;
            SceneManager.LoadScene("LevelSelectScene");
        }
    }

    /// <summary>
    /// Check key with hold handling
    /// </summary>
    /// <returns>(should handle key, should skip others)</returns>
    private Tuple<bool, bool> GetKey(KeyCode key) {
        if (Input.GetKeyDown(key)) {
            _sinceLastHold = 0;
            return new Tuple<bool, bool>(true, true);
        }

        if (Input.GetKey(key)) {
            if (_sinceLastHold < InputHoldWaitTime) {
                _sinceLastHold += Time.deltaTime;
                return new Tuple<bool, bool>(false, true);
            }
            _sinceLastHold = 0;
            return new Tuple<bool, bool>(true, true);
        }

        return new Tuple<bool, bool>(false, false);
    }

    private void CreateTiles() {
        const float TileSize = AppConsts.TileSize;
        const float FieldSize = AppConsts.FieldSize;

        var level = _state.level;
        var transform = tileContainer.transform;
        var rows = _state.level.Rows;
        var cols = _state.level.Columns;
        var width = cols * TileSize;
        var height = rows * TileSize;

        var scale = Mathf.Min(FieldSize / width, FieldSize / height);
        width *= scale;
        height *= scale;
        // Debug.Log($"Width: {width}, height: {height}, posX: {(FieldSize - width) / 2 - FieldSize / 2}, posZ: {(FieldSize - height) / 2 - FieldSize / 2}");

        transform.localScale = new Vector3(scale, scale, scale);
        transform.localPosition = new Vector3((FieldSize - width) / 2 - FieldSize / 2, 0, (FieldSize - height) / 2 - FieldSize / 2);
        snakeContainer.transform.localScale = new Vector3(scale, scale, scale);
        snakeContainer.transform.localPosition = new Vector3(transform.localPosition.x, 0.25f * scale, transform.localPosition.z);
        fruitContainer.transform.localScale = new Vector3(scale, scale, scale);
        fruitContainer.transform.localPosition = new Vector3(transform.localPosition.x, 0.1f * scale, transform.localPosition.z);

        var tiles = new TileProcessor[rows, cols];
        _state.tiles = tiles;
        var specialTiles = new Dictionary<Tuple<int, int>, Tile>();
        foreach (var tile in level.SpecialTiles) {
            specialTiles.Add(new Tuple<int, int>(tile.X, tile.Y), tile);
        }
        for (var y = 0; y < rows; ++y) {
            for (var x = 0; x < cols; ++x) {
                var tileObj = GameObject.Instantiate(tilePrefab);
                tileObj.transform.localPosition = new Vector3(x * TileSize + TileSize / 2, 0, y * TileSize + TileSize / 2);
                tileObj.transform.SetParent(transform, false);

                if (!specialTiles.TryGetValue(new Tuple<int, int>(x, y), out var info)) {
                    info = new Tile();
                }
                var tile = new Tile {
                    X = x,
                    Y = y,
                    TileType = info.TileType,
                };
                tile.TileArgs.AddRange(info.TileArgs);

                var controller = tileObj.GetComponent<TileController>();

                if (!TileProcessorTypes.TryGetValue(tile.TileType, out var processorType)) {
                    processorType = typeof(TileProcessor);
                }
                controller.processor = Activator.CreateInstance(processorType, tile, controller) as TileProcessor;
                controller.isWitness = level.IsWitness;
                tiles[y, x] = controller.processor;
            }
        }
    }

    private void CreateSnake() {
        var idx = -1;
        foreach (var block in _state.snake.Blocks) {
            idx += 1;
            if (block.Object != null) {
                continue;
            }

            block.Object = GameObject.Instantiate(idx == 0 ? snakeHeadPrefab : snakeBlockPrefab);
            block.Object.transform.SetParent(snakeContainer.transform, false);

            var controller = block.Object.GetComponent<SnakeBlockController>();
            controller.snake = _state.snake;
            controller.blockIdx = idx;
        }
    }

    private void CreateFruits() {
        const float TileSize = AppConsts.TileSize;

        foreach (var fruit in _state.fruits) {
            if (fruit.Object != null) {
                continue;
            }

            fruit.Object = GameObject.Instantiate(fruitPrefab);
            fruit.Object.transform.SetParent(fruitContainer.transform, false);
            fruit.Object.transform.localPosition = new Vector3(fruit.X * TileSize + TileSize / 2, 0, fruit.Y * TileSize + TileSize / 2);
            fruit.Object.GetComponent<FruitController>().fruit = fruit;
        }
    }

    private void MoveSnake(int dx, int dy) {
        var blocks = _state.snake.Blocks;
        if (blocks.Count == 0) {
            return;
        }

        var head = blocks[0];
        var newX = head.X + dx;
        var newY = head.Y + dy;
        if (
            !CanMove(head.X, head.Y, newX, newY) ||
            (_state.level.IsWitness && !CanMove(head.X, head.Y, head.X + dx / 2, head.Y + dy / 2))
         ) {
            head.Object.GetComponent<SnakeBlockController>().PlayCannotMove();
            PlaySound(cantMoveSound);
            return;
        }

        PushHistory();

        var fruit = _state.fruits.Find(f => f.X == newX && f.Y == newY);
        var lastX = blocks[^1].X;
        var lastY = blocks[^1].Y;

        var stepSize = Math.Max(Math.Abs(dx), Math.Abs(dy));
        var blockIdx = -1;
        foreach (var block in blocks) {
            blockIdx += 1;
            if (blockIdx % stepSize != 0) {
                continue;
            }
            var prevX = block.X;
            var prevY = block.Y;
            _state.occupancy[prevY, prevX] = null;

            block.X = newX;
            block.Y = newY;
            _state.occupancy[newY, newX] = block;

            newX = prevX;
            newY = prevY;
        }
        if (stepSize == 2) {
            newX = blocks[0].X - dx / 2;
            newY = blocks[0].Y - dy / 2;
            blockIdx = -1;
            foreach (var block in blocks) {
                blockIdx += 1;
                if (blockIdx % stepSize != 1) {
                    continue;
                }
                var prevX = block.X;
                var prevY = block.Y;
                _state.occupancy[prevY, prevX] = null;

                block.X = newX;
                block.Y = newY;
                _state.occupancy[newY, newX] = block;

                newX = prevX;
                newY = prevY;
            }
        }

        if (fruit != null && !fruit.IsConsumed) {
            fruit.IsConsumed = true;
            blocks.Add(new SnakeBlock {
                X = lastX,
                Y = lastY,
                IsEmpty = fruit.IsEmpty,
                IsFruit = !fruit.IsPersistent
            });
            _state.occupancy[lastY, lastX] = blocks[^1];
            CreateSnake();

            // for persistent fruit, append to all previous states
            // and remove all states prior to last fruit consumption
            // this must be valid as we at most occupy the earliest state
            if (fruit.IsPersistent) {
                var validIdx = _history.Count - 1;
                for (; validIdx > 0; --validIdx) {
                    if (_history[validIdx].Snake.Blocks.Count != _history[validIdx - 1].Snake.Blocks.Count) {
                        break;
                    }
                }
                _history = _history.Skip(validIdx).ToList();

                var prevBlock = _history[0].Snake.Blocks[^1];
                _history.RemoveAt(0);
                validIdx = 0;
                var idx = 0;
                foreach (var hist in _history) {
                    idx += 1;
                    if (hist.Snake.Blocks.Any(b => b.X == prevBlock.X && b.Y == prevBlock.Y)) {
                        validIdx = idx;
                    }
                    hist.Snake.Blocks.Add(new SnakeBlock {
                        X = prevBlock.X,
                        Y = prevBlock.Y,
                        IsEmpty = fruit.IsEmpty,
                    });
                    hist.Snake.Blocks.ForEach(b => b.IsFruit = false);
                    prevBlock = hist.Snake.Blocks[^2];
                }
                _history = _history.Skip(validIdx).ToList();
                UpdateUndoStackText();

                blocks.ForEach(b => {
                    b.IsFruit = false;
                    b.Object.GetComponent<SnakeBlockController>().SetMaterials();
                });
            }
        }

        if (_state.level.IsWitness) {
            dx = blocks[^1].X - lastX;
            dy = blocks[^1].Y - lastY;
            if (dx != 0) {
                foreach (var ddx in new[] { dx / 2, 0 }) {
                    blocks.Add(new SnakeBlock {
                        X = lastX + ddx,
                        Y = lastY,
                    });
                    _state.occupancy[lastY, lastX + ddx] = blocks[^1];
                }
            }
            else {
                foreach (var ddy in new[] { dy / 2, 0 }) {
                    blocks.Add(new SnakeBlock {
                        X = lastX,
                        Y = lastY + ddy,
                    });
                    _state.occupancy[lastY + ddy, lastX] = blocks[^1];
                }
            }
            CreateSnake();
        }

        CheckWin();
    }

    private bool CanMove(int fromX, int fromY, int toX, int toY) {
        if (toX < 0 || toX >= _state.level.Columns) {
            return false;
        }
        if (toY < 0 || toY >= _state.level.Rows) {
            return false;
        }
        if (_state.tiles[toY, toX].Tile.TileType == TileType.Wall) {
            return false;
        }

        var blocks = _state.snake.Blocks;
        for (var i = 1; i < blocks.Count; ++i) {
            if (blocks[i].X == toX && blocks[i].Y == toY) {
                return false;
            }
        }

        return true;
    }

    private void CheckWin() {
        var head = _state.snake.Blocks[0];
        if (_state.tiles[head.Y, head.X].Tile.TileType != TileType.Goal) {
            return;
        }

        var win = true;
        foreach (var tile in _state.tiles) {
            win &= tile.Check(_state);
        }

        if (!win) {
            PlaySound(levelFailSound);
            return;
        }

        var levelKey = AppGlobals.GetLevelKey();
        if (!String.IsNullOrEmpty(levelKey)) {
            var userData = AppGlobals.UserData;
            if (!userData.ClearedLevels.Contains(levelKey)) {
                userData.ClearedLevels.Add(levelKey);
                userData.Save();
            }
        }

        PlaySound(levelPassSound);
        _uiState = UIState.Win;
        EnableWinUI();
    }

    private void ReloadLevel(bool setHelpText = true) {
        _uiState = UIState.Load;
        DisableWinUI();

        _history.Clear();

        _state.snake?.Destroy();
        _state.snake = null;

        if (_state.tiles != null) {
            foreach (var tile in _state.tiles) {
                GameObject.Destroy(tile.Controller.gameObject);
            }
        }
        if (_state.fruits != null) {
            foreach (var fruit in _state.fruits) {
                GameObject.Destroy(fruit.Object);
            }
        }
        _state.tiles = null;

        _state.level = AppGlobals.GetLevel();
        _state.snake = _state.level.StartSnake;
        _state.occupancy = new SnakeBlock[_state.level.Rows, _state.level.Columns];
        _state.snake.Blocks.ForEach(b => _state.occupancy[b.Y, b.X] = b);
        _state.fruits = _state.level.Fruits;
        CreateTiles();
        CreateSnake();
        if (!_state.level.IsWitness) {
            CreateFruits();
        }

        if (!String.IsNullOrEmpty(AppGlobals.LevelOverride)) {
            levelNameText.text = "Custom Level";
            if (musicSource.isPlaying) {
                musicSource.Stop();
            }
        }
        else {
            var group = AppGlobals.LevelGroups[AppGlobals.LevelGroupIndex];
            levelNameText.text = $"{group.Name} / {AppGlobals.LevelIndex + 1:D2}";

            if (group.Name == "SP-WT" || group.Name == "SP-BR") {
                if (!musicSource.isPlaying) {
                    musicSource.Play();
                }
            }
            else if (musicSource.isPlaying) {
                musicSource.Stop();
            }
        }

        if (setHelpText) {
            SetHelpText(_state.level.HelpText);
        }
        _uiState = UIState.Play;
        _showUndoStack = _state.fruits.Any(f => f.IsPersistent) && _state.fruits.Any(f => !f.IsPersistent);
        undoStackText.text = "";
    }

    private void PlaySound(AudioClip clip) {
        if (AppGlobals.SoundVolume > 0) {
            audioSource.PlayOneShot(clip, AppGlobals.SoundVolume);
        }
    }

    private void SetHelpText(string text) {
        helpText.text = text;
        toastHelpText.text = text;

        var hasHelp = !String.IsNullOrEmpty(text);
        helpTextBg.SetActive(hasHelp);
        toastHelpTextBg.SetActive(hasHelp);
        _toastShownTime = hasHelp ? 0f : 100f;
        SetToastAlpha(1);
    }

    private void UpdateHelpTextToast() {
        if (_toastShownTime >= ToastHoldTime + ToastFadeTime) {
            return;
        }

        _toastShownTime += Time.deltaTime;
        if (_toastShownTime < ToastHoldTime) {
            return;
        }
        if (_toastShownTime >= ToastHoldTime + ToastFadeTime) {
            toastHelpText.text = "";
            toastHelpTextBg.SetActive(false);
            return;
        }

        SetToastAlpha(1 - (_toastShownTime - ToastHoldTime) / ToastFadeTime);
    }

    private void SetToastAlpha(float v) {
        var image = toastHelpTextBg.GetComponent<Image>();
        image.color = new Color(0, 0, 0, v * 0.5f);
        toastHelpText.color = new Color(1, 1, 1, v);
    }

    private void EnableWinUI() {
        winCanvas.SetActive(true);
    }

    private void DisableWinUI() {
        winCanvas.SetActive(false);
    }

    public void BackButton() {
        SceneManager.LoadScene("LevelSelectScene");
    }

    private void GoNextLevel() {
        var group = AppGlobals.LevelGroups[AppGlobals.LevelGroupIndex];
        if (AppGlobals.LevelIndex < group.Levels.Count - 1) {
            AppGlobals.LevelIndex += 1;
            ReloadLevel();
        }
        else if (AppGlobals.LevelGroupIndex < AppGlobals.LevelGroups.Count - 2) {
            // Don't auto advance to "Custom", which is always last group
            AppGlobals.LevelGroupIndex += 1;
            AppGlobals.LevelIndex = 0;
            ReloadLevel();
        }
        else {
            AppGlobals.LevelSelectGroupIndex = -1;
            SceneManager.LoadScene("LevelSelectScene");
        }
    }

    private void PopulateLevelDropdown() {
        for (var groupIdx = 0; groupIdx < AppGlobals.LevelGroups.Count; ++groupIdx) {
            var group = AppGlobals.LevelGroups[groupIdx];
            for (var levelIdx = 0; levelIdx < group.Levels.Count; ++levelIdx) {
                levelDropdown.options.Add(new TMP_Dropdown.OptionData { text = $"{group.Name}/{levelIdx:D2}" });
                _dropdownData.Add(new int[] { groupIdx, levelIdx });
            }
        }
        levelDropdown.value = FindLevelDropdownIndex();
    }

    public void LevelDropdown(int v) {
        var arr = _dropdownData[v];
        AppGlobals.LevelGroupIndex = arr[0];
        AppGlobals.LevelIndex = arr[1];
        ReloadLevel();
    }

    public void ResetLevelButton() {
        ReloadLevel();
    }

    public void NextLevelButton() {
        var idx = FindLevelDropdownIndex();
        if (idx < _dropdownData.Count - 1) {
            levelDropdown.value = idx + 1;
        }
    }

    public void PreviousLevelButton() {
        var idx = FindLevelDropdownIndex();
        if (idx > 0) {
            levelDropdown.value = idx - 1;
        }
    }

    public void ConfigButton() {
        if (!AppGlobals.IsConfigOpen) {
            GameObject.Instantiate(configPrefab);
        }
    }

    private int FindLevelDropdownIndex() {
        var idx = _dropdownData.FindIndex(arr => arr[0] == AppGlobals.LevelGroupIndex && arr[1] == AppGlobals.LevelIndex);
        if (idx < 0) {
            return 0;
        }
        return idx;
    }

    private void PushHistory() {
        _history.Add(new HistoryState {
            Snake = new Snake {
                Blocks = _state.snake.Blocks.Select(b => new SnakeBlock {
                    X = b.X,
                    Y = b.Y,
                    IsEmpty = b.IsEmpty,
                    IsFruit = b.IsFruit,
                }).ToList(),
            },
            FruitConsumed = _state.fruits.Select(f => f.IsConsumed).ToArray(),
        });

        UpdateUndoStackText();
    }

    private void PopHistory() {
        if (_history.Count == 0) {
            _state.snake.Blocks[0].Object.GetComponent<SnakeBlockController>().PlayCannotMove();
            PlaySound(cantMoveSound);
            return;
        }

        var hist = _history[^1];
        _history.RemoveAt(_history.Count - 1);

        // restore snake
        _state.snake.Destroy();
        _state.snake = hist.Snake;
        CreateSnake();

        // update occupancy
        _state.occupancy = new SnakeBlock[_state.level.Rows, _state.level.Columns];
        foreach (var block in _state.snake.Blocks) {
            _state.occupancy[block.Y, block.X] = block;
        }

        // update fruits
        for (var i = 0; i < _state.fruits.Count; ++i) {
            if (!_state.fruits[i].IsPersistent) {
                _state.fruits[i].IsConsumed = hist.FruitConsumed[i];
            }
        }

        UpdateUndoStackText();
    }

    private void UpdateUndoStackText() {
        if (!_showUndoStack) {
            return;
        }

        undoStackText.text = $"Undo stack: {_history.Count}";
    }

    private class HistoryState {
        public Snake Snake { get; set; }
        public bool[] FruitConsumed { get; set; }
    }
}
