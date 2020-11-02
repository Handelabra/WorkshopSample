using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.RuduenFanMods.BreachMage
{
    public class BreachMageCharacterCardController : HeroCharacterCardController
    {
        public string str;

        public BreachMageCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            switch (index)
            {
                case 0:
                    {
                        var message = this.GameController.SendMessageAction("This is the first thing that does nothing.", Priority.Medium, GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return this.GameController.StartCoroutine(message);
                        }
                        else
                        {
                            this.GameController.ExhaustCoroutine(message);
                        }
                        break;
                    }
                case 1:
                    {
                        var message = this.GameController.SendMessageAction("This is the second thing that does nothing.", Priority.Medium, GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return this.GameController.StartCoroutine(message);
                        }
                        else
                        {
                            this.GameController.ExhaustCoroutine(message);
                        }
                        break;
                    }
                case 2:
                    {
                        var message = this.GameController.SendMessageAction("Tricked you! Also does nothing.", Priority.Medium, GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return this.GameController.StartCoroutine(message);
                        }
                        else
                        {
                            this.GameController.ExhaustCoroutine(message);
                        }
                        break;
                    }
            }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator e;
            //// Draw 3 cards!
            //IEnumerator e = DrawCards(this.HeroTurnTakerController, 3);

            //if (UseUnityCoroutines)
            //{
            //    yield return this.GameController.StartCoroutine(e);
            //}
            //else
            //{
            //    this.GameController.ExhaustCoroutine(e);
            //}


            //StatusEffect test = new StatusEffect()
            //e = this.GameController.SelectAndDestroyCard(this.DecisionMaker, new CardCriteria())

            // Deal 1 target 2 psychic damage
            e = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), 2, DamageType.Fire, 1, false, 1, cardSource:GetCardSource());

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