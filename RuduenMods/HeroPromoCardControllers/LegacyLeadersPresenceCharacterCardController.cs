using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Legacy
{
    public class LegacyLeadersPresenceCharacterCardController : PromoDefaultCharacterCardController
    {
        public LegacyLeadersPresenceCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            List<PlayCardAction> storedResults = new List<PlayCardAction>();

            IEnumerator coroutine;

            // Play card.
            coroutine = this.GameController.SelectAndPlayCardFromHand(this.HeroTurnTakerController, false, storedResults: storedResults, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (storedResults.Count > 0 && storedResults.FirstOrDefault().IsSuccessful)
            {
                Card card = storedResults.FirstOrDefault().CardToPlay;
                if (card.IsOngoing)
                {
                    // Draw a card.
                    coroutine = this.DrawCards(this.DecisionMaker, 1);
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
            }
        }
    }
}