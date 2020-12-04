using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.TheHarpy
{
    public class TheHarpyExtremeCallingCharacterCardController : HeroCharacterCardController
    {
        public TheHarpyExtremeCallingCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            TokenPool arcanaPool = base.Card.FindTokenPool(TokenPool.ArcanaControlPool);
            TokenPool avianPool = base.Card.FindTokenPool(TokenPool.AvianControlPool);
            this.SpecialStringMaker.ShowSpecialString(() => string.Format("The Harpy has {0} {1} and {2} {3} control tokens.",
                new object[] { arcanaPool.CurrentValue, "{arcana}", avianPool.CurrentValue, "{avian}" }),
                null, null);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;
            int powerNumeral = GetPowerNumeral(0, 1);

            coroutine = this.FlipAllControlTokenAndDamage();
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Draw 1 card.
            coroutine = this.DrawCards(this.DecisionMaker, powerNumeral);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

        }

        // Token: 0x060015E4 RID: 5604 RVA: 0x0003C07C File Offset: 0x0003A27C
        public IEnumerator FlipAllControlTokenAndDamage()
        {
            IEnumerator coroutine;

            List<SelectWordDecision> storedResultsWord = new List<SelectWordDecision>();
            List<FlipTokensAction> storedResultsToken = new List<FlipTokensAction>();
            TokenPool avianPool = this.Card.FindTokenPool(TokenPool.AvianControlPool);
            TokenPool arcanaPool = this.Card.FindTokenPool(TokenPool.ArcanaControlPool);
            int tokensFlipped = 0;

            if (avianPool == null || arcanaPool == null || !this.TurnTaker.IsHero)
            {
                TurnTaker turnTaker = this.FindTurnTakersWhere((TurnTaker tt) => tt.Identifier == "TheHarpy", false).FirstOrDefault<TurnTaker>();
                if (turnTaker != null)
                {
                    avianPool = turnTaker.CharacterCard.FindTokenPool(TokenPool.AvianControlPool);
                    arcanaPool = turnTaker.CharacterCard.FindTokenPool(TokenPool.ArcanaControlPool);
                }
            }
            if (avianPool != null && arcanaPool != null)
            {
                string[] words = new string[]
                        {
                            "Flip " + arcanaPool.CurrentValue + " {arcana}",
                            "Flip " + avianPool.CurrentValue + " {avian}"
                        };

                coroutine = this.GameController.SelectWord(this.DecisionMaker, words, SelectionType.HarpyTokenType, storedResultsWord, false, null, this.GetCardSource());
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                string text = this.GetSelectedWord(storedResultsWord);

                if (text != null)
                {
                    TokenPool originPool;
                    TokenPool destinationPool;
                    if (text == "Flip " + avianPool.CurrentValue + " {avian}")
                    {
                        originPool = avianPool;
                        destinationPool = arcanaPool;
                    }
                    else
                    {
                        originPool = arcanaPool;
                        destinationPool = avianPool;
                    }
                    tokensFlipped = originPool.CurrentValue;
                    coroutine = this.FlipTokens(originPool, destinationPool, originPool.CurrentValue, storedResultsToken);
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
            }
            else
            {
                coroutine = this.GameController.SendMessageAction("There are no control tokens in play.", Priority.Medium, this.GetCardSource(), null, true);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }

            // Deal self damage.
            coroutine = this.GameController.DealDamageToTarget(new DamageSource(this.GameController, this.CharacterCard), this.CharacterCard, storedResultsToken.Count(), DamageType.Psychic, cardSource: this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}