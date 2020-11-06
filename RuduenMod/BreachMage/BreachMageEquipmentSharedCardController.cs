using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.BreachMage
{
    public class BreachMageEquipmentSharedCardController : CardController
    {
        public BreachMageEquipmentSharedCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // Return card to hand.
            IEnumerator coroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, base.HeroTurnTaker.Hand);
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}