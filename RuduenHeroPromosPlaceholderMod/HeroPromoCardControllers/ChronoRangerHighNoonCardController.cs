using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenPromosWorkshop.HeroPromos
{
    public class ChronoRangerHighNoonCardController : HeroPromosSharedToHeroCardController
    {
        public ChronoRangerHighNoonCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            ToHeroIdentifier = "ChronoRangerCharacter";
            PowerDescription = "Until the end of your next turn, damage dealt to and by this hero is irreducible. Draw a card or play a card.";
        }

        public override IEnumerator PowerCoroutine(CardController cardController)
        {
            IEnumerator coroutine;

            // Separate effects for making irreducible, or else it only applies to self-inflicted.

            MakeDamageIrreducibleStatusEffect makeDamageIrreducibleA = new MakeDamageIrreducibleStatusEffect();
            makeDamageIrreducibleA.SourceCriteria.IsSpecificCard = cardController.CharacterCard;
            makeDamageIrreducibleA.UntilEndOfNextTurn(cardController.TurnTaker);
            coroutine = this.AddStatusEffect(makeDamageIrreducibleA, true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            MakeDamageIrreducibleStatusEffect makeDamageIrreducibleB = new MakeDamageIrreducibleStatusEffect();
            makeDamageIrreducibleB.TargetCriteria.IsSpecificCard = cardController.CharacterCard;
            makeDamageIrreducibleB.UntilEndOfNextTurn(cardController.TurnTaker);
            coroutine = this.AddStatusEffect(makeDamageIrreducibleB, true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Draw or play. 
            coroutine = this.DrawACardOrPlayACard(cardController.DecisionMaker, false);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

    }
}