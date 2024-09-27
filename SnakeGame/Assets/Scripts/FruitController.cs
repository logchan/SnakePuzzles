using GameModel;
using UnityEngine;

public class FruitController : MonoBehaviour {
    public GameObject sphere;
    public Fruit fruit;

    void Start() {
        if (fruit.IsEmpty) {
            var material = sphere.GetComponent<MeshRenderer>().material;
            var color = material.GetColor("_Color");
            color.a = 0.5f;
            material.SetColor("_Color", color);
        }
        if (fruit.IsPersistent) {
            var material = sphere.GetComponent<MeshRenderer>().material;
            var color = material.GetColor("_Color");
            color.g = 0.5f;
            color.b = 0.3f;
            material.SetColor("_Color", color);
        }
    }

    void Update() {
        sphere.SetActive(!fruit.IsConsumed);
    }
}
