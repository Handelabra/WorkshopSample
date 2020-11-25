using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Expatriette
{
    public class ExpatrietteQuickShotCharacterCardController : PromoDefaultCharacterCardController
    {
        public ExpatrietteQuickShotCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;

            ReduceDamageStatusEffect reduceDamageStatusEffect = new ReduceDamageStatusEffect(this.GetPowerNumeral(0, 1));
            reduceDamageStatusEffect.SourceCriteria.IsSpecificCard = this.CharacterCard;
            reduceDamageStatusEffect.UntilThisTurnIsOver(this.Game);
            coroutine = base.AddStatusEffect(reduceDamageStatusEffect, true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (this.HeroTurnTaker.Deck.IsEmpty)
            {
                coroutine = this.GameController.SendMessageAction(this.TurnTaker.Name + " cannot discard a card nor discard the top card of their deck, so" + this.Card.Title + " has no effect.", Priority.Medium, this.GetCardSource());
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
            else
            {
                coroutine = this.GameController.PlayTopCard(this.DecisionMaker, this.TurnTakerController, numberOfCards: 1, cardSource: this.GetCardSource());
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }

            coroutine = this.GameController.SelectAndUsePower(this.DecisionMaker, false, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }


    }
}