using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.RuduenFanMods.BreachMage
{
    public class WorstCardEverCardController : CardController
    {
        public WorstCardEverCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            var message = GameController.SendMessageAction("You knew this card does nothing. Why would you play it?", Priority.Medium, GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(message);
            }
            else
            {
                GameController.ExhaustCoroutine(message);
            }
        }
    }
}
