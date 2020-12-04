using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Parse
{
    public class ParseLaplaceShotCharacterCardController : PromoDefaultCharacterCardController
    {
        public ParseLaplaceShotCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;
            int powerNumeral = this.GetPowerNumeral(0, 1);

            List<SelectLocationDecision> storedResultsDeck = new List<SelectLocationDecision>();
            List<DealDamageAction> storedResultsDamage = new List<DealDamageAction>();

            TurnTakerController env = this.FindEnvironment();
            if (env.TurnTaker.Deck != null)
            {
                // Reveal a card.
                List<Card> storedCardResults = new List<Card>();
                coroutine = this.GameController.RevealCards(base.TurnTakerController, env.TurnTaker.Deck, 1, storedCardResults, false, RevealedCardDisplay.ShowRevealedCards, null, this.GetCardSource(null));
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                coroutine = this.CleanupCardsAtLocations(new List<Location> { env.TurnTaker.Revealed }, env.TurnTaker.Deck, cardsInList: storedCardResults);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }

            int trashCount = env.TurnTaker.Trash.NumberOfCards;

            coroutine = base.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.Card), trashCount, DamageType.Projectile, powerNumeral, false, 0, storedResultsDamage: storedResultsDamage, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (storedResultsDamage.Count > 0 && storedResultsDamage.Any((DealDamageAction dd) => dd.DidDealDamage))
            {
                coroutine = this.GameController.ShuffleTrashIntoDeck(env, cardSource: this.GetCardSource());
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}