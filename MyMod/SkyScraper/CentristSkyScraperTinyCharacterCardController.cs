using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Workshopping.SkyScraper
{
    public class CentristSkyScraperTinyCharacterCardController : HeroCharacterCardController
    {
        public CentristSkyScraperTinyCharacterCardController(Card card, TurnTakerController turnTakerController)
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
            // Reveal the top card of a deck. Put it into play or into the trash.
            var storedResults = new List<SelectLocationDecision>();
            var e = this.GameController.SelectADeck(this.DecisionMaker, SelectionType.RevealTopCardOfDeck, l => true, storedResults, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(e);
            }
            else
            {
                this.GameController.ExhaustCoroutine(e);
            }

            if (DidSelectDeck(storedResults))
            {
                var deck = storedResults.First().SelectedLocation.Location;
                if (deck != null)
                {
                    e = RevealCard_PlayItOrDiscardIt(this.TurnTakerController, deck, true, responsibleTurnTaker: this.TurnTaker, isDiscard: false);
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
