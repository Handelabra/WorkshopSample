using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.MigrantCoder
{
    public class PunchingBagCardController : CardController
    {
        public PunchingBagCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        private int _customDecisionIndex;

        public override IEnumerator Play()
        {
            IEnumerator e;

            _customDecisionIndex = 0;
            var storedResults = new List<YesNoCardDecision>();
            e = this.GameController.MakeYesNoCardDecision(this.DecisionMaker, SelectionType.Custom, this.Card, storedResults:storedResults, cardSource:GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(e);
            }
            else
            {
                this.GameController.ExhaustCoroutine(e);
            }

            if (DidPlayerAnswerYes(storedResults))
            {
                var damageResults = new List<DealDamageAction>();
                e = DealDamage(this.CharacterCard, this.CharacterCard, 1, DamageType.Melee, storedResults: damageResults);
                if (UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(e);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(e);
                }

                if (DidDealDamage(damageResults))
                {
                    // Play two cards.
                    e = SelectAndPlayCardsFromHand(this.DecisionMaker, 2);
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
            else
            {
                // Ask another decision of the same type, because maybe they want cake.
                _customDecisionIndex = 1;
                storedResults = new List<YesNoCardDecision>();
                e = this.GameController.MakeYesNoCardDecision(this.DecisionMaker, SelectionType.Custom, this.Card, storedResults: storedResults, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(e);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(e);
                }

                string msg = DidPlayerAnswerYes(storedResults) ? "Good call, it's delicious and moist!" : "No cake for you!";
                var message = this.GameController.SendMessageAction(msg, Priority.Medium, GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(message);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(message);
                }
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (decision is YesNoCardDecision yesNoCard)
            {
                if (_customDecisionIndex == 0)
                {
                    return new CustomDecisionText("Do you want to think happy thoughts about {0}?", "Should they think happy thoughts?", "Vote for thinking happy thoughts", "happy thinking");
                }
                else
                {
                    return new CustomDecisionText("Can I get you some cake?", "Should they get cake?", "Vote for getting cake", "get cake", false);
                }
            }

            return null;
        }
    }
}
