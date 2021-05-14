using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.DevStream
{
    public class ModderCardController: CardController
    {
        public ModderCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
            // At the start of the environment turn... 
            AddStartOfTurnTrigger(tt => tt == this.TurnTaker, PlayTopCardOfEachDeckInTurnOrderResponse, TriggerType.PlayCard);

        }

        private IEnumerator PlayTopCardOfEachDeckInTurnOrderResponse(PhaseChangeAction phaseChange)
        {
            // ...put the top card of the villain deck into play.
            var e1 = this.GameController.SendMessageAction("Slamara puts the top card of the villain deck, and then the top card of each hero deck into play in turn order.", Priority.Low, cardSource: GetCardSource());
            var e2 = PlayTopCardOfEachDeckInTurnOrder(ttc => ttc.IsVillain && !ttc.TurnTaker.IsScion, l => l.IsVillain, this.TurnTaker, true, false);
            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(e1);
                yield return this.GameController.StartCoroutine(e2);
            }
            else
            {
                this.GameController.ExhaustCoroutine(e1);
                this.GameController.ExhaustCoroutine(e2);

            }

            // Then, put the top card of each hero deck into play in turn order.
            // If a limited card already in play would be put into play, discard that card instead.
            var e = PlayTopCardOfEachDeckInTurnOrder(ttc => ttc.IsHero, l => l.IsHero, this.TurnTaker, true, false);
            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(e);
            }
            else
            {
                this.GameController.ExhaustCoroutine(e);

            }
        }
    }
}
