using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Workshopping.Inquirer
{
    // Token: 0x0200054D RID: 1357
    public class InquirerDistortionSharedCardController : CardController
    {
        protected LinqCardCriteria NextToCriteria { get; set; }
        public InquirerDistortionSharedCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            this.NextToCriteria = new LinqCardCriteria((Card c) => c.IsTarget, "targets", false, false, null, null, false);
        }

        public override void AddTriggers()
        {
            // Selfdestruct at start of turn. 
            base.AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, new Func<PhaseChangeAction, IEnumerator>(base.DestroyThisCardResponse), TriggerType.DestroySelf, null, false);

            // Played next to cards - adjust location.
            base.AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToTheirPlayAreaTrigger(true, true);

            // Common On Destroy trigger.
            base.AddWhenDestroyedTrigger(new Func<DestroyCardAction, IEnumerator>(this.OnDestroyResponse), TriggerType.DealDamage);
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // Deal Damage.
            IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(base.GameController, base.Card), 3, DamageType.Melee, 1, false, 1, false, false, false, null, null, null, null, null, false, null, null, false, null, base.GetCardSource(null));
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
        }


        // Override on individual instances.
        protected virtual IEnumerator OnDestroyResponse(DestroyCardAction dc)
        {
            yield break;
        }

    }
}
