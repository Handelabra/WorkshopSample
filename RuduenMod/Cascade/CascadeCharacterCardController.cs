using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using Workshopping.BreachMage;

// TODO: TEST!

namespace Workshopping.Cascade
{
    public class CascadeCharacterCardController : HeroCharacterCardController, ICascadeRiverSharedCardController
    {
        public string str;
        protected static Location _riverDeck;
        protected static Card _riverbank;

        public CascadeCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            _riverbank = null;
            _riverDeck = null;
        }

        // TODO: Figure out why the spell value comparison breaks the check! 

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;
            List<DiscardCardAction> storedResults = new List<DiscardCardAction>();

            // Initial test: Discard up to four cards.
            coroutine = base.GameController.SelectAndDiscardCards(this.HeroTurnTakerController, 4, false, 0, storedResults);
            if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            int? spellValue = 0;
            foreach (DiscardCardAction discard in storedResults)
            {
                spellValue += discard.CardToDiscard.MagicNumber;
            }

            // Select a card under the riverbank whose cost is less than the others.
            // Yes, this is messy, but it's still the cleanest way of mimicing the official SelectCardAndDoAction without access to the evenIfIndestructable flag. Battle Zones shouldn't be an issue. 
            coroutine = this.GameController.SelectCardAndDoAction(
                new SelectCardDecision(this.GameController, this.DecisionMaker, SelectionType.MoveCard, this.GameController.FindCardsWhere((Card c) => c.Location == this.Riverbank().UnderLocation && c.FindTokenPool("CascadeCostPool").MaximumValue <= spellValue)),
                (SelectCardDecision d) => this.GameController.MoveCard(this.DecisionMaker, d.SelectedCard, this.HeroTurnTaker.Trash, false, false, false, null, false, null, null, null, true, false, null, false, false, false, false, this.GetCardSource()),
                false);
            if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

        }

        //public IEnumerator MoveCardTest(SelectCardDecision d)
        //{
        //    IEnumerator coroutine = this.GameController.MoveCard(this.DecisionMaker, d.SelectedCard, this.HeroTurnTaker.Trash, false, false, false, null, false, null, null, null, true, false, null, false, false, false, false, this.GetCardSource());
        //    if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        //}

        //public bool ConfirmCost(Card c, int? spellValue)
        //{
        //    if (c.Location == this.Riverbank().UnderLocation)
        //    {
        //        int? tokenPoolValue = c.FindTokenPool("CascadeCostPool").MaximumValue;
        //        if (tokenPoolValue <= spellValue)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            // TODO: Implement Incapacitated Abilities.
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

        public Location RiverDeck()
        {
            if (CascadeCharacterCardController._riverDeck == null)
            {
                CascadeCharacterCardController._riverDeck = base.TurnTaker.FindSubDeck("RiverDeck");
            }
            return CascadeCharacterCardController._riverDeck;

        }

        public Card Riverbank()
        {
            if (CascadeCharacterCardController._riverbank == null)
            {
                CascadeCharacterCardController._riverbank = base.FindCard("Riverbank", false);
            }
            return CascadeCharacterCardController._riverbank;
        }

    }
}