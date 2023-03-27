using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.XR;

public class Pen : MonoBehaviour, IGraspable, IUseable
{
    private Hand controller;
    public GameObject _canvas;
    public GameObject _laser_pointer;
    public GameObject _laser;
    private Transform _nib;
    private DrawableSurface ds;

    void Start()
    {
        ds = _canvas.GetComponent<DrawableSurface>();
        _nib = transform.Find("Grip/Nib");
    }

    private void LateUpdate()
    {
        if (controller)
        {
            transform.position = controller.transform.position;
            transform.rotation = controller.transform.rotation;

            ds.PenPosition = _nib.position;
            ds.PenDirection = _nib.TransformDirection(Vector3.forward);

            Laser ls = _laser.GetComponent<Laser>();
            ls.PenPosition = _nib.position;
            ls.PenDirection = _nib.TransformDirection(Vector3.forward);

            LaserPointer lp = _laser_pointer.GetComponent<LaserPointer>();
            lp.PenPosition = _nib.position;
            lp.PenDirection = _nib.TransformDirection(Vector3.forward);
        }
    }

    void IGraspable.Grasp(Hand controller)
    {
        this.controller = controller;
    }

    void IGraspable.Release(Hand controller)
    {
        this.controller = null;
    }

    void IUseable.Use(Hand controller)
    {
        ds.drawing = true;
    }

    void IUseable.UnUse(Hand controller)
    {
        ds.drawing = false;
    }
}
