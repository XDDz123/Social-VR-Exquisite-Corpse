using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPointer : MonoBehaviour
{
    private Vector3 _offset = new Vector3(0.0f, 0.0f, -0.001f);
    private Color _color = Color.black;
    private float _scale = 1;
    public Vector3 PenPosition;
    public Vector3 PenDirection;

    public void UpdateColor(Color color)
    {
        _color = color;
    }

    public void UpdateScale(float size)
    {
        _scale = size;
    }

    void Start()
    {
        GetComponent<Renderer>().enabled = false;
    }

    void Update()
    {
        // change sprite color
        GetComponent<Renderer>().material.color = _color;

        // change sprite size
        transform.localScale = new Vector3(0.009f * _scale * 0.6f, 0.009f * _scale * 0.6f, 1.0f);

        RaycastHit hit;

        if (Physics.Raycast(PenPosition, PenDirection, out hit, 100.0f))
        {

            MeshCollider meshCollider = hit.collider as MeshCollider;
 
            if (meshCollider == null)
            {
                GetComponent<Renderer>().enabled = false;
                return;
            }

            GetComponent<Renderer>().enabled = false;

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

            GetComponent<Renderer>().enabled = true;
            transform.position = hit.point + _offset;

        }
        else
        {
            GetComponent<Renderer>().enabled = false;
        }
    }

}
