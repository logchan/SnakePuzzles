using GameModel;
using UnityEngine;

public class SnakeBlockController : MonoBehaviour {
    public Snake snake;
    public int blockIdx;

    private GameObject _cube;
    private GameObject _corner;

    void Start() {
        if (snake != null) {
            if (blockIdx == 0) {
                _cube = snake.Blocks[0].Object.transform.Find("Sphere")?.gameObject;
            }
            else {
                _cube = snake.Blocks[blockIdx].Object.transform.Find("Cube")?.gameObject;
                _corner = snake.Blocks[blockIdx].Object.transform.Find("Corner")?.gameObject;
            }
            SetMaterials();
        }
    }

    public void SetMaterials() {
        SetMaterial(_cube);
        SetMaterial(_corner);
    }

    public void PlayCannotMove() {
        var animator = _cube?.GetComponent<Animator>();
        if (animator == null) {
            return;
        }

        animator.SetTrigger("CannotMove");
    }

    private void SetMaterial(GameObject obj) {
        if (obj == null) {
            return;
        }
        var block = snake.Blocks[blockIdx];
        var material = obj.GetComponent<MeshRenderer>().material;
        var color = material.GetColor("_Color");
        if (block.IsFruit) {
            color.r = 0.8f;
            color.g = 0.9f;
            color.b = 1f;
            color.a = block.IsEmpty ? 0.3f : 0.7f;
        }
        else {
            color.r = 1f;
            color.g = 1f;
            color.b = 1f;
            color.a = block.IsEmpty ? 0.2f : 0.5f;
        }
        material.SetColor("_Color", color);
    }

    void Update() {
        if (snake == null) {
            return;
        }

        const float TileSize = AppConsts.TileSize;
        if (blockIdx >= snake.Blocks.Count) {
            return;
        }

        var block = snake.Blocks[blockIdx];
        transform.localPosition = new Vector3(block.X * TileSize + TileSize / 2, 0, block.Y * TileSize + TileSize / 2);

        // Set shape from neighboring block positions

        if (blockIdx == 0) {
            SetCube();
            return;
        }
        if (blockIdx == snake.Blocks.Count - 1) {
            // last block, always straight
            SetCube();

            var neighbor = snake.Blocks[blockIdx - 1];
            if (neighbor.X == block.X) {
                SetDirection(0);
            }
            else {
                SetDirection(90);
            }
            return;
        }

        var x = block.X;
        var y = block.Y;
        var x1 = snake.Blocks[blockIdx - 1].X;
        var y1 = snake.Blocks[blockIdx - 1].Y;
        var x2 = snake.Blocks[blockIdx + 1].X;
        var y2 = snake.Blocks[blockIdx + 1].Y;
        var left = x1 < x || x2 < x;
        var right = x1 > x || x2 > x;
        var top = y1 > y || y2 > y;
        var bottom = y1 < y || y2 < y;

        if (top && bottom) {
            SetCube();
            SetDirection(0);
        }
        else if (left && right) {
            SetCube();
            SetDirection(90);
        }
        else if (top && right) {
            SetCorner();
            SetDirection(180);
        }
        else if (bottom && left) {
            SetCorner();
            SetDirection(0);
        }
        else if (bottom && right) {
            SetCorner();
            SetDirection(270);
        }
        else {
            // top && left
            SetCorner();
            SetDirection(90);
        }
    }

    void SetDirection(float direction) {
        var curr = transform.localRotation.eulerAngles;
        transform.localRotation = Quaternion.Euler(curr.x, direction, curr.z);
    }

    void SetCube() {
        _cube.SetActive(true);
        _corner?.SetActive(false);
    }

    void SetCorner() {
        _cube.SetActive(false);
        _corner.SetActive(true);
    }
}
