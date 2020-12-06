using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.AbsoluteZero
{
    public class AbsoluteZeroOverchillCharacterCardController : PromoDefaultCharacterCardController
    {
        public AbsoluteZeroOverchillCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            List<int> powerNumerals = new List<int>(){
                this.GetPowerNumeral(0, 1), // Amount of damage.
                this.GetPowerNumeral(1, 2) // Cards Drawn.
            };

            List<PlayCardAction> storedResults = new List<PlayCardAction>();

            IEnumerator coroutine;

            // Deal self damage.
            coroutine = this.GameController.DealDamageToTarget(new DamageSource(this.GameController, this.CharacterCard), this.CharacterCard, powerNumerals[0], DamageType.Cold, cardSource: this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // You may play equipment card.
            coroutine = this.GameController.SelectAndPlayCardFromHand(this.HeroTurnTakerController, true, cardCriteria: new LinqCardCriteria((Card c) => this.IsEquipment(c), "equipment"), storedResults: storedResults, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // No successful card play means destroy.
            if (storedResults.Count == 0 || !storedResults.FirstOrDefault().IsSuccessful)
            {
                coroutine = this.GameController.SelectAndDestroyCard(this.DecisionMaker, cardCriteria: new LinqCardCriteria((Card c) => this.IsEquipment(c), "equipment"), false, cardSource: this.GetCardSource());
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }

            // Draw 2 cards.
            coroutine = this.DrawCards(this.DecisionMaker, powerNumerals[1]);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}