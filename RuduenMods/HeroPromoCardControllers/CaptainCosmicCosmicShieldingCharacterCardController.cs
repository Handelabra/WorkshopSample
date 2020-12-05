using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;

namespace RuduenWorkshop.CaptainCosmic
{
    public class CaptainCosmicCosmicShieldingCharacterCardController : PromoDefaultCharacterCardController
    {
        public CaptainCosmicCosmicShieldingCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int powerNumeral = this.GetPowerNumeral(0, 2);
            List<SelectCardDecision> storedResults = new List<SelectCardDecision>();

            IEnumerator coroutine;

            // Draw a card.
            coroutine = this.DrawCards(this.DecisionMaker, 1);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Select a construct target.
            coroutine = this.GameController.SelectCardAndStoreResults(this.DecisionMaker, SelectionType.SelectTargetFriendly,
                new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.IsConstruct && c.IsTarget, "construct target"),
                storedResults, false, cardSource: this.GetCardSource());
            if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            Card selectedCard = this.GetSelectedCard(storedResults);
            if (selectedCard != null)
            {
                // Make Indestructible.
                MakeIndestructibleStatusEffect mi = new MakeIndestructibleStatusEffect();
                mi.CardsToMakeIndestructible.IsSpecificCard = selectedCard;
                mi.UntilStartOfNextTurn(this.HeroTurnTaker);
                coroutine = this.GameController.AddStatusEffect(mi, true, this.GetCardSource());
                if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                // Construct Damages Self.
                coroutine = this.GameController.DealDamageToTarget(new DamageSource(this.GameController, selectedCard), selectedCard, powerNumeral, DamageType.Energy, cardSource: this.GetCardSource(null));
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}