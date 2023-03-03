using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawableSurface : MonoBehaviour
{
    private Material _material;
    private Camera _camera;
    private RenderTexture _texture;
    private Color _color;
    private Vector2? _lastPosition;

    void Start()
    {
        // Create the texture that will be drawn on
        _texture = new RenderTexture(1024, 1024, 24);
        _texture.filterMode = FilterMode.Point;
        _texture.enableRandomWrite = true;
        _texture.Create();

        Graphics.SetRenderTarget(_texture);
        GL.Clear(false, true, Color.white);

        _material = new Material(Shader.Find("DrawableSurface"));
        _material.SetTexture("_MainTex", _texture);

        _color = Color.black;

        GetComponent<Renderer>().material.mainTexture = _texture;

        if ((GetComponent<Collider>() as MeshCollider) == null) {
            gameObject.AddComponent<MeshCollider>();
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out hit, 100.0f)) {
                return;
            }

            if (_lastPosition is Vector2 start) {
                _material.SetVector("_Start", start);
                _material.SetVector("_End", hit.textureCoord);
                _material.SetColor("_Color", _color);
                _material.SetFloat("_BrushSize", 0.01f);

                RenderTexture tmp = RenderTexture.GetTemporary(_texture.width, _texture.height);

                Graphics.Blit(_texture, tmp);
                Graphics.Blit(tmp, _texture, _material);

                RenderTexture.ReleaseTemporary(tmp);
            }

            _lastPosition = hit.textureCoord;
        } else {
            _lastPosition = null;
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(_texture, dest);
    }
}
