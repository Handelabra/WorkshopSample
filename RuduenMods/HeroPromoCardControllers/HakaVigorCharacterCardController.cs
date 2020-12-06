using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Haka
{
    public class HakaVigorCharacterCardController : PromoDefaultCharacterCardController
    {
        public HakaVigorCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            List<PlayCardAction> storedResults = new List<PlayCardAction>();
            int powerNumeral = this.GetPowerNumeral(0, 2); // Amount to draw.

            IEnumerator coroutine;

            // Play card.
            coroutine = this.GameController.SelectAndPlayCardFromHand(this.HeroTurnTakerController, false, storedResults: storedResults, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Draw until you have 3. 
            coroutine = this.DrawCardsUntilHandSizeReached(this.DecisionMaker, powerNumeral);
            if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}