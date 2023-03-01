using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawableSurface : MonoBehaviour
{
    private ComputeShader _shader;
    private RenderTexture _texture;
    private Vector4 _color;
    private Vector2? _lastPosition;

    void Start()
    {
        _shader = Instantiate<ComputeShader>(Resources.Load<ComputeShader>("Shader/DrawableSurface"));

        // Create the texture that will be drawn on
        _texture = new RenderTexture(1024, 1024, 24);
        _texture.filterMode = FilterMode.Point;
        _texture.enableRandomWrite = true;
        _texture.Create();

        int clearKernel = _shader.FindKernel("Clear");

        _shader.SetTexture(clearKernel, "canvas", _texture);
        _shader.Dispatch(clearKernel, _texture.width / 8, _texture.height / 8, 1);

        // Setup the game object
        Renderer renderer = GetComponent<Renderer>();
        renderer.material = new Material(renderer.material);
        renderer.material.mainTexture = _texture;

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

            int updateKernel = _shader.FindKernel("Update");

            Vector2 end;

            end.x = hit.textureCoord.x * _texture.width;
            end.y = hit.textureCoord.y * _texture.height;

            if (_lastPosition is Vector2 start) {
                _shader.SetTexture(updateKernel, "canvas", _texture);
                _shader.SetVector("start", start);
                _shader.SetVector("end", end);
                _shader.SetVector("color", _color);
                _shader.SetFloat("size", 4.0f);
                _shader.SetTexture(updateKernel, "canvas", _texture);
                _shader.Dispatch(updateKernel, _texture.width / 8, _texture.height / 8, 1);
            }

            _lastPosition = end;
        } else {
            _lastPosition = null;
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(_texture, dest);
    }
}
