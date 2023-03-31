using System;
using System.Collections;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Workshopping.BaronBlade
{
    public class BaronJeremyCharacterCardController : VillainCharacterCardController
    {
        public BaronJeremyCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override void AddSideTriggers()
        {
            if (!this.Card.IsFlipped)
            {
                // At the start of the Villain turn...
                this.SideTriggers.Add(AddStartOfTurnTrigger(tt => tt == this.TurnTaker, StartVillainTurnResponse, TriggerType.DealDamage));

                // At the end of the Villain Turn...
                this.SideTriggers.Add(AddEndOfTurnTrigger(tt => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[] { TriggerType.FlipCard }));
            }
            else
            {
                
            }

            // Both sides defeat trigger
            AddDefeatedIfDestroyedTriggers();
        }

        private IEnumerator StartVillainTurnResponse(PhaseChangeAction phaseChange)
        {
            // ... Baron Blade deals each hero Target H Fire Damage.
            var dealDamage = DealDamage(this.Card, c => c.IsHero, H, DamageType.Fire);

            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(dealDamage);
            }
            else
            {
                this.GameController.ExhaustCoroutine(dealDamage);

            }
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction phaseChange)
        {
            // ... if there are no Minions in play, flip {BaronBlade}'s villain character card.
            var minionCount = FindCardsWhere(c => IsMinion(c) && c.IsInPlay).Count();
            if (minionCount == 0)
            {
                IEnumerator flipcardE = this.GameController.FlipCard(this, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(flipcardE);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(flipcardE);

                }
            }
        }
    }
}
