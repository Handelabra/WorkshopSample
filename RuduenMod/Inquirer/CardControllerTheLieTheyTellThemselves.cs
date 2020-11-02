using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.RuduenFanMods.Inquirer
{
    public class CardControllerTheLieTheyTellThemselves : RuduenCardController
    {
        public CardControllerTheLieTheyTellThemselves(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }
		
        public override void AddTriggers()
        {
			// Add trigger for destroyed Distortion.
			base.AddTrigger<DestroyCardAction>((DestroyCardAction d) => d.CardToDestroy.Card.IsDistortion && d.WasCardDestroyed, new Func<DestroyCardAction, IEnumerator>(this.DealDamageResponse), TriggerType.DealDamage, TriggerTiming.After, ActionDescription.Unspecified, false, true, null, false, null, null, false, false);
        }
		private IEnumerator DealDamageResponse(GameAction d)
		{
			// Pick any target
			List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
			IEnumerator coroutine = base.GameController.SelectCardAndStoreResults(this.DecisionMaker, SelectionType.SelectTarget, new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.IsTarget, "targets", false, false, null, null, false), storedResults, false, false, null, true, base.GetCardSource(null));
			yield return RunCoroutine(coroutine);
			Card selectedCard = base.GetSelectedCard(storedResults);

			// That target damages themselves. 
			coroutine = base.DealDamage(selectedCard, selectedCard, 1, DamageType.Psychic, false, false, false, null, null, null, false, null);
			yield return RunCoroutine(coroutine);
		}
		public override IEnumerator UsePower(int index = 0)
		{
			// Draw 2 cards.
			int powerNumeral = base.GetPowerNumeral(0, 2);
			IEnumerator coroutine = base.DrawCards(this.DecisionMaker, powerNumeral, false, false, null, true, null);
			yield return base.RunCoroutine(coroutine);

			// Destroy a distortion.

			coroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController,
				new LinqCardCriteria((Card c) => c.IsDistortion, "distortion", true, false, null, null, false),
				false, null, null, base.GetCardSource(null));
			yield return base.RunCoroutine(coroutine);

		}
	}
}
