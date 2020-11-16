using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;

namespace Workshopping.Cascade
{
    public class RisingWatersCardController : CascadeRiverSharedCardController
    {
        public RisingWatersCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
            this.AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, new Func<PhaseChangeAction, IEnumerator>(this.DestroyOngoingResponse), TriggerType.DestroyCard, null, false);
            this.AddIncreaseDamageTrigger((DealDamageAction dealDamage) => dealDamage.DamageSource.IsHero, 1, null, null, false);
        }

        protected IEnumerator DestroyOngoingResponse(PhaseChangeAction phaseChange)
        {
            // Destroy an ongoing.
            IEnumerator coroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsOngoing, "ongoing", true, false, null, null, false), false, null, null, base.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}