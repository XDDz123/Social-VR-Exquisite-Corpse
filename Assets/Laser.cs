using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.XR;

public class Laser : MonoBehaviour
{
    private LineRenderer _laser;
    private Color _color = Color.black;

    public Vector3 PenPosition;
    public Vector3 PenDirection;

    public void UpdateColor(Color color)
    {
        _color = color;
    }

    void Start()
    {
        _laser = GetComponent<LineRenderer>();
    }

    void Update()
    {
        GetComponent<Renderer>().enabled = false;
        RaycastHit hit;

        if (!Physics.Raycast(PenPosition, PenDirection, out hit, 100.0f))
        {
            return;
        }

        MeshCollider meshCollider = hit.collider as MeshCollider;

        if (meshCollider == null)
        {
            return;
        }

        // fixes "Texture coordinate channel "0" not found" error when accessing hit.textureCoord
        // patch inspired by https://github.com/microsoft/MixedRealityToolkit-Unity/issues/10339
        // and https://github.com/microsoft/MixedRealityToolkit-Unity/pull/10370
        Mesh sharedMesh = meshCollider.sharedMesh;

        if (sharedMesh == null || !sharedMesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord0))
        {
            GetComponent<Renderer>().enabled = false;
            return;
        }

        // checks if the hit object is the drawing canvas
        // canvas obj's tag is set to "Canvas" in unity
        if (!meshCollider.CompareTag("Canvas"))
        {
            GetComponent<Renderer>().enabled = false;
            return;
        }

        _laser.startColor = _color;
        _laser.endColor = _color;
        _laser.SetPosition(0, PenPosition);
        _laser.SetPosition(1, hit.point);

        GetComponent<Renderer>().enabled = true;
    }
}
