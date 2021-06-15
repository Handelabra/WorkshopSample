using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
namespace Workshopping.SkyScraper
{
    public class CentristSkyScraperNormalCharacterCardController : HeroCharacterCardController
    {
        public CentristSkyScraperNormalCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator e = null;

            switch (index)
            {
                case 0:
                    // One player may play a card now.
                    e = SelectHeroToPlayCard(this.DecisionMaker);
                    break;
                case 1:
                    // One hero may use a power now.
                    e = this.GameController.SelectHeroToUsePower(this.DecisionMaker, cardSource: GetCardSource());
                    break;
                case 2:
                    // One player may draw a card now
                    e = this.GameController.SelectHeroToDrawCard(this.DecisionMaker, cardSource: GetCardSource());
                    break;
            }

            return e;
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // Draw 3 cards!
            var numberOfCards = GetPowerNumeral(0, 3);
            IEnumerator e = DrawCards(this.HeroTurnTakerController, numberOfCards);

            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(e);
            }
            else
            {
                this.GameController.ExhaustCoroutine(e);
            }

            // Switch to huge!
            if (this.Card.IsInPlayAndHasGameText && !this.TurnTaker.IsIncapacitatedOrOutOfGame && this.Card.SharedIdentifier != null)
            {
                var otherCard = this.TurnTaker.FindCard("SkyScraperHugeCharacter");

                if (otherCard != null)
                {
                    e = this.GameController.SwitchCards(this.Card, otherCard, cardSource: GetCardSource());
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
    }
}
