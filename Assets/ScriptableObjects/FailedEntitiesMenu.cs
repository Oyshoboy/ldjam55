using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;
using UnityEngine;


[Serializable]
public class FailedEntityRecord
{
    public Utilities.FailedEntityType type;
    public GameObject prefab;
}

[CreateAssetMenu(fileName = "FailedEntitiesMenu", menuName = "Menus/Failed Entities Menu")]
public class FailedEntitiesMenu: SerializedScriptableObject
{
    public List<FailedEntityRecord> failedEntities = new List<FailedEntityRecord>();
}