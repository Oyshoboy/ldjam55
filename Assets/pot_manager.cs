using System.Collections;
using System.Collections.Generic;
using FIMSpace.Jiggling;
using UnityEngine;

public class pot_manager : MonoBehaviour
{
    [Header("References")]
    public FJiggling_Simple myJiggler;
    public ParticleSystem splash;
    public List<item_controller> items = new List<item_controller>();
    public MeshRenderer[] indicators;
    public Material indicatorOn;
    public Material indicatorOff;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PutItem(item_controller item)
    {
        myJiggler.StartJiggle();
        splash.Play();
        items.Add(item);
        
        if(items.Count - 1 > indicators.Length) return;
        indicators[items.Count - 1].material = indicatorOn;
    }
}
