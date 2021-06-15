using System;
using System.Collections;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Workshopping.TheBaddies
{
    public class TheRealBaddiesCharacterCardController : VillainCharacterCardController
    {
        public TheRealBaddiesCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override bool CanBeDestroyed
        {
            get
            {
                // Can only be destroyed if we are flipped.
                return this.CharacterCard.IsFlipped;
            }
        }

        public override void AddSideTriggers()
        {
            if (!this.Card.IsFlipped)
            {
                // At the start of the villain turn...
                this.SideTriggers.Add(AddEndOfTurnTrigger(tt => tt == this.TurnTaker, StartVillainTurnResponse, TriggerType.DealDamage));

                AddDefeatedIfMovedOutOfGameTriggers();
            }
            else
            {

                // If HP is <= 0, we are defeated.
                AddDefeatedIfDestroyedTriggers();
            }
        }

        public override IEnumerator DestroyAttempted(DestroyCardAction destroyCard)
        {
            if (!this.Card.IsFlipped)
            {
                // Run a FlipAction when we would be destroyed.
                var e = this.GameController.FlipCard(this, actionSource: destroyCard.ActionSource, cardSource: GetCardSource());
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

        private IEnumerator StartVillainTurnResponse(PhaseChangeAction phaseChange)
        {
            // {TheBaddies} deals the Hero Target with the highest HP {H} Toxic Damage.
            return DealDamageToHighestHP(this.CharacterCard, 1, c => c.IsHero, c => H, DamageType.Toxic);
        }

        public override IEnumerator AfterFlipCardImmediateResponse()
        {
            var e = base.AfterFlipCardImmediateResponse();
            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(e);
            }
            else
            {
                this.GameController.ExhaustCoroutine(e);

            }

            // When flipped to this side, restore to 20 HP.
            var restoreHP = this.GameController.ChangeMaximumHP(this.Card, 20, true, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(restoreHP);
            }
            else
            {
                this.GameController.ExhaustCoroutine(restoreHP);

            }
        }

        protected override EndingResult GetEndingResultType()
        {
            // If on his back side, it's a normal victory.
            // If on his front side, a premature victory.

            if (this.Card.IsFlipped)
            {
                return EndingResult.VillainDestroyedVictory;
            }
            else
            {
                return EndingResult.PrematureVictory;
            }
        }
    }
}
