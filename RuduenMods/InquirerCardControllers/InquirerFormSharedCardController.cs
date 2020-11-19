using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Inquirer
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
            this.AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, new Func<PhaseChangeAction, IEnumerator>(this.MoveOrDestroyResponse), new TriggerType[]
            {
                TriggerType.MoveCard,
                TriggerType.DestroySelf
            }, null, false);
        }

        // TODO: Change to Select Cards and Move if appropriate.
        // TODO: Validate if DecisionMaker should be this.DecisionMaker, or if this.HeroTurnTakerController works well.
        private IEnumerator MoveOrDestroyResponse(PhaseChangeAction phaseChange)
        {
            IEnumerator coroutine;
            // If enough cards exist
            if (this.HeroTurnTaker.Trash.Cards.Count<Card>() >= 2)
            {
                // TODO: Is there a better option for yesnoamountdecision? And can we add the card so it displays?)
                // Ask if we should move the top two cards of the trash to the bottom of the deck for things.
                YesNoAmountDecision yesNoDecision = new YesNoAmountDecision(this.GameController, this.HeroTurnTakerController, SelectionType.MoveCard, 2);
                coroutine = this.GameController.MakeDecisionAction(yesNoDecision);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                if (this.DidPlayerAnswerYes(yesNoDecision))
                {
                    // Fetch top two cards.
                    List<Card> revealedCards = new List<Card>();
                    coroutine = this.GameController.RevealCards(this.HeroTurnTakerController, HeroTurnTaker.Trash, 2, revealedCards, false, RevealedCardDisplay.None, null, this.GetCardSource(null));
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                    // Ask which to place on bottom.
                    List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
                    coroutine = this.GameController.SelectCardAndStoreResults(this.HeroTurnTakerController, SelectionType.MoveCardOnBottomOfDeck, revealedCards, storedResults, false, false, null, null, null, null, null, false, true, null);
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                    // Selected card should be stored to move to the bottom.
                    Card selectedCard = this.GetSelectedCard(storedResults);

                    // Move the other card to the bottom first.
                    revealedCards.Remove(selectedCard);
                    coroutine = this.GameController.MoveCard(this.HeroTurnTakerController, revealedCards.FirstOrDefault(), this.HeroTurnTaker.Deck, true);
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                    // Move the selected card to the new, 'actual' bottom.
                    coroutine = this.GameController.MoveCard(this.HeroTurnTakerController, selectedCard, this.HeroTurnTaker.Deck, true);
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
                else
                {
                    // No movement - destroy.
                    coroutine = this.GameController.DestroyCard(this.HeroTurnTakerController, this.Card, false, null, null, null, null, null, null, null, null, this.GetCardSource(null));
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
            }
            else
            {
                // Not enough cards - automatically destroy.
                // TODO: Add message if appropriate.
                coroutine = this.GameController.DestroyCard(this.HeroTurnTakerController, this.Card, false, null, null, null, null, null, null, null, null, this.GetCardSource(null));
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}