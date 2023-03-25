using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using System.IO;
using System.Text;

public class DrawableSurface : MonoBehaviour
{
    private NetworkContext context;

    private Material _material;
    private Camera _camera;
    private RenderTexture _texture;
    public Color brushColor;
    private Vector2? _lastPosition;

    private Material _material_full;
    private RenderTexture _texture_full;

    public float brushSize = 0.01f;
    public int player_count = 2;
    public float timer = 10;
    public int player_remaining;
    private bool curr_player_done = false;
    public float time_remaining;
    public bool count_down_start = false;
    private bool game_end = false;

    private Vector2 remote_start;
    private Vector2 remote_end;
    private Color remote_color;
    private float remote_brush_size;

    private NetworkId me;
    private int player_idx;

    private int drawable_side = -1;

    private struct Message
    {
        public Vector2 ms_start;
        public Vector2 ms_end;
        public Color ms_color;
        public float ms_brush_size;
        public int ms_player_remaining;

        public Message(Vector2 start, Vector2 end, Color color, float brushSize, int player_remaining)
        {
            this.ms_start = start;
            this.ms_end = end;
            this.ms_color = color;
            this.ms_brush_size = brushSize;
            this.ms_player_remaining = player_remaining;
        }
    }

    void Start()
    {
        context = NetworkScene.Register(this);
        me = context.Scene.Id;

        // Create the texture that will be drawn on
        _texture = new RenderTexture(1024, 1024, 24);
        _texture.filterMode = FilterMode.Point;
        _texture.enableRandomWrite = true;
        _texture.Create();

        Graphics.SetRenderTarget(_texture);
        GL.Clear(false, true, Color.white);

        _material = new Material(Shader.Find("DrawableSurface"));
        _material.SetTexture("_MainTex", _texture);

        brushColor = Color.black;

        GetComponent<Renderer>().material.mainTexture = _texture;

        if ((GetComponent<Collider>() as MeshCollider) == null)
        {
            gameObject.AddComponent<MeshCollider>();
        }

        // set object to render with shader
        GetComponent<Renderer>().material.shader = Shader.Find("DrawableSurface");

        time_remaining = timer;
        player_remaining = player_count;


        _texture_full = new RenderTexture(1024, 1024, 24);
        _texture_full.filterMode = FilterMode.Point;
        _texture_full.enableRandomWrite = true;
        _texture_full.Create();
        _material_full = new Material(Shader.Find("DrawableSurface"));
        _material_full.SetTexture("_MainTex", _texture_full);

        Graphics.SetRenderTarget(_texture_full);
        GL.Clear(false, true, Color.white);

        Graphics.SetRenderTarget(_texture);
        GL.Clear(false, true, Color.white);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<Message>();
        if (player_remaining == data.ms_player_remaining)
        {
            remote_start = data.ms_start;
            remote_end = data.ms_end;
            remote_color = data.ms_color;
            remote_brush_size = data.ms_brush_size;

            float idx_start = 0.45f;
            float idx_end = 0.55f;
            if (remote_end.x > idx_start && remote_end.x < idx_end)
            {
                DrawOnCanvas(_material, _texture, remote_start, remote_end, remote_color, remote_brush_size);
            }

            DrawOnCanvas(_material_full, _texture_full, remote_start, remote_end, remote_color, remote_brush_size);
        }
        else
        {
            player_remaining = data.ms_player_remaining;
        }
    }


    void LateUpdate()
    {
        GameObject go = GameObject.Find("Player Selector");

        // do nothing when players haven't finished selecting sides
        if (go != null && go.activeSelf)
        {
            return;
        }
        
        if (player_remaining > 0)
        {
            // stops drawing when the current player's timer runs out
            if (!curr_player_done)
            {
                // start timer when the player starts drawing
                // count_down_start is set to true upon first pixel update
                if (count_down_start)
                {
                    if (time_remaining > 0)
                    {
                        time_remaining -= Time.deltaTime;
                    }
                    else
                    {
                        Debug.Log("Time run out!");
                        time_remaining = timer;
                        count_down_start = false;
                        curr_player_done = true;
                        player_remaining -= 1;

                        // dummy vars sent as message, conditioned in ProcessMessage to update only player_remaining
                        context.SendJson(new Message(new Vector2(0,0), new Vector2(0, 0), brushColor, brushSize, player_remaining));
                    }
                }

                if (Input.GetMouseButton(0))
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (!Physics.Raycast(ray, out hit, 100.0f))
                    {
                        // reset _lastPosition whenever raycasting no longer hits the object
                        // i.e. draw line should no longer connect when moving the cursor out of the canvas area
                        _lastPosition = null;
                        return;
                    }

                    // fixes "Texture coordinate channel "0" not found" error when accessing hit.textureCoord
                    // patch inspired by https://github.com/microsoft/MixedRealityToolkit-Unity/issues/10339
                    // and https://github.com/microsoft/MixedRealityToolkit-Unity/pull/10370
                    MeshCollider meshCollider = hit.collider as MeshCollider;

                    if (meshCollider == null)
                    {
                        _lastPosition = null;
                        return;
                    }

                    Mesh sharedMesh = meshCollider.sharedMesh;
                    if (sharedMesh == null || !sharedMesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord0))
                    {
                        _lastPosition = null;
                        return;
                    }

                    // checks if the hit object is the drawing canvas
                    // canvas obj's tag is set to "Canvas" in unity

                    if (!meshCollider.CompareTag("Canvas"))
                    {
                        _lastPosition = null;
                        return;
                    }

                    if (_lastPosition is Vector2 start)
                    {
                        if (drawable_side == -1)
                        {
                            _lastPosition = null;
                            return;
                        }

                        // start the timer since we begin drawing here
                        if (!count_down_start)
                        {
                            count_down_start = true;
                        }

                        player_idx = drawable_side;

                        // limit which side the player can draw on
                        if (hit.textureCoord.x > 1.0f / player_count * player_idx || hit.textureCoord.x < 1.0f / player_count * (player_idx - 1.0f))
                        {
                            _lastPosition = null;
                            return;
                        }

                        context.SendJson(new Message(start, hit.textureCoord, brushColor, brushSize, player_remaining));

                        DrawOnCanvas(_material, _texture, start, hit.textureCoord, brushColor, brushSize);
                        DrawOnCanvas(_material_full, _texture_full, start, hit.textureCoord, brushColor, brushSize);
                    }

                    _lastPosition = hit.textureCoord;
                }
                else
                {
                    _lastPosition = null;
                }
            }
        }
        else
        {
            if (!game_end)
            {
                // do stuff
                Graphics.SetRenderTarget(_texture_full);
                GetComponent<Renderer>().material.mainTexture = _texture_full;
                game_end = true;
            }
        }
    }

    public void side(int s)
    {
        // s = 1 right
        // s = 2 left
        drawable_side = s;
    }

    void DrawOnCanvas(Material _material, RenderTexture _texture, Vector2 start, Vector2 end, Color brushColor, float brushSize)
    {
        Graphics.SetRenderTarget(_texture);

        _material.SetVector("_Start", start);
        _material.SetVector("_End", end);
        _material.SetColor("_Color", brushColor);
        _material.SetFloat("_BrushSize", brushSize);

        RenderTexture tmp = RenderTexture.GetTemporary(_texture.width, _texture.height);

        Graphics.Blit(_texture, tmp);
        Graphics.Blit(tmp, _texture, _material);

        RenderTexture.ReleaseTemporary(tmp);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(_texture, dest);
    }
}
