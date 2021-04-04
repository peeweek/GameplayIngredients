using UnityEngine;

namespace GameplayIngredients.Actions
{
    [HelpURL(Help.URL + "messager")]
    [Callable("Game", "Actions/ic-action-message.png")]
    public class SendMessageAction : ActionBase
    {
        public string MessageToSend = "Message";

        public override void Execute(GameObject instigator = null)
        {
            Messager.Send(MessageToSend, instigator);
        }

        public override string GetDefaultName()
        {
            return $"Send Message : '{MessageToSend}'";
        }
    }
}


