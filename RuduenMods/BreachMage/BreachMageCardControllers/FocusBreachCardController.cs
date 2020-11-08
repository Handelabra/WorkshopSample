using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.BreachMage
{
    public class FocusBreachCardController : CardController
    {
        public FocusBreachCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;
            List<ActivateAbilityDecision> storedResults = new List<ActivateAbilityDecision>();

            // Draw a card.
            coroutine = this.DrawCard(this.HeroTurnTaker);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Search if appropriate.
            if (this.TurnTaker.IsHero)
            {
                coroutine = this.SearchForCards(this.DecisionMaker, true, true, 1, 1,
                    new LinqCardCriteria((Card c) => c.DoKeywordsContain("breach"), "breach", true, false, null, null, false)
                    , true, false, false, false, null, false, null, null);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
            else
            {
                coroutine = this.GameController.SendMessageAction(this.Card.Title + " has no deck or trash to search.", Priority.Medium, this.GetCardSource(null), null, true);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
            // Play a card.
            coroutine = this.SelectAndPlayCardFromHand(this.HeroTurnTakerController, true, null, null, false, false, true, null);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}