using UnityEngine;

public class Text3D : MonoBehaviour {

    void Start () {
        renderer.material.mainTexture = this.GetComponent<TextMesh>().font.material.mainTexture;
		renderer.material.color = this.GetComponent<TextMesh>().color;
    }
}