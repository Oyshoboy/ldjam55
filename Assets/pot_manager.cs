using System.Collections;
using System.Collections.Generic;
using FIMSpace.Jiggling;
using UnityEngine;

public class pot_manager : MonoBehaviour
{
    [Header("References")]
    public FJiggling_Simple myJiggler;
    public ParticleSystem splash;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ItemPlaced()
    {
        myJiggler.StartJiggle();
        splash.Play();
    }
}