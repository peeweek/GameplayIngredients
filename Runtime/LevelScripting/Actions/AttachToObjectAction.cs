using NaughtyAttributes;
using UnityEngine;

namespace GameplayIngredients.Actions
{
    [AddComponentMenu(ComponentMenu.actionsPath + "Attach To Object Action")]
    [Callable("Game Objects", "Actions/ic-action-attach.png")]
    public class AttachToObjectAction : ActionBase
    {
        public enum Action
        {
            Attach,
            Detach,
            ToggleAttachment
        }

        public Action action = Action.Attach;

        [ReorderableList]
        public GameObject[] objectsToAttach;
        public bool AttachInstigator = false;
        public GameObject parentObject;
        public bool KeepScale;

        public override void Execute(GameObject instigator = null)
        {
            if (parentObject == null)
            {
                Debug.LogWarning("No Object to attach to.");
                return;
            }
            if (objectsToAttach != null)
            {
                foreach (var obj in objectsToAttach)
                    DoAttach(obj, parentObject, action, KeepScale);
            }
            if (AttachInstigator && instigator != null)
                DoAttach(instigator, parentObject, action, KeepScale);

        }

        static void DoAttach(GameObject attachment, GameObject parent, Action action, bool keepScale)
        {
            if(action == Action.Attach || (action == Action.ToggleAttachment && attachment.transform.parent != parent.transform))
            {
                attachment.transform.parent = parent.transform;
            }
            else if(action == Action.Detach || (action == Action.ToggleAttachment && attachment.transform.parent == parent.transform))
            {
                attachment.transform.parent = null;
            }
        }

        public override string GetDefaultName()
        {
            return $"{action} : {(AttachInstigator?"instigator":"objects")} to '{parentObject?.name}'";
        }
    }
}
