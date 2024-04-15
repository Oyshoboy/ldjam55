using System.Collections;
using System.Collections.Generic;
using FIMSpace.Jiggling;
using UnityEngine;

public class item_controller : MonoBehaviour
{
    [Header("references")]
    public GameObject text;
    public GameObject myParent;
    public FJiggling_Simple myJiggler;
    
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    // Start is called before the first frame update
    void Start()
    {
        if (!myParent) return;
        _initialRotation = myParent.transform.rotation;
        _initialPosition = myParent.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Grab(GameObject grabbingPoint)
    {
        text.SetActive(false);
        myParent.transform.position = grabbingPoint.transform.position;
        myParent.transform.rotation = grabbingPoint.transform.rotation;
        myParent.transform.SetParent(grabbingPoint.transform);
        myJiggler.StartJiggle();
    }

    public void Release()
    {
        text.SetActive(true);
        myParent.transform.SetParent(null);
        myParent.transform.position = _initialPosition;
        myParent.transform.rotation = _initialRotation;
        myJiggler.StartJiggle();
    }

    public void PlacedToPot()
    {
        Debug.Log($"{myParent.name} placed to pot");
        myParent.SetActive(false);
    }
}
