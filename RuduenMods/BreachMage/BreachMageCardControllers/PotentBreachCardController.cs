using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Workshopping.BreachMage
{
    public class PotentBreachCardController : CardController
    {
        public PotentBreachCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
            this.AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, new Func<PhaseChangeAction, IEnumerator>(this.CastResponse), TriggerType.DestroyCard, null, false);

            // Add damage boost if the direct source of the damage trigger was this card.
            bool criteria(DealDamageAction dd)
            {
                // Increase damage if the direct trigger of the damage was this card.
                return (from acs in dd.CardSource.AssociatedCardSources
                        where acs.Card != null
                        select acs.Card).Any((Card c) => c == this.Card);
            }
            this.AddIncreaseDamageTrigger(criteria, 1, null, null, false);
        }

        protected IEnumerator CastResponse(PhaseChangeAction phaseChange)
        {
            IEnumerator coroutine;
            List<ActivateAbilityDecision> storedResults = new List<ActivateAbilityDecision>();

            // Use a Cast.
            coroutine = this.GameController.SelectAndActivateAbility(this.HeroTurnTakerController, "cast", null, storedResults, true, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (storedResults.Count > 0)
            {
                // Destroy the cast card.
                coroutine = this.GameController.DestroyCard(this.HeroTurnTakerController, storedResults.FirstOrDefault().SelectedCard);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}