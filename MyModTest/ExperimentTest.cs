using System;
using NUnit.Framework;
using Handelabra.Sentinels.Engine.Model;
using System.Collections.Generic;
using System.Linq;

namespace Handelabra.Sentinels.UnitTest
{
    [TestFixture]
    public class ExperimentTest : BaseTest
    {
        [Test]
        public void TestSuperScientificTachyonProbability()
        {
            var allDeckIdentifiers = new List<string>();
            allDeckIdentifiers.AddRange(DeckDefinition.AvailableHeroes);
            allDeckIdentifiers.AddRange(DeckDefinition.AvailableVillains);
            allDeckIdentifiers.AddRange(DeckDefinition.AvailableVillainTeamMembers);
            allDeckIdentifiers.AddRange(DeckDefinition.AvailableEnvironments);
            SetupGameController(allDeckIdentifiers);
            CheckSSTProbability();

            SetupGameController(DeckDefinition.OblivAeonDeck);
            CheckSSTProbability();
        }

        private void CheckSSTProbability()
        {
            foreach (var hero in this.GameController.AllHeroControllers)
            {
                hero.HeroTurnTaker.MoveAllCards(hero.HeroTurnTaker.Hand, hero.HeroTurnTaker.Deck);
            }

            foreach (var tt in this.GameController.TurnTakerControllers.Select(ttc => ttc.TurnTaker))
            {
                CheckDeckProbability(tt.Deck, tt.Name);
                if (tt.SubDecks != null)
                {
                    foreach (var subdeck in tt.SubDecks)
                    {
                        CheckDeckProbability(subdeck, subdeck.SubDeckName);
                    }
                }
            }
        }

        private void CheckDeckProbability(Location deck, string name)
        {
            float canPlay = 0;
            float combo = 0;

            var cards = deck.Cards.Where(c => !c.IsCharacter && c.IsRealCard && !c.IsShield && c.Identifier != "SafeHouse");

            foreach (Card a in cards)
            {
                foreach (Card b in cards)
                {
                    if (a != b)
                    {
                        combo++;
                        if (a.Definition.Keywords.Any(b.Definition.Keywords.Contains))
                        {
                            canPlay++;
                        }
                    }
                }
            }

            var play = (canPlay / combo) * 100f;
            var discard = 100f - play;
            Console.WriteLine(name + "\t " + play + "\t " + discard, 1);
        }

        [Test]
        public void TestSuperScientificTachyonProbabilityMonteCarlo()
        {

            var allDeckIdentifiers = new List<string>();
            allDeckIdentifiers.AddRange(DeckDefinition.AvailableHeroes);
            allDeckIdentifiers.AddRange(DeckDefinition.AvailableVillains);
            allDeckIdentifiers.AddRange(DeckDefinition.AvailableVillainTeamMembers);
            allDeckIdentifiers.AddRange(DeckDefinition.AvailableEnvironments);
            SetupGameController(allDeckIdentifiers);
            CheckSSTMonteCarlo();

            SetupGameController(DeckDefinition.OblivAeonDeck);
            CheckSSTMonteCarlo();
        }

        private void CheckSSTMonteCarlo()
        {
            foreach (var hero in this.GameController.AllHeroControllers)
            {
                hero.HeroTurnTaker.MoveAllCards(hero.HeroTurnTaker.Hand, hero.HeroTurnTaker.Deck);
            }

            foreach (var tt in this.GameController.TurnTakerControllers.Select(ttc => ttc.TurnTaker))
            {
                CheckDeckMonteCarlo(tt.Deck, tt.Name);
                if (tt.SubDecks != null)
                {
                    foreach (var subdeck in tt.SubDecks)
                    {
                        CheckDeckMonteCarlo(subdeck, subdeck.SubDeckName);
                    }
                }
            }
        }

        private void CheckDeckMonteCarlo(Location deck, string name)
        {
            int numTrials = 10;
            //int numTrials = 1000000;
            int numPlays = 0;

            for (int i = 0; i < numTrials; i++)
            {
                deck.ShuffleCards();

                var allowedCards = deck.Cards.Where(c => !c.IsCharacter && c.IsRealCard && !c.IsShield && c.Identifier != "SafeHouse");
                var cards = allowedCards.Take(2);
                var a = cards.First();
                var b = cards.Last();

                if (a.Definition.Keywords.Any(b.Definition.Keywords.Contains))
                {
                    numPlays++;
                }
            }

            var play = 100f * (float)numPlays / (float)numTrials;
            var discard = 100f - play;
            Console.WriteLine(name + "\t " + play + "\t " + discard, 1);
        }
    }
}
