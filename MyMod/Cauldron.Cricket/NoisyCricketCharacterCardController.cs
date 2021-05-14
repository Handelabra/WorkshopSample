using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.Cricket
{
    public class NoisyCricketCharacterCardController : HeroCharacterCardController
    {
        public NoisyCricketCharacterCardController(Card card, TurnTakerController turnTakerController)
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
            var targets = GetPowerNumeral(0, 1);
            var amount = GetPowerNumeral(1, 10);
            var storedResults = new List<DealDamageAction>();
            var source = new DamageSource(this.GameController, this.CharacterCard);
            var e = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, source, amount, DamageType.Energy, targets, false, targets, true, storedResultsDamage:storedResults, cardSource: GetCardSource());

            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(e);
            }
            else
            {
                this.GameController.ExhaustCoroutine(e);
            }

            if (DidDealDamage(storedResults))
            {
                // Destroy self!
                e = this.GameController.DestroyCard(this.HeroTurnTakerController, this.CharacterCard);

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
