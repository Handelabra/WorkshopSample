using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.Inquirer
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
            base.AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.Card == base.CharacterCard, 1, null, null, false);

            // Add trigger for discard-for-power.
            base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, new Func<PhaseChangeAction, IEnumerator>(this.DiscardToUsePowerResponse), new TriggerType[]
            {
                TriggerType.DiscardCard,
                TriggerType.UsePower
            }, null, false);
        }
        private IEnumerator DiscardToUsePowerResponse(PhaseChangeAction p)
        {
            List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
            IEnumerator coroutine;

            coroutine = base.GameController.SelectAndDiscardCard(base.HeroTurnTakerController, true, null, storedResults, SelectionType.DiscardCard);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            if (base.DidDiscardCards(storedResults, null, false))
            {
                coroutine = base.GameController.SelectAndUsePower(base.HeroTurnTakerController);
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
