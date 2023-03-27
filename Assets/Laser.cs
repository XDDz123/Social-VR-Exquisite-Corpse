using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.XR;

public class Laser : MonoBehaviour
{
    private LineRenderer laser;
    private Hand controller;
    public Color color;
    public Vector3 PenPosition;
    public Vector3 PenDirection;

    // Start is called before the first frame update
    void Start()
    {
        color = Color.black;
        laser = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

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

            laser.startColor = color;
            laser.endColor = color;
            laser.SetPosition(0, PenPosition);
            laser.SetPosition(1, hit.point);

            GetComponent<Renderer>().enabled = true;

        }
        else
        {
            GetComponent<Renderer>().enabled = false;
        }
    }
}
