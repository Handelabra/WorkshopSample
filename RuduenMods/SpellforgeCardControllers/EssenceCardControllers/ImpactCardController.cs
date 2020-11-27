using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.Spellforge
{
    public class ImpactCardController : SpellforgeSharedEssenceCardController
    {
        public ImpactCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        protected override IEnumerator PerformModifiedAction(CardSource cardSource)
        {
            // Deal all 1 melee.
            IEnumerator coroutine = this.DealDamage(this.CharacterCard, (Card c) => c.IsTarget, 2, DamageType.Melee);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}