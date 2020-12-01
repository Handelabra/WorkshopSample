using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;

namespace RuduenWorkshop.ChronoRanger
{
    public class ChronoRangerHighNoonCharacterCardController : PromoDefaultCharacterCardController
    {
        public ChronoRangerHighNoonCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;

            // Draw.
            coroutine = this.DrawCard(this.HeroTurnTaker);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Separate effects for making irreducible, or else it only applies to self-inflicted.

            MakeDamageIrreducibleStatusEffect makeDamageIrreducibleA = new MakeDamageIrreducibleStatusEffect();
            makeDamageIrreducibleA.SourceCriteria.IsSpecificCard = this.CharacterCard;
            makeDamageIrreducibleA.UntilEndOfNextTurn(this.TurnTaker);
            coroutine = this.AddStatusEffect(makeDamageIrreducibleA, true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            MakeDamageIrreducibleStatusEffect makeDamageIrreducibleB = new MakeDamageIrreducibleStatusEffect();
            makeDamageIrreducibleB.TargetCriteria.IsSpecificCard = this.CharacterCard;
            makeDamageIrreducibleB.UntilEndOfNextTurn(this.TurnTaker);
            coroutine = this.AddStatusEffect(makeDamageIrreducibleB, true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}