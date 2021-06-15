using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.MigrantCoder
{
    public class MigrantCoderCharacterCardController : HeroCharacterCardController
    {
        public string str;

        public MigrantCoderCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // Draw 1 card
            var numberOfCards = GetPowerNumeral(0, 1);
            IEnumerator e = DrawCards(this.HeroTurnTakerController, numberOfCards);

            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(e);
            }
            else
            {
                this.GameController.ExhaustCoroutine(e);
            }

            // Deal 1 target 2 psychic damage
            var numberOfTargets = GetPowerNumeral(1, 1);
            var damageAmount = GetPowerNumeral(2, 2);
            e = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), damageAmount, DamageType.Psychic, numberOfTargets, false, numberOfTargets, cardSource:GetCardSource());

            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(e);
            }
            else
            {
                this.GameController.ExhaustCoroutine(e);
            }
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
    }
}