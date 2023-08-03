using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.CaptainThunder
{
    public class CaptainThunderstruckCharacterCardController : HeroCharacterCardController
    {
        public CaptainThunderstruckCharacterCardController(Card card, TurnTakerController turnTakerController)
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
            // Increase all lightning damage and sonic damage dealt by 2 until the end of your next turn.
            var amount = GetPowerNumeral(0, 2);

            IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(amount);
            effect.CardDestroyedExpiryCriteria.Card = this.Card;
            effect.DamageTypeCriteria.AddType(DamageType.Lightning);
            effect.DamageTypeCriteria.AddType(DamageType.Sonic);
            effect.UntilEndOfNextTurn(this.TurnTaker);

            return AddStatusEffect(effect);
        }
    }
}
