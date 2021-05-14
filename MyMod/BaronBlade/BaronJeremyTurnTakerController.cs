using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.BaronBlade
{
    public class BaronJeremyTurnTakerController:TurnTakerController
    {
        public BaronJeremyTurnTakerController(TurnTaker turnTaker, GameController gameController)
            : base(turnTaker, gameController)
        {
        }

        public override IEnumerator StartGame()
        {
            // Play all BBs
            var battalions = FindCardsWhere(c => c.Identifier == "BladeBattalion");
            foreach (var bb in battalions)
            {
                var playCard = this.GameController.PlayCard(this, bb, true, cardSource: new CardSource(this.CharacterCardController));
                if (UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(playCard);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(playCard);
                }
            }

            // Shuffle the villain deck
            var shuffle = this.GameController.ShuffleLocation(this.TurnTaker.Deck, cardSource: new CardSource(this.CharacterCardController));

            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(shuffle);
            }
            else
            {
                this.GameController.ExhaustCoroutine(shuffle);
            }
        }
    }
}
