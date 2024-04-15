using System.Collections;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class CombinationRecords
{
    public Recipe recipe;
    public Utilities.EntityType result;
}

[Serializable]
public class Recipe
{
    public Utilities.ElementsType itemOne;
    public Utilities.ElementsType itemTwo;
    public Utilities.ElementsType itemThree;
}

[CreateAssetMenu(fileName = "CombinationsMenu", menuName = "Menus/Combinations Menu")]
public class CombinationsMenu: SerializedScriptableObject
{
    public List<CombinationRecords> combinations = new List<CombinationRecords>();
}
