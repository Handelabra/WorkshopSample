using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using Handelabra.Sentinels.Engine.Controller.TheSentinels;
using System.Collections.Generic;
using Handelabra;

namespace Workshopping.TheSentinels
{
    public class WrattleCharacterCardController : SentinelHeroCharacterCardController
    {
        public WrattleCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

		public override IEnumerator UsePower(int index = 0)
		{
			HeroTurnTaker hero = null;
			string turnTakerName;
			if (this.TurnTaker.IsHero)
			{
				hero = this.TurnTaker.ToHero();
				turnTakerName = this.TurnTaker.Name;
			}
			else
			{
				turnTakerName = this.Card.Title;
			}

			// The next time a hero target is destroyed, you may move it to the owner's hand.
			WhenCardIsDestroyedStatusEffect effect = new WhenCardIsDestroyedStatusEffect(this.CardWithoutReplacements, "MoveItToTheBottomOfItsDeckResponse", "The next time a hero target is destroyed, " + turnTakerName + " may move it to its owner's hand.", new TriggerType[] { TriggerType.MoveCard, TriggerType.ChangePostDestroyDestination }, hero, this.Card);
			effect.CardDestroyedCriteria.IsHero = true;
			effect.CardDestroyedCriteria.IsTarget = true;
			effect.CanEffectStack = false;
			effect.Priority = StatusEffectPriority.Medium;
			effect.PostDestroyDestinationMustBeChangeable = true;
			effect.NumberOfUses = 1;

			return AddStatusEffect(effect);
		}

		public IEnumerator MoveItToTheBottomOfItsDeckResponse(DestroyCardAction d, HeroTurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
		{
			//...you may move it to the bottom of its deck.
			if (d.PostDestroyDestinationCanBeChanged)
			{
				var storedResults = new List<YesNoCardDecision>();
				HeroTurnTakerController httc = null;
				if (hero != null)
				{
					httc = FindHeroTurnTakerController(hero);
				}
				var e = this.GameController.MakeYesNoCardDecision(httc, SelectionType.MoveCardToHand, d.CardToDestroy.Card, storedResults: storedResults, cardSource: GetCardSource());
				if (UseUnityCoroutines)
				{
					yield return this.GameController.StartCoroutine(e);
				}
				else
				{
					this.GameController.ExhaustCoroutine(e);
				}

				if (DidPlayerAnswerYes(storedResults))
				{
					var nativeDeck = d.CardToDestroy.Card.NativeDeck;
					var hand = nativeDeck.OwnerTurnTaker.ToHero().Hand;
					d.SetPostDestroyDestination(hand, false, storedResults.CastEnumerable<YesNoCardDecision, IDecision>(), cardSource: GetCardSource());
				}
			}
		}
	}
}
