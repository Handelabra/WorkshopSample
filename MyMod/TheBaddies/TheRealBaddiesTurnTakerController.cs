using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;

namespace Workshopping.TheBaddies
{
    public class TheRealBaddiesTurnTakerController : TurnTakerController
    {
        public TheRealBaddiesTurnTakerController(TurnTaker turnTaker, GameController gameController)
            : base(turnTaker, gameController)
        {
        }

        public override IEnumerator StartGame()
        {
            // Play a WrightWeigh
            Card card = this.TurnTaker.GetCardByIdentifier("WrightWeigh");
            var playCard = this.GameController.PlayCard(this, card, true, cardSource: new CardSource(this.CharacterCardController));

            // Shuffle the villain deck
            var shuffle = this.GameController.ShuffleLocation(this.TurnTaker.Deck, cardSource: new CardSource(this.CharacterCardController));

            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(playCard);
                yield return this.GameController.StartCoroutine(shuffle);
            }
            else
            {
                this.GameController.ExhaustCoroutine(playCard);
                this.GameController.ExhaustCoroutine(shuffle);
            }
        }
    }
}
