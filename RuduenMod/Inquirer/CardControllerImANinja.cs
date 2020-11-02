using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.RuduenFanMods.Inquirer
{
    // TODO: TEST!
    public class CardControllerImANinja : CardControllerFormShared
    {
        public CardControllerImANinja(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
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
            yield return base.RunCoroutine(coroutine);
            if (base.DidDiscardCards(storedResults, null, false))
            {
                coroutine = base.GameController.SelectAndUsePower(base.HeroTurnTakerController);
                yield return base.RunCoroutine(coroutine);
            }

            yield break;
        }
    }
}
