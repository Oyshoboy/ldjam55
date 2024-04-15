using System.Collections;
using System.Collections.Generic;
using FIMSpace.Jiggling;
using TMPro;
using UnityEngine;

public class item_controller : MonoBehaviour
{
    [Header("references")]
    public TMP_Text text;
    public GameObject myParent;
    public FJiggling_Simple myJiggler;
    
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    public bool attentionJiggle;
    private float _attentionJiggleInterval = 1.5f;
    private float _lastAttentionJiggleTime;
    
    public Utilities.ElementsType elementType;
    // Start is called before the first frame update
    void Start()
    {
        if (!myParent) return;
        _initialRotation = myParent.transform.rotation;
        _initialPosition = myParent.transform.position;
        
        text.text = elementType.ToString() ?? "Unknown";
    }

    // Update is called once per frame
    void Update()
    {
        AttentionJiggleHandler();
    }

    private void AttentionJiggleHandler()
    {
        if (attentionJiggle && _lastAttentionJiggleTime < Time.time)
        {
            myJiggler.StartJiggle();
            _lastAttentionJiggleTime = Time.time + _attentionJiggleInterval;
        }
    }

    public void ActivateAttentionJiggler()
    {
        attentionJiggle = true;
    }
    
    public void DeactivateAttentionJiggler()
    {
        attentionJiggle = false;
    }

    public void Grab(GameObject grabbingPoint)
    {
        text.gameObject.SetActive(true);
        myParent.transform.position = grabbingPoint.transform.position;
        myParent.transform.rotation = grabbingPoint.transform.rotation;
        myParent.transform.SetParent(grabbingPoint.transform);
        myJiggler.StartJiggle();
    }

    public void Release()
    {
        text.gameObject.SetActive(false);
        myParent.transform.SetParent(null);
        myParent.transform.position = _initialPosition;
        myParent.transform.rotation = _initialRotation;
        myJiggler.StartJiggle();
    }

    public void PlacedToPot()
    {
        text.gameObject.SetActive(false);
        myParent.SetActive(false);
        myParent.transform.SetParent(null);
        myParent.transform.position = _initialPosition;
        myParent.transform.rotation = _initialRotation;
    }
}
