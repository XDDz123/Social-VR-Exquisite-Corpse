using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.XR;
using Ubiq.Messaging;

public class Pen : MonoBehaviour, IGraspable
{
    private NetworkContext _context;
    private bool _isOwner;
    private Hand _controller;
    private Transform _nib;

    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;

        public Message(Transform transform)
        {
            position = transform.position;
            rotation = transform.rotation;
        }
    }

    private void Start()
    {
        _context = NetworkScene.Register(this);
        _nib = transform.Find("Grip/Nib");
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<Message>();
        transform.position = data.position;
        transform.rotation = data.rotation;
    }

    private void FixedUpdate()
    {
        if (_isOwner)
        {
            _context.SendJson(new Message(transform));
        }

        RaycastHit hit;

        if (Physics.Raycast(_nib.position, _nib.TransformDirection(Vector3.forward), out hit, 10))
        {
            GameObject obj = hit.transform.gameObject;
            DrawableSurface surf = obj.GetComponent<DrawableSurface>();

            if (surf != null)
            {
                surf.BeginDrawing(hit);
            }
        }
    }

    private void LateUpdate()
    {
        if (_controller != null)
        {
            transform.position = _controller.transform.position;
            transform.rotation = _controller.transform.rotation;
        }
    }

    void IGraspable.Grasp(Hand controller)
    {
        _isOwner = true;
        _controller = controller;
    }

    void IGraspable.Release(Hand controller)
    {
        _isOwner = false;
        _controller = null;
    }
}
