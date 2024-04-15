using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityRecord
{
    public Utilities.EntityType type;
    public GameObject prefab;
}

[CreateAssetMenu(fileName = "EntitiesMenu", menuName = "Menus/Entities Menu")]
public class EntitiesMenu: SerializedScriptableObject
{
    public List<EntityRecord> entities = new List<EntityRecord>();
}
