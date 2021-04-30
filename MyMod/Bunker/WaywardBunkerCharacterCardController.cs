using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.Bunker
{
    public class WaywardBunkerCharacterCardController : HeroCharacterCardController
    {
        public WaywardBunkerCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            switch (index)
            {
                case 0:
                    // One player may play a card now.
                    var e0 = SelectHeroToPlayCard(this.DecisionMaker);
                    if (UseUnityCoroutines)
                    {
                        yield return this.GameController.StartCoroutine(e0);
                    }
                    else
                    {
                        this.GameController.ExhaustCoroutine(e0);

                    }
                    break;
                case 1:
                    // One hero may use a power now.
                    var e1 = this.GameController.SelectHeroToUsePower(this.DecisionMaker, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return this.GameController.StartCoroutine(e1);
                    }
                    else
                    {
                        this.GameController.ExhaustCoroutine(e1);

                    }
                    break;
                case 2:
                    // One player may draw a card now
                    var e2 = this.GameController.SelectHeroToDrawCard(this.DecisionMaker, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return this.GameController.StartCoroutine(e2);
                    }
                    else
                    {
                        this.GameController.ExhaustCoroutine(e2);

                    }
                    break;
            }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // Draw 2 cards!
            var numberOfCards = GetPowerNumeral(0, 2);
            IEnumerator e = DrawCards(this.HeroTurnTakerController, numberOfCards);

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
