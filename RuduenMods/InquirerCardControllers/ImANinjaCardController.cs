using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RuduenWorkshop.Inquirer
{
    // TODO: TEST!
    public class ImANinjaCardController : InquirerFormSharedCardController
    {
        public ImANinjaCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // Add trigger for increasing damage.
            this.AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.Card == this.CharacterCard, 1, null, null, false);

            // Add trigger for discard-for-power.
            this.AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, new Func<PhaseChangeAction, IEnumerator>(this.DiscardToUsePowerResponse), new TriggerType[]
            {
                TriggerType.DiscardCard,
                TriggerType.UsePower
            }, null, false);
        }

        private IEnumerator DiscardToUsePowerResponse(PhaseChangeAction p)
        {
            List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
            IEnumerator coroutine;

            coroutine = this.GameController.SelectAndDiscardCard(this.HeroTurnTakerController, true, null, storedResults, SelectionType.DiscardCard);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (this.DidDiscardCards(storedResults, null, false))
            {
                // Select and use a power.
                coroutine = this.GameController.SelectAndUsePower(this.DecisionMaker, false);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}