using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Workshopping.Inquirer
{
	// Token: 0x0200054D RID: 1357
	public class InquirerFormSharedCardController : CardController
	{
		// Token: 0x060028EC RID: 10476 RVA: 0x00023B10 File Offset: 0x00021D10
		public InquirerFormSharedCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
		{
		}

		// Token: 0x060028ED RID: 10477 RVA: 0x00066649 File Offset: 0x00064849
		public override void AddTriggers()
		{
			base.AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, new Func<PhaseChangeAction, IEnumerator>(this.MoveOrDestroyResponse), new TriggerType[]
			{
				TriggerType.MoveCard,
				TriggerType.DestroySelf
			}, null, false);
		}

		// TODO: Change to Select Cards and Move. 
		// TODO: Validate if DecisionMaker should be this.DecisionMaker, or if base.HeroTurnTakerController works well.
		private IEnumerator MoveOrDestroyResponse(PhaseChangeAction phaseChange)
		{
			IEnumerator coroutine;
			// If enough cards exist
			if (base.HeroTurnTaker.Trash.Cards.Count<Card>() >= 2)
            {
				// TODO: Is there a better option for yesnoamountdecision? And can we add the card so it displays?) 
				// Ask if we should move the top two cards of the trash to the bottom of the deck for things. 
				YesNoAmountDecision yesNoDecision = new YesNoAmountDecision(base.GameController, base.HeroTurnTakerController, SelectionType.MoveCard, 2);
				coroutine = base.GameController.MakeDecisionAction(yesNoDecision);
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(coroutine);
				}
				else
				{
					base.GameController.ExhaustCoroutine(coroutine);
				}
				if (base.DidPlayerAnswerYes(yesNoDecision))
                {
					// Fetch top two cards. 
					List<Card> revealedCards = new List<Card>();
					coroutine = this.GameController.RevealCards(base.HeroTurnTakerController, HeroTurnTaker.Trash, 2, revealedCards, false, RevealedCardDisplay.None, null, this.GetCardSource(null));
					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(coroutine);
					}
					else
					{
						base.GameController.ExhaustCoroutine(coroutine);
					}

					// Ask which to place on bottom. 
					List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
					coroutine = this.GameController.SelectCardAndStoreResults(base.HeroTurnTakerController, SelectionType.MoveCardOnBottomOfDeck, revealedCards, storedResults, false, false, null, null, null, null, null, false, true, null);
					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(coroutine);
					}
					else
					{
						base.GameController.ExhaustCoroutine(coroutine);
					}

					// Selected card should be stored to move to the bottom. 
					Card selectedCard = this.GetSelectedCard(storedResults);

					// Move the other card to the bottom first.
					revealedCards.Remove(selectedCard);
					coroutine = this.GameController.MoveCard(base.HeroTurnTakerController, revealedCards.FirstOrDefault(), base.HeroTurnTaker.Deck, true);
					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(coroutine);
					}
					else
					{
						base.GameController.ExhaustCoroutine(coroutine);
					}

					// Move the selected card to the new, 'actual' bottom. 
					coroutine = this.GameController.MoveCard(base.HeroTurnTakerController, selectedCard, base.HeroTurnTaker.Deck, true);
					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(coroutine);
					}
					else
					{
						base.GameController.ExhaustCoroutine(coroutine);
					}
				}
				else
                {
					// No movement - destroy. 
                    coroutine = base.GameController.DestroyCard(base.HeroTurnTakerController, base.Card, false, null, null, null, null, null, null, null, null, base.GetCardSource(null));
					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(coroutine);
					}
					else
					{
						base.GameController.ExhaustCoroutine(coroutine);
					}
				}
			}
            else
            {
				// Not enough cards - automatically destroy. 
				coroutine = base.GameController.DestroyCard(base.HeroTurnTakerController, base.Card, false, null, null, null, null, null, null, null, null, base.GetCardSource(null));
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(coroutine);
				}
				else
				{
					base.GameController.ExhaustCoroutine(coroutine);
				}
			}

		}
	}
}
