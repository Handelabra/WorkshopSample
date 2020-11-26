using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Tachyon
{
    public class TachyonControlledPaceCharacterCardController : PromoDefaultCharacterCardController
    {
        public TachyonControlledPaceCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;

            if (this.HeroTurnTaker.Trash.Cards.Count() > 0)
            {
                // Play the top card of your trash.
                coroutine = this.GameController.PlayCard(this.DecisionMaker, this.HeroTurnTaker.Trash.TopCard, cardSource: this.GetCardSource());
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                if (this.HeroTurnTaker.Trash.Cards.Count() > 0)
                {
                    // Move the top card of your trash.
                    coroutine = this.GameController.MoveCard(this.DecisionMaker, this.HeroTurnTaker.Trash.TopCard, this.HeroTurnTaker.Deck, true, cardSource: this.GetCardSource());
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
                else
                {
                    coroutine = this.GameController.SendMessageAction("There are no cards in the trash, so the top card cannot be moved.", Priority.Medium, cardSource: this.GetCardSource());
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
            }
            else
            {
                coroutine = this.GameController.SendMessageAction("There are no cards in the trash, so the top card cannot be played or moved.", Priority.Medium, cardSource: this.GetCardSource());
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}
