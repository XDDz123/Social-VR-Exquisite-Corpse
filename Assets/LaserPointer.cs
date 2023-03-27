using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPointer : MonoBehaviour
{
    private Vector3 offset;
    private Renderer rend;
    public Color color;
    public float scale;

    // Start is called before the first frame update
    void Start()
    {
        scale = 1;
        color = Color.black;
        offset = new Vector3(0f, 0f, -0.001f);
        GetComponent<Renderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // change sprite color
        rend = GetComponent<Renderer>();
        rend.material.color = color;
        // change sprite size
        this.transform.localScale = new Vector3(0.009f * scale * 0.95f, 0.009f * scale * 0.95f, 1.0f);


        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 100.0f))
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
            transform.position = hit.point + offset;

        }
        else
        {
            GetComponent<Renderer>().enabled = false;
        }
    }

}
