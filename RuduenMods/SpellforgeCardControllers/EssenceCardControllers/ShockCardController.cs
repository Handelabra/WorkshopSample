using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.Spellforge
{
    public class ShockCardController : SpellforgeSharedEssenceCardController
    {
        public ShockCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        protected override IEnumerator PerformModifiedAction(CardSource cardSource)
        {
            // Deal up to 3 targets 3 lightning.
            IEnumerator coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), 3, DamageType.Lightning, 3, false, 3, cardSource: cardSource);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}