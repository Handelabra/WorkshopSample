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
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

            // Search if appropriate.
            if (base.TurnTaker.IsHero)
            {
                coroutine = base.SearchForCards(this.DecisionMaker, true, true, 1, 1,
                    new LinqCardCriteria((Card c) => c.DoKeywordsContain("breach"), "breach", true, false, null, null, false)
                    , true, false, false, false, null, false, null, null);
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
            }
            else
            {
                coroutine = base.GameController.SendMessageAction(base.Card.Title + " has no deck or trash to search.", Priority.Medium, base.GetCardSource(null), null, true);
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
            }
            // Play a card.
            coroutine = base.SelectAndPlayCardFromHand(base.HeroTurnTakerController, true, null, null, false, false, true, null);
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}