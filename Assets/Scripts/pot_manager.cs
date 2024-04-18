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
    public Material indicatorWrong;
    public Material indicatorOff;
    public GameManager gameManager;

    public void PutItem(item_controller item)
    {
        myJiggler.StartJiggle();
        splash.Play();
        items.Add(item);

        if (items.Count == 3)
        {
            gameManager.StartSummoning(items);
        }
        
        if(items.Count > indicators.Length) return;
        
        var itemsNeeded = gameManager.GetItemsNeeded();
        var nextIndicatorMaterial = indicatorWrong;

        foreach (var currentItem in itemsNeeded)
        {
            if (currentItem == item.elementType)
            {
                nextIndicatorMaterial = indicatorOn;
            }
        }
        
        indicators[items.Count - 1].material = nextIndicatorMaterial;
    }

    public void ResetIndicators()
    {
        foreach (var indicator in indicators)
        {
            indicator.material = indicatorOff;
        }
    }

    public void ResetAllItems()
    {
        items.Clear();
    }
}
