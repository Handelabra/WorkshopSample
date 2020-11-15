using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// TODO: TEST!

namespace Workshopping.Inquirer
{
    public class InquirerLiesOnLiesCharacterCardController : HeroCharacterCardController
    {
        public string str;

        public InquirerLiesOnLiesCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            // TODO: Implement Incapacitated Abilities.
            switch (index)
            {
                case 0:
                    {
                        var message = this.GameController.SendMessageAction("This is the first thing that does nothing.", Priority.Medium, GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return this.GameController.StartCoroutine(message);
                        }
                        else
                        {
                            this.GameController.ExhaustCoroutine(message);
                        }
                        break;
                    }
                case 1:
                    {
                        var message = this.GameController.SendMessageAction("This is the second thing that does nothing.", Priority.Medium, GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return this.GameController.StartCoroutine(message);
                        }
                        else
                        {
                            this.GameController.ExhaustCoroutine(message);
                        }
                        break;
                    }
                case 2:
                    {
                        var message = this.GameController.SendMessageAction("Tricked you! Also does nothing.", Priority.Medium, GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return this.GameController.StartCoroutine(message);
                        }
                        else
                        {
                            this.GameController.ExhaustCoroutine(message);
                        }
                        break;
                    }
            }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;
            List<SelectCardDecision> storedResults = new List<SelectCardDecision>();

            // Select a distortion to move to the top of its deck.
            coroutine = this.GameController.SelectCardAndStoreResults(this.HeroTurnTakerController, SelectionType.MoveCardOnDeck, new LinqCardCriteria((Card c) => c.IsInPlay && !c.IsOneShot && c.IsDistortion && this.GameController.IsCardVisibleToCardSource(c, this.GetCardSource(null)), "distortion cards in play", false, false, null, null, false), storedResults, false, false, null, true, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Move based on decision. 
            SelectCardDecision selectCardDecision = storedResults.FirstOrDefault();
            if (selectCardDecision != null && selectCardDecision.SelectedCard != null)
            {
                Card card = selectCardDecision.SelectedCard;
                if (selectCardDecision.Choices.Count<Card>() == 1)
                {
                    coroutine = this.GameController.SendMessageAction(card.Title + " is the only distortion card in play.", Priority.Low, this.GetCardSource(null), selectCardDecision.Choices, true);
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
                coroutine = this.GameController.MoveCard(this.TurnTakerController, card, card.NativeDeck, false, false, true, null, false, null, null, null, false, false, null, false, false, false, false, this.GetCardSource(null));
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                card = null;
            }

            // Play the top card of your deck.
            if (this.TurnTaker.IsHero)
            {
                coroutine = this.GameController.PlayTopCard(this.DecisionMaker, this.TurnTakerController, false, 1, false, null, null, null, false, null, false, false, false, null, null, this.GetCardSource(null));
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
            else
            {
                coroutine = this.GameController.SendMessageAction(this.Card.Title + " has no deck to play cards from.", Priority.Medium, this.GetCardSource(null), null, true);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}