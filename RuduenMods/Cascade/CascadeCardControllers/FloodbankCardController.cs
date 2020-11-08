using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Workshopping.Cascade;

namespace Workshopping.Cascade
{
    public class FloodbankCardController : CascadeRiverSharedCardController
    {
        public FloodbankCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            // Damage.
            IEnumerator coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.Card), 2, DamageType.Cold, 1, false, 1, false, false, false, null, null, null, null, null, false, null, null, false, null, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Heal.
            coroutine = this.GameController.GainHP(this.CharacterCard, 2, null, null, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}