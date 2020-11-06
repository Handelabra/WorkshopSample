using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Workshopping.BreachMage
{
    public class StaffCharmCardController : BreachMageSpellSharedCardController
    {
        public StaffCharmCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
            // Add damage boost if the direct source of the damage trigger was this card..
            Func<DealDamageAction, bool> criteria = delegate (DealDamageAction dd)
            {
                // Increase damage if the direct trigger of the damage was this card.
                return (from acs in dd.CardSource.AssociatedCardSources
                        where acs.Card != null
                        select acs.Card).Any((Card c) => c == this.Card);
            };
            base.AddIncreaseDamageTrigger(criteria, 2, null, null, false);
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;
            List<ActivateAbilityDecision> storedResults = new List<ActivateAbilityDecision>();

            // Use a Cast.
            coroutine = base.GameController.SelectAndActivateAbility(base.HeroTurnTakerController, "cast", null, storedResults, false, base.GetCardSource(null));
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

            if (storedResults.Count > 0)
            {
                // Destroy the cast card.
                coroutine = base.GameController.DestroyCard(base.HeroTurnTakerController, storedResults.FirstOrDefault().SelectedCard);
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}