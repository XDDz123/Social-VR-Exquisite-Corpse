using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEditor;

public class DrawableSurface : MonoBehaviour
{
    private NetworkContext context;

    private Material _material;
    private RenderTexture _texture;

    private Material _materialFull;
    private RenderTexture _textureFull;

    private Vector2? _lastPosition;
    private Color _brushColor = Color.black;
    private float _brushSize = 0.01f;

    private GameSystem.State _gameState;

    private NetworkId _me;
    private List<NetworkId> _currPlayers;

    private int _drawableSide;

    private Texture2D _localTexture;

    public int playerCount = 2;

    // TODO: clean this up
    public Vector3 PenPosition;
    public Vector3 PenDirection;
    public bool drawing;

    [Serializable]
    public class DrawingBegunEvent : UnityEvent {};
    public DrawingBegunEvent onDrawingBegun = new DrawingBegunEvent();

    [Serializable]
    public class ActivePlayerLeftEvent : UnityEvent {};
    public ActivePlayerLeftEvent onActivePlayerLeft  = new ActivePlayerLeftEvent();

    [Serializable]
    private struct DrawArgs
    {
        public Vector2 start;
        public Vector2 end;
        public Color brushColor;
        public float brushSize;
    }

    [Serializable]
    private struct PlayerArgs
    {
        public bool request;
        public NetworkId id;
    }

    [Serializable]
    private struct TextureArgs
    {
        public bool request;
        public string tex;
    }

    private enum MessageType
    {
        Unknown,
        Draw,
        Player,
        Textures
    }

    [Serializable]
    private struct Message
    {
        public MessageType type;
        public string args;

        public Message(object obj)
        {
            if (obj.GetType().Equals(typeof(DrawArgs))) {
                type = MessageType.Draw;
            } else if (obj.GetType().Equals(typeof(PlayerArgs))) {
                type = MessageType.Player;
            } else if (obj.GetType().Equals(typeof(TextureArgs)))
            {
                type = MessageType.Textures;
            } else {
                type = MessageType.Unknown;
            }

            args = JsonUtility.ToJson(obj);
        }
    }

    public void Start()
    {
        context = NetworkScene.Register(this);
        _me = context.Scene.Id;

        // Create the texture that will be drawn on
        _texture = new RenderTexture(1024, 1024, 24);
        _texture.filterMode = FilterMode.Point;
        _texture.enableRandomWrite = true;
        _texture.Create();

        Vector2 textureScale = new Vector2(transform.localScale.x, transform.localScale.z);

        _material = new Material(Shader.Find("DrawableSurface"));
        _material.mainTexture = _texture;
        _material.mainTextureScale = textureScale;

        GetComponent<Renderer>().material = _material;

        if ((GetComponent<Collider>() as MeshCollider) == null)
        {
            gameObject.AddComponent<MeshCollider>();
        }

        _textureFull = new RenderTexture(_texture.width, _texture.height, 24);
        _textureFull.filterMode = FilterMode.Point;
        _textureFull.enableRandomWrite = true;
        _textureFull.Create();

        _materialFull = new Material(Shader.Find("DrawableSurface"));
        _materialFull.mainTexture = _textureFull;
        _materialFull.mainTextureScale = textureScale;

        _localTexture = new Texture2D(_texture.width, _texture.height);

        RoomClient.Find(this).OnJoinedRoom.AddListener(OnRoom);
        RoomClient.Find(this).OnPeerRemoved.AddListener(OnLeave);

        Reset();
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var container = JsonUtility.FromJson<Message>(msg.ToString());

        switch(container.type)
        {
            case MessageType.Draw:
                HandleDrawMessage(JsonUtility.FromJson<DrawArgs>(container.args));
                break;

            case MessageType.Player:
                HandlePlayerMessage(JsonUtility.FromJson<PlayerArgs>(container.args));
                break;

            case MessageType.Textures:
                HandleTextureMessage(JsonUtility.FromJson<TextureArgs>(container.args));
                break;

        }
    }

    private void HandleTextureMessage(TextureArgs args)
    {
        if (args.request)
        {
            // if the current player is drawing on some side
            // then send the current drawing over
            if (_currPlayers.Contains(_me))
            {
                // port render texture from gpu to rexture2d in cpu
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = _textureFull;
                _localTexture.ReadPixels(new Rect(0, 0, _texture.width, _texture.height), 0, 0);
                _localTexture.Apply();
                RenderTexture.active = prev;

                byte[] bytes = _localTexture.EncodeToPNG();

                context.SendJson(new Message(new TextureArgs()
                {
                    request = false,
                    tex = System.Convert.ToBase64String(bytes)
                }));
            }
        }
        else
        {
            _localTexture.LoadImage(System.Convert.FromBase64String(args.tex));
            Graphics.Blit(_localTexture, _textureFull);
        }
    }

    private void HandleDrawMessage(DrawArgs args)
    {

        if (args.end.x > 0.45f && args.end.x < 0.55f)
        {
            DrawOnCanvas(_material, _texture, args.start, args.end, args.brushColor, args.brushSize);
        }

        DrawOnCanvas(_materialFull, _textureFull, args.start, args.end, args.brushColor,
                        args.brushSize);
    }

    private void HandlePlayerMessage(PlayerArgs args)
    {
        if(!_currPlayers.Contains(args.id)) {
            Debug.Log("Player " + args.id + " joined");
            _currPlayers.Add(args.id);
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

    public void UpdateSide(int side)
    {
        // s = 1 right
        // s = 2 left
        _drawableSide = side;

        if (!_currPlayers.Contains(_me))
        {
            _currPlayers.Add(_me);
            context.SendJson(new Message(new PlayerArgs()
            {
                request = false,
                id = _me,
            }));
        }
    }

    public void OnGameStateChanged(GameSystem.State state)
    {
        _gameState = state;

        switch(state)
        {
            case GameSystem.State.Prepare:
                Reset();
                break;

            case GameSystem.State.InProgress:
                if (!_currPlayers.Contains(_me)) {
                    GetComponent<Renderer>().material.mainTexture = _textureFull;
                }
                break;

            case GameSystem.State.Finished:
                GetComponent<Renderer>().material.mainTexture = _textureFull;
                break;
        }
    }

    void LateUpdate()
    {
        switch(_gameState)
        {
            case GameSystem.State.Prepare:
                if (_drawableSide != -1 && _currPlayers.Count == playerCount) {
                    onDrawingBegun?.Invoke();
                }
                return;

            case GameSystem.State.Finished:
                return;
        }

        if (!drawing && !Input.GetMouseButton(0))
        {
            _lastPosition = null;
            return;
        }

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

        if (!meshCollider.CompareTag("Canvas"))
        {
            _lastPosition = null;
            return;
        }

        Vector2 end = hit.textureCoord;

        if (_lastPosition is Vector2 start)
        {
            // limit which side the player can draw on
            if (end.x > 1.0f / playerCount * _drawableSide ||
                end.x < 1.0f / playerCount * (_drawableSide - 1.0f))
            {
                _lastPosition = null;
                return;
            }

            context.SendJson(new Message(new DrawArgs()
            {
                start = start,
                end = end,
                brushColor = _brushColor,
                brushSize = _brushSize
            }));

            DrawOnCanvas(_material, _texture, start, end, _brushColor, _brushSize);
            DrawOnCanvas(_materialFull, _textureFull, start, end, _brushColor, _brushSize);
        }

        _lastPosition = end;
    }

    private void DrawOnCanvas(Material material, RenderTexture texture, Vector2 start, Vector2 end,
                              Color brushColor, float brushSize)
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
        Reset();
        context.SendJson(new Message(new TextureArgs()
        {
            request = true,
            tex = null
        }));
    }

    void OnLeave(IPeer other)
    {
        if (_currPlayers.Contains(other.networkId))
        {
            onActivePlayerLeft?.Invoke();
        }
    }

    private void Reset()
    {
        _gameState = GameSystem.State.Prepare;
        _drawableSide = -1;
        _currPlayers = new List<NetworkId>();

        Graphics.SetRenderTarget(_texture);
        GL.Clear(false, true, Color.white);

        Graphics.SetRenderTarget(_textureFull);
        GL.Clear(false, true, Color.white);

        GetComponent<Renderer>().material.mainTexture = _texture;
    }
}
