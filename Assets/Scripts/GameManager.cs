using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public GameObject hand;
    public Animator pointerAnimator;
    public Camera mainCamera;

    public GameObject trackingPoint;
    public Transform lookAtPoint;
    
    [Header("Configurations")]
    public float pointerDistance = 3f;

    private Vector3 _average;
    private Vector3 _lastPosition;
    private bool _isOnScreen;
    private bool _isGrabbing;
    
    
    public int listSize = 10;
    private List<Vector3> _trackerPositions = new List<Vector3>();
    private void Start()
    {
        if (!mainCamera)
        {
            mainCamera = Camera.main;
        }
    }


    private void Update()
    {
        HandleTrackerPosition();
        HandleHandPosition();
        HandleAnimationState();
    }

    private void HandleAnimationState()
    {
        if(Input.GetMouseButtonDown(0) && !_isGrabbing)
        {
            pointerAnimator.SetBool("Natural", false);
            pointerAnimator.SetBool("GrabLarge", true);
            _isGrabbing = true;
        }
        else if((Input.GetMouseButtonUp(0) || !_isOnScreen) && _isGrabbing)
        {
            pointerAnimator.SetBool("GrabLarge", false);
            pointerAnimator.SetBool("Natural", true);
            _isGrabbing = false;
        }
    }

    private void HandleHandPosition()
    {
        // hand is following the pointer
        if (_trackerPositions.Count > 0)
        {
            hand.transform.position = _average;
        }
        
        // rotated towards camera
        Vector3 direction = lookAtPoint.transform.position - hand.transform.position;
        hand.transform.rotation = Quaternion.LookRotation(direction);
    }

    private void HandleTrackerPosition()
    {
       // make 3d hand following the pointer on screen projection
       Vector3 mousePosition = Input.mousePosition;
       mousePosition.z = pointerDistance;
       Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
       
       // check if the pointer out of screen then set it to the last position
       var screenPoint = mainCamera.WorldToViewportPoint(worldPosition);
       _isOnScreen = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
       
       if (!_isOnScreen)
       {
           worldPosition = _lastPosition;
       }
       else
       {
           _lastPosition = worldPosition;
       }
       
       trackingPoint.transform.position = worldPosition;
       _trackerPositions.Add(worldPosition);
       _average += worldPosition / listSize;

       if (_trackerPositions.Count > listSize)
       {
           _trackerPositions.RemoveAt(0);
           _average -= _trackerPositions[0] / listSize;
       }
    }
}
