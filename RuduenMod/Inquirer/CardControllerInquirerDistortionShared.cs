using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Workshopping.RuduenFanMods.Inquirer
{
	// Token: 0x0200054D RID: 1357
	public class CardControllerInquirerDistortionShared : RuduenCardController
	{
		protected LinqCardCriteria NextToCriteria { get; set; }
		public CardControllerInquirerDistortionShared(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
		{
			this.NextToCriteria = new LinqCardCriteria((Card c) => c.IsTarget, "targets", false, false, null, null, false);
		}

        public override void AddTriggers()
		{
			// Selfdestruct at start of turn. 
			base.AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, new Func<PhaseChangeAction, IEnumerator>(base.DestroyThisCardResponse), TriggerType.DestroySelf, null, false);

			// Played next to cards - adjust location.
			base.AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToTheirPlayAreaTrigger(true, true);

			// Common On Destroy trigger.
			base.AddWhenDestroyedTrigger(new Func<DestroyCardAction, IEnumerator>(this.OnDestroyResponse), TriggerType.DealDamage);
		}
		
		// 
		public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
		{
			IEnumerator coroutine = base.SelectCardThisCardWillMoveNextTo(this.NextToCriteria, storedResults, isPutIntoPlay, decisionSources);
			yield return base.RunCoroutine(coroutine);
		}

		public override IEnumerator Play()
		{
			Card nextTo = base.GetCardThisCardIsNextTo(true);
			if (nextTo != null)
			{
				yield return ActivateNextToEffect(nextTo);
			}
		}

		// Override on individual instances.
		protected virtual IEnumerator ActivateNextToEffect(Card nextTo)
        {
			yield break;
			//IEnumerator coroutine = base.DealDamage(nextTo, nextTo, 2, DamageType.Psychic, true, false, false, null, null, null, false, null);
			//yield return base.RunCoroutine(coroutine);
			//nextTo = base.GetCardThisCardIsNextTo(true);
			//if (nextTo != null && nextTo.IsInPlayAndHasGameText)
			//{
			//	coroutine = base.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(base.GameController, nextTo), 2, DamageType.Psychic, new int?(1), false, new int?(1), true, false, false, (Card c) => !c.IsHero && c != nextTo, null, null, null, null, false, null, null, false, null, base.GetCardSource(null));
			//	yield return base.RunCoroutine(coroutine);
			//}
		}

		// Override on individual instances.
		protected virtual IEnumerator OnDestroyResponse(DestroyCardAction dc)
        {
			yield break;
        }

	}
}
