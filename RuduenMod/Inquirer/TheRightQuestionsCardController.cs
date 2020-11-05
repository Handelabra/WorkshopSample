using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.Inquirer
{
    // TODO: TEST!
    public class TheRightQuestionsCardController : CardController
    {
        public TheRightQuestionsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;
            List<DestroyCardAction> storedResultsAction = new List<DestroyCardAction>();

            // Destroy Ongoing/Environment.
            coroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsOngoing || c.IsEnvironment, "ongoing or environment", true, false, null, null, false), false, storedResultsAction, null, base.GetCardSource(null));
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

            if (base.GetNumberOfCardsDestroyed(storedResultsAction) > 0)
            {
                // Play a distortion.
                coroutine = base.SelectAndPlayCardsFromHand(base.HeroTurnTakerController, 1, false, 0, new LinqCardCriteria((Card c) => c.IsDistortion, "distortion", true));
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
            }

            // Bounce one of your distortions.
            coroutine = base.GameController.SelectAndMoveCard(base.HeroTurnTakerController, (Card c) => c.IsDistortion && c.Owner == base.HeroTurnTaker, this.HeroTurnTaker.Hand);
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

        }
    }
}
