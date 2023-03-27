using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Rooms;
using System;
using UnityEditor;

public class DrawableSurface : MonoBehaviour
{
    private NetworkContext context;

    private Material _material;
    private RenderTexture _texture;

    private Material _material_full;
    private RenderTexture _texture_full;

    private Vector2? _lastPosition;
    private Color _brushColor = Color.black;
    private float _brushSize = 0.01f;

    public int player_count = 2;
    public float timer = 10;
    public int player_remaining;
    public bool curr_player_done;
    public float time_remaining;
    public bool count_down_start;
    private bool game_end;

    private Vector2 remote_start;
    private Vector2 remote_end;
    private Color remote_color;
    private float remote_brush_size;

    private NetworkId me;
    private List<NetworkId> curr_players;

    private int _drawableSide;

    private Texture2D local_tex;

    public Vector3 PenPosition;
    public Vector3 PenDirection;
    public bool drawing;

    private struct Message
    {
        public Vector2 ms_start;
        public Vector2 ms_end;
        public Color ms_color;
        public float ms_brush_size;
        public int ms_player_remaining;
        public int flag;
        public List<NetworkId> players;

        public Message(int flag, Vector2 start, Vector2 end, Color color, float brushSize, int player_remaining, List<NetworkId> players)
        {
            this.flag = flag;
            this.ms_start = start;
            this.ms_end = end;
            this.ms_color = color;
            this.ms_brush_size = brushSize;
            this.ms_player_remaining = player_remaining;
            this.players = players;
        }
    }

    public void Start()
    {
        context = NetworkScene.Register(this);
        me = context.Scene.Id;

        local_tex = new Texture2D(1024, 1024);

        curr_players = new List<NetworkId>();

        _drawableSide = -1;
        curr_player_done = false;
        count_down_start = false;
        game_end = false;

        // Create the texture that will be drawn on
        _texture = new RenderTexture(1024, 1024, 24);
        _texture.filterMode = FilterMode.Point;
        _texture.enableRandomWrite = true;
        _texture.Create();

        Graphics.SetRenderTarget(_texture);
        GL.Clear(false, true, Color.white);

        Vector2 textureScale = new Vector2(transform.localScale.x, transform.localScale.z);

        _material = new Material(Shader.Find("DrawableSurface"));
        _material.mainTexture = _texture;
        _material.mainTextureScale = textureScale;

        GetComponent<Renderer>().material = _material;

        if ((GetComponent<Collider>() as MeshCollider) == null)
        {
            gameObject.AddComponent<MeshCollider>();
        }

        player_remaining = player_count;

        _texture_full = new RenderTexture(_texture.width, _texture.height, 24);
        _texture_full.filterMode = FilterMode.Point;
        _texture_full.enableRandomWrite = true;
        _texture_full.Create();

        _material_full = new Material(Shader.Find("DrawableSurface"));
        _material_full.mainTexture = _texture_full;
        _material_full.mainTextureScale = textureScale;

        Graphics.SetRenderTarget(_texture_full);
        GL.Clear(false, true, Color.white);

        Graphics.SetRenderTarget(_texture);
        GL.Clear(false, true, Color.white);

        RoomClient.Find(this).OnJoinedRoom.AddListener(OnRoom);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<Message>();

        if (data.flag == 0)
        {
            // if the current player is drawing on some side
            // then send the current drawing over
            if (curr_players.Contains(me))
            {
                // port render texture from gpu to rexture2d in cpu
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = _texture_full;
                local_tex.ReadPixels(new Rect(0, 0, _texture.width, _texture.height), 0, 0);
                local_tex.Apply();
                RenderTexture.active = prev;

                // save the current drawing as png
                // https://answers.unity.com/questions/1331297/how-to-save-a-texture2d-into-a-png.html
                byte[] bytes = local_tex.EncodeToPNG();
                var dirPath = Application.dataPath + "/../TempImages/";
                if (!System.IO.Directory.Exists(dirPath))
                {
                    System.IO.Directory.CreateDirectory(dirPath);
                }
                System.IO.File.WriteAllBytes(dirPath + "image" + ".png", bytes);

                Vector2 temp = new Vector2(0, 0);
                // send the entire drawing when requesting state on join
                context.SendJson(new Message(1, temp, temp, _brushColor, 0f, 0, curr_players));
            }
        }
        else if (data.flag == 1)
        {
            //Debug.Log(data.players.Count);
            // if the current player is not participating
            // then update their canvas with the received texture
            if (!data.players.Contains(me))
            {
                byte[] imgData;
                string path = Application.dataPath + "/../TempImages/image.png";

                // load saved temp image as texture
                if (System.IO.File.Exists(path))
                {
                    imgData = System.IO.File.ReadAllBytes(path);
                    local_tex.LoadImage(imgData);
                    Graphics.Blit(local_tex, _texture_full);
                }

                // display both full canvas when spectating
                if (data.players.Count == 2)
                {
                    Graphics.SetRenderTarget(_texture_full);
                    GetComponent<Renderer>().material.mainTexture = _texture_full;
                }
            }
        }
        else
        {
            if (player_remaining == data.ms_player_remaining)
            {
                remote_start = data.ms_start;
                remote_end = data.ms_end;
                remote_color = data.ms_color;
                remote_brush_size = data.ms_brush_size;

                foreach (NetworkId i in data.players)
                {
                    if (!curr_players.Contains(i))
                    {
                        curr_players.Add(i);
                    }
                }

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
    }

    public void UpdateColor(Color color)
    {
        _brushColor = color;
    }

    public void UpdateBrushSize(float size)
    {
        _brushSize = 0.01f * size;
    }

    void LateUpdate()
    {
        // GameObject go = GameObject.Find("Player Selector");
        GameObject go = null;

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
                        context.SendJson(new Message(2, new Vector2(0,0), new Vector2(0, 0),
                                         _brushColor, _brushSize, player_remaining, curr_players));
                    }
                }

                if (drawing || Input.GetMouseButton(0))
                {
                    RaycastHit hit;
                    bool rayHit = false;

                    if (drawing) {
                        rayHit = Physics.Raycast(PenPosition, PenDirection, out hit, 100.0f);
                    } else {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        rayHit = Physics.Raycast(ray, out hit, 100.0f);
                    }

                    if (!rayHit)
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

                    Vector2 end = hit.textureCoord;

                    if (_lastPosition is Vector2 start)
                    {
                        if (_drawableSide == -1)
                        {
                            _lastPosition = null;
                            return;
                        }

                        // limit which side the player can draw on
                        if (end.x > 1.0f / player_count * _drawableSide ||
                            end.x < 1.0f / player_count * (_drawableSide - 1.0f))
                        {
                            _lastPosition = null;
                            return;
                        }

                        // start the timer since we begin drawing here
                        if (!count_down_start)
                        {
                            count_down_start = true;
                            time_remaining = timer;

                            if (!curr_players.Contains(me))
                            {
                                curr_players.Add(me);
                            }
                        }

                        context.SendJson(new Message(2, start, end, _brushColor, _brushSize,
                                                     player_remaining, curr_players));

                        DrawOnCanvas(_material, _texture, start, end, _brushColor, _brushSize);
                        DrawOnCanvas(_material_full, _texture_full, start, end, _brushColor, _brushSize);
                    }

                    _lastPosition = end;
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

    public void Side(int s)
    {
        // s = 1 right
        // s = 2 left
        _drawableSide = s;
    }

    void DrawOnCanvas(Material material, RenderTexture texture, Vector2 start, Vector2 end, Color brushColor, float brushSize)
    {
        Graphics.SetRenderTarget(_texture);

        float aspectRatio = material.mainTextureScale.x / material.mainTextureScale.y;

        brushSize /= aspectRatio;
        start *= material.mainTextureScale;
        end *= material.mainTextureScale;

        material.SetVector("_Start", start);
        material.SetVector("_End", end);
        material.SetColor("_Color", brushColor);
        material.SetFloat("_BrushSize", brushSize);

        RenderTexture tmp = RenderTexture.GetTemporary(texture.width, texture.height);

        Graphics.Blit(texture, tmp);
        Graphics.Blit(tmp, texture, material);

        RenderTexture.ReleaseTemporary(tmp);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(_texture, dest);
    }

    void OnRoom(IRoom other)
    {
        // request info when joining room
        Vector2 temp = new Vector2(0, 0);
        context.SendJson(new Message(0, temp, temp, _brushColor, 0f, 0, curr_players));
    }
}
