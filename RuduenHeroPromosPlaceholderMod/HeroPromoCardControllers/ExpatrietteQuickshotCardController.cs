using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.HeroPromos
{
    public class ExpatrietteQuickshotCardController : CardController
    {
        public ExpatrietteQuickshotCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;
            // If deck is empty, indicate via message. 
            if (this.HeroTurnTaker.Deck.NumberOfCards == 0)
            {
                coroutine = this.GameController.SendMessageAction("The deck is empty - there are no cards to discard.", Priority.Medium, this.GetCardSource(null));
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
            else
            {
                List<MoveCardAction> storedResults = new List<MoveCardAction>();
                coroutine = this.GameController.DiscardTopCards(this.DecisionMaker, this.HeroTurnTaker.Deck, 1, storedResults);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                // If card is equipment, play it and use a power. 
                if (storedResults.Count > 0 && IsEquipment(storedResults.FirstOrDefault().CardToMove))
                {
                    coroutine = this.GameController.PlayCard(this.DecisionMaker, storedResults.FirstOrDefault().CardToMove);
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                    coroutine = this.GameController.SelectAndUsePower(this.DecisionMaker, false);
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
            }

        }
    }
}