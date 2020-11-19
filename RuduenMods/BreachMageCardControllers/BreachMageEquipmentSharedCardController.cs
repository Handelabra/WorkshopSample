using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.BreachMage
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
            IEnumerator coroutine = this.GameController.MoveCard(this.TurnTakerController, this.Card, this.HeroTurnTaker.Hand);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}