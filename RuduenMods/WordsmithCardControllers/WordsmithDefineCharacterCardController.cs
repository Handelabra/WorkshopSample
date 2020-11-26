using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace RuduenWorkshop.Wordsmith
{
    public class WordsmithDefineCharacterCardController : HeroCharacterCardController
    {
        public string str;

        public WordsmithDefineCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;
            List<int> powerNumerals = new List<int>(){
                GetPowerNumeral(0, 1),
                GetPowerNumeral(1, 1)
            };
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
                coroutine = this.GameController.SendMessageAction("{Wordsmith} uses" + spacedPrefixTitle + " " + this.CharacterCard.Definition.Body + spacedSuffixTitle + "!", Priority.Low, this.GetCardSource());
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }

            // Deal 1 target 1 infernal (as a base). 
            coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), powerNumerals[1], DamageType.Projectile, powerNumerals[0], false, powerNumerals[0], false, false, false, null, null, null, null, null, false, null, null, false, null, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Clear all temporary triggers created by this card.
            this.RemoveTemporaryTriggers();
        }

        // TODO: Replace with something more unique!
        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator coroutine;
            switch (index)
            {
                case 0:
                    {
                        coroutine = this.SelectHeroToPlayCard(this.DecisionMaker);
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    }
                case 1:
                    {
                        coroutine = base.GameController.SelectHeroToUsePower(this.DecisionMaker, cardSource: this.GetCardSource(null));
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    }
                case 2:
                    {
                        coroutine = base.GameController.SelectHeroToDrawCard(this.DecisionMaker, cardSource: this.GetCardSource(null));
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    }
            }
            yield break;
        }
    }
}