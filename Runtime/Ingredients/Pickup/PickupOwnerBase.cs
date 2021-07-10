using NaughtyAttributes;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace GameplayIngredients.Pickup
{
    [HelpURL(Help.URL + "pickup")]
    public abstract class PickupOwnerBase : GameplayIngredientsBehaviour
    {
        [ReorderableList, NoLabel]
        public string[] acceptPickupTags;

        public bool PickUp(PickupItem pickup)
        {
            // If PickupItem can be accepted
            if (acceptPickupTags != null && acceptPickupTags.Contains(pickup.gameObject.tag))
            {
                // Apply Effects
                foreach (var effect in pickup.effects)
                {
                    effect.ApplyPickupEffect(this);
                }
                return true;
            }
            else
                return false;
        }
    }
}

