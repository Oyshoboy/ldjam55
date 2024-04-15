using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public enum SummonStatus
    {
        Ready,
        Busy,
    }
    
    public enum ElementsType
    {
        Unknown,
        Brain,
        Essence,
        Crystal,
        BloodBag,
        Bone,
        Veggies,
        Fang,
        Skin,
        Silver,
        Homunculus,
        Fur,
        Ashes,
    }

    public enum EntityType
    {
        Random,
        KnightKitty,
        Werewolf,
        CrystalGolem,
        Einstein,
        Vampire,
        Butcher,
        Skeleton,
        SwampThing,
        Hawking,
    }
    
    public enum FailedEntityType
    {
        Random,
        Explosion,
        WormBite,
        Skull,
        Screamer
    }
}
