using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.RuduenFanMods.Inquirer
{
    // TODO: TEST!
    public class CardControllerImAVictorian : CardControllerFormShared
    {
        public CardControllerImAVictorian(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
            // Add trigger for discard-to-discard.
            base.AddTrigger<DiscardCardAction>((DiscardCardAction d) => d.WasCardDiscarded && d.Origin.IsHand && d.Origin.OwnerTurnTaker == base.TurnTaker, new Func<DiscardCardAction, IEnumerator>(this.DiscardResponse), TriggerType.IncreaseDamage, TriggerTiming.After, ActionDescription.Unspecified, false, true, null, false, null, null, false, false);

            // Add trigger for healing.
            base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction p) => base.GameController.GainHP(this.CharacterCard, 1),TriggerType.GainHP);
        }
        private IEnumerator DiscardResponse(DiscardCardAction discardCard)
        {
            List<MoveCardAction> storedResults = new List<MoveCardAction>();
            IEnumerator coroutine = base.GameController.DiscardTopCard(base.HeroTurnTaker.Deck, storedResults);
            yield return base.RunCoroutine(coroutine);
        }
    }
}
