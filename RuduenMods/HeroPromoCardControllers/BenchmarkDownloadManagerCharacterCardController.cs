using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Benchmark
{
    public class BenchmarkDownloadManagerCharacterCardController : PromoDefaultCharacterCardController
    {
        public BenchmarkDownloadManagerCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int powerNumeral = this.GetPowerNumeral(0, 1);

            List<PlayCardAction> storedResults = new List<PlayCardAction>();

            IEnumerator coroutine;

            // You may play a Software.
            coroutine = this.GameController.SelectAndPlayCardFromHand(this.HeroTurnTakerController, true, cardCriteria: new LinqCardCriteria((Card c) => c.IsSoftware, "software"), storedResults: storedResults, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // No successful card play means destroy.
            if (storedResults.Count > 0 && storedResults.FirstOrDefault().WasCardPlayed)
            {
                Card playedCard = storedResults.FirstOrDefault().CardToPlay;
                if (playedCard != null)
                {
                    MakeIndestructibleStatusEffect makeIndestructibleStatusEffect = new MakeIndestructibleStatusEffect();
                    makeIndestructibleStatusEffect.CardsToMakeIndestructible.IsSpecificCard = playedCard;
                    makeIndestructibleStatusEffect.UntilEndOfNextTurn(this.HeroTurnTaker);
                    coroutine = this.AddStatusEffect(makeIndestructibleStatusEffect, true);
                    if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
            }

            // Deal damage to two targets equal to number of software.
            int amount = this.FindCardsWhere((Card c) => c.IsInPlay && c.IsSoftware).Count();

            coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), amount, DamageType.Lightning, powerNumeral, false, powerNumeral, cardSource: this.GetCardSource());
            if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}