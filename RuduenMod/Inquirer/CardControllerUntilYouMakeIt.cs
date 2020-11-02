using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.RuduenFanMods.Inquirer
{
    // TODO: TEST!
    public class CardControllerUntilYouMakeIt : CardControllerInquirerDistortionShared
    {
        public CardControllerUntilYouMakeIt(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;
			// Draw card.
			coroutine = base.DrawCard(null, false, null, true);
			yield return base.RunCoroutine(coroutine);

			// Search for form.
			coroutine = base.GameController.SelectCardFromLocationAndMoveIt(base.HeroTurnTakerController, base.TurnTaker.Deck, new LinqCardCriteria((Card c) => c.IsForm, () => "form", true, false, null, null, false), new MoveCardDestination[]
			{
				new MoveCardDestination(base.TurnTaker.PlayArea, false, false, false),
				new MoveCardDestination(base.HeroTurnTaker.Hand, false, false, false)
			}, true, true, true, false, null, false, false, null, false, false, null, null, base.GetCardSource(null));
			yield return base.RunCoroutine(coroutine);

			// Play card.
			coroutine = base.SelectAndPlayCardFromHand(base.HeroTurnTakerController, true, null, null, false, false, true, null);
			yield return base.RunCoroutine(coroutine);
		}
    }
}
