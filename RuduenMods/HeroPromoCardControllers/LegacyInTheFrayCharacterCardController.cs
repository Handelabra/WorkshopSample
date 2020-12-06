using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Legacy
{
    public class LegacyInTheFrayCharacterCardController : PromoDefaultCharacterCardController
    {
        public LegacyInTheFrayCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            List<PlayCardAction> storedResults = new List<PlayCardAction>();

            IEnumerator coroutine;

            // You may play a one-shot. 
            coroutine = this.GameController.SelectAndPlayCardFromHand(this.HeroTurnTakerController, true, storedResults: storedResults,
                cardCriteria: new LinqCardCriteria((Card c) => c.IsOneShot, "one-shot"),
                cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (storedResults.Count == 0 || !storedResults.FirstOrDefault().WasCardPlayed)
            {
                // Draw a card.
                coroutine = this.DrawCards(this.DecisionMaker, 1);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}