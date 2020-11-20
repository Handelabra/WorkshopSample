using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;

namespace RuduenWorkshop.Inquirer
{
    public class ImAMolePersonCardController : InquirerFormSharedCardController
    {
        public ImAMolePersonCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // Add trigger for increasing healing.
            this.AddTrigger<GainHPAction>((GainHPAction g) => g.HpGainer == this.CharacterCard,
                (GainHPAction g) => this.GameController.IncreaseHPGain(g, 1, this.GetCardSource(null)),
                new TriggerType[] { TriggerType.IncreaseHPGain, TriggerType.ModifyHPGain },
                TriggerTiming.Before, null, false, true, null, false, null, null, false, false);

            // Add trigger for end of turn damage.
            this.AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, new Func<PhaseChangeAction, IEnumerator>(this.DealDamageResponse), TriggerType.DealDamage, null, false);
        }

        public IEnumerator DealDamageResponse(PhaseChangeAction phaseChange)
        {
            IEnumerator coroutine;
            // Deal Damage.
            coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), 1, DamageType.Melee, 1, false, 1, cardSource: this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}