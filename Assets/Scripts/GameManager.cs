using System;
using System.Collections;
using System.Collections.Generic;
using FIMSpace.Jiggling;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public GameObject hand;
    public Animator pointerAnimator;
    public Camera mainCamera;
    public GameObject handCart;

    public GameObject trackingPoint;
    public Transform lookAtPoint;
    public GameObject grabbingPoint;
    
    [Header("Configurations")]
    public float pointerDistance = 3f;
    public Vector3 handOnGrabRotation = new Vector3(0, 0, 0);
    
    public LayerMask grabMask;
    public LayerMask potMask;

    private Vector3 _average;
    private Vector3 _lastPosition;
    private bool _isOnScreen;
    private bool _isGrabbing;
    private item_controller _grabbedItem;
    
    
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

    // ReSharper disable Unity.PerformanceAnalysis
    private void HandleAnimationState()
    {
        if(Input.GetMouseButtonDown(0) && !_isGrabbing)
        {
            pointerAnimator.SetBool("Natural", false);
            pointerAnimator.SetBool("GrabLarge", true);
            _isGrabbing = true;
            GrabCheck();
        }
        else if((Input.GetMouseButtonUp(0) || !_isOnScreen) && _isGrabbing)
        {
            pointerAnimator.SetBool("GrabLarge", false);
            pointerAnimator.SetBool("Natural", true);
            _isGrabbing = false;
            
            if (!_grabbedItem) return;
            
            
            var potDetected = PotDetector();
            if (potDetected)
            {
                _grabbedItem.PlacedToPot();
                potDetected.PutItem(_grabbedItem);
            } else
            {
                _grabbedItem.Release();
            }
            
            MoveHandWhenGrabbing(false);
            _grabbedItem = null;
        }
    }

    private pot_manager PotDetector()
    {
        var from = mainCamera.transform.position;
        // forward from camera
        var direction = trackingPoint.transform.position - from;
        Ray ray = new Ray(from, direction);
        RaycastHit hit;
        
        var layerMaskInt = potMask.value;
        Debug.Log("pot detector");
        
        if (Physics.SphereCast(ray, .1f, out hit, 20f, layerMaskInt))
        {
            if (hit.collider.CompareTag("pot"))
            {
                var potController = hit.collider.GetComponent<pot_manager>();
                return potController;
            }
        }
        return null;
    }

    private void GrabCheck()
    {
        var from = mainCamera.transform.position;
        // forward from camera
        var direction = trackingPoint.transform.position - from;
        Ray ray = new Ray(from, direction);
        RaycastHit hit;

        var layerMaskInt = grabMask.value;
        
        
        if (Physics.SphereCast(ray, .1f, out hit, 20f, layerMaskInt))
        {
            if (hit.collider.CompareTag("grabbable") && !_grabbedItem)
            {
                var hitGrabController = hit.collider.GetComponent<item_controller>();
                if (hitGrabController)
                {
                    MoveHandWhenGrabbing(true);
                    hitGrabController.Grab(grabbingPoint);
                    _grabbedItem = hitGrabController;
                }
            }
            
            Debug.Log(hit.collider.name);
        }
    }

    private void MoveHandWhenGrabbing(bool b)
    {
        if (b)
        {
            handCart.transform.localEulerAngles = handOnGrabRotation;
        } else {
            handCart.transform.localEulerAngles = Vector3.zero;
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
