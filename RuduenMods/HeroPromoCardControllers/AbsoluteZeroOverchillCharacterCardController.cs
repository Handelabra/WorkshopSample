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
            int powerNumeral = this.GetPowerNumeral(0, 1); // Amount of damage.

            List<PlayCardAction> storedResults = new List<PlayCardAction>();

            IEnumerator coroutine;

            // Deal self damage.
            coroutine = this.GameController.DealDamageToTarget(new DamageSource(this.GameController, this.CharacterCard), this.CharacterCard, powerNumeral, DamageType.Cold, cardSource: this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Selection: Play or destroy one of your equipments. 

            List<Function> list = new List<Function>
                {
                    new Function(this.DecisionMaker, "Play an Equipment card", SelectionType.PlayCard,
                        () => this.GameController.SelectAndPlayCardFromHand(this.HeroTurnTakerController, false,
                            cardCriteria: new LinqCardCriteria((Card c) => this.IsEquipment(c), "equipment"),
                            cardSource: this.GetCardSource()),
                        this.CanPlayCards(this.HeroTurnTakerController) && this.HeroTurnTaker.Hand.Cards.Any((Card c)=>this.IsEquipment(c) && !this.GameController.IsLimitedAndInPlay(c)),
                        this.TurnTaker.Name + " cannot destroy any of their Equipment cards, so they must play an Equipment card.", null),

                    new Function(this.DecisionMaker, "Destroy one of your Equipment cards", SelectionType.DestroyCard,
                        () => this.GameController.SelectAndDestroyCard(this.DecisionMaker,
                            cardCriteria: new LinqCardCriteria((Card c) => this.IsEquipment(c), "equipment"),
                            false,
                            cardSource: this.GetCardSource()),
                        this.HeroTurnTaker.PlayArea.Cards.Any((Card c) => this.IsEquipment(c)), this.TurnTaker.Name + " cannot play any Equipment cards, so they must destroy one of their Equipment cards instead.", null)
                };
            SelectFunctionDecision selectFunction = new SelectFunctionDecision(this.GameController, this.DecisionMaker, list, false, null, this.TurnTaker.Name + " cannot play any Equipment cards nor destroy any of their equipment cards, so" + this.Card.Title + " has no effect.", null, this.GetCardSource(null));

            coroutine = this.GameController.SelectAndPerformFunction(selectFunction, null, null);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Draw 2 cards.
            coroutine = this.DrawCards(this.HeroTurnTakerController, 2);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

    }
}