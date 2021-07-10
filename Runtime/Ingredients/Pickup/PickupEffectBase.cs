using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayIngredients.Pickup
{
    [HelpURL(Help.URL + "pickup")]
    public abstract class PickupEffectBase : GameplayIngredientsBehaviour
    {
        public abstract void ApplyPickupEffect(PickupOwnerBase owner);
    }
}
