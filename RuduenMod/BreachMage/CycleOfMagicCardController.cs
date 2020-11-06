using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Workshopping.BreachMage
{
    public class CycleOfMagicCardController : CardController
    {
        public CycleOfMagicCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;
            List<ActivateAbilityDecision> storedResults = new List<ActivateAbilityDecision>();

            // Draw 2 cards.
            coroutine = this.DrawCards(this.HeroTurnTakerController, 2);
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

            // Use a Cast. 
            coroutine = base.GameController.SelectAndActivateAbility(base.HeroTurnTakerController, "cast", null, storedResults, false, base.GetCardSource(null));
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

            if (storedResults.Count > 0)
            {
                // Move the top card back to top of deck.
                coroutine = base.GameController.MoveCard(base.HeroTurnTakerController, storedResults.FirstOrDefault().SelectedCard, base.HeroTurnTaker.Deck);
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}
