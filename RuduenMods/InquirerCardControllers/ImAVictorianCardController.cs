using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RuduenWorkshop.Inquirer
{
    // TODO: TEST!
    public class ImAVictorianCardController : InquirerFormSharedCardController
    {
        public ImAVictorianCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // Add trigger for discard-to-discard.
            this.AddTrigger<DiscardCardAction>((DiscardCardAction d) => d.WasCardDiscarded && d.Origin.IsHand && d.Origin.OwnerTurnTaker == this.TurnTaker, new Func<DiscardCardAction, IEnumerator>(this.DiscardResponse), TriggerType.DiscardCard, TriggerTiming.After, ActionDescription.Unspecified);

            // Add trigger for healing.
            this.AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, (PhaseChangeAction p) => this.GameController.GainHP(this.CharacterCard, 1), TriggerType.GainHP);
        }

        private IEnumerator DiscardResponse(DiscardCardAction discardCard)
        {
            List<MoveCardAction> storedResults = new List<MoveCardAction>();
            IEnumerator coroutine = this.GameController.DiscardTopCard(this.HeroTurnTaker.Deck, storedResults);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}