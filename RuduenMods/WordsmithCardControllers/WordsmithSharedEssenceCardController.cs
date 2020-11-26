using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Wordsmith
{
    public abstract class WordsmithSharedEssenceCardController : CardController
    {
        public WordsmithSharedEssenceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        protected virtual IEnumerator PerformModifiedAction()
        {
            // Deal 1 target 1 infernal (as a base). 
            IEnumerator coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), 1, DamageType.Projectile, 1, false, 1, false, false, false, null, null, null, null, null, false, null, null, false, null, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;
            List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
            string spacedPrefixTitle = "";
            string spacedSuffixTitle = "";

            // Discard prefix.
            coroutine = this.GameController.SelectAndDiscardCard(this.DecisionMaker, true, (Card c) => c.DoKeywordsContain("prefix"), storedResults, SelectionType.DiscardCard);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (storedResults.Count > 0 && storedResults.FirstOrDefault().IsSuccessful)
            {
                Card c = storedResults.FirstOrDefault().CardToDiscard;
                CardController cc = this.FindCardController(c);
                if (cc is WordsmithSharedModifierCardController)
                {
                    // Type matches, everything should be implemented now!
                    WordsmithSharedModifierCardController wcc = (WordsmithSharedModifierCardController)this.FindCardController(c);
                    this.AddToTemporaryTriggerList(wcc.AddModifierTrigger(this.GetCardSource()));
                    spacedPrefixTitle = " " + c.Title;
                }
            }

            // Discard suffix.
            storedResults.Clear();
            coroutine = this.GameController.SelectAndDiscardCard(this.DecisionMaker, true, (Card c) => c.DoKeywordsContain("suffix"), storedResults, SelectionType.DiscardCard);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (storedResults.Count > 0 && storedResults.FirstOrDefault().IsSuccessful)
            {
                Card c = storedResults.FirstOrDefault().CardToDiscard;
                CardController cc = this.FindCardController(c);
                if (cc is WordsmithSharedModifierCardController)
                {
                    // Type matches, everything should be implemented now!
                    WordsmithSharedModifierCardController wcc = (WordsmithSharedModifierCardController)this.FindCardController(c);
                    this.AddToTemporaryTriggerList(wcc.AddModifierTrigger(this.GetCardSource()));
                    spacedSuffixTitle = " " + c.Title;
                }
            }

            if (spacedPrefixTitle.Length > 0 || spacedSuffixTitle.Length > 0)
            {
                coroutine = this.GameController.SendMessageAction("{Wordsmith} uses" + spacedPrefixTitle + " " + this.Card.Title + spacedSuffixTitle + "!", Priority.Low, this.GetCardSource());
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }

            coroutine = this.PerformModifiedAction();
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Clear all temporary triggers created by this card.
            this.RemoveTemporaryTriggers();
        }
    }
}