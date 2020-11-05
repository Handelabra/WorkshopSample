using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.Inquirer
{
    // TODO: TEST!
    public class UntilYouMakeItCardController : CardController
    {
        public UntilYouMakeItCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;
			// Draw card.
			coroutine = base.DrawCard(null, false, null, true);
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

            // Search for form.
            coroutine = base.GameController.SelectCardFromLocationAndMoveIt(base.HeroTurnTakerController, base.TurnTaker.Deck, new LinqCardCriteria((Card c) => c.IsForm, () => "form", true, false, null, null, false), new MoveCardDestination[]
			{
				new MoveCardDestination(base.TurnTaker.PlayArea, false, false, false)
			}, true, true, true, false, null, false, false, null, false, false, null, null, base.GetCardSource(null));
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

            // Play card.
            coroutine = base.SelectAndPlayCardFromHand(base.HeroTurnTakerController, true, null, null, false, false, true, null);
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}
