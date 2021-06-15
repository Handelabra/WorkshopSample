using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
namespace Workshopping.SkyScraper
{
    public class CentristSkyScraperHugeCharacterCardController : HeroCharacterCardController
    {
        public CentristSkyScraperHugeCharacterCardController(Card card, TurnTakerController turnTakerController)
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
            // Sky-Scraper deals each target 1 sonic damage.
            var damageAmount = GetPowerNumeral(0, 1);
            return DealDamage(this.Card, c => true, damageAmount, DamageType.Sonic);
        }
    }
}
