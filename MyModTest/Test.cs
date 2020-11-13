using NUnit.Framework;
using System;
using Workshopping;
using Workshopping.MigrantCoder;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Linq;
using System.Collections;
using Handelabra.Sentinels.UnitTest;
using Workshopping.TheBaddies;

namespace MyModTest
{
    [TestFixture()]
    public class Test : BaseTest
    {
        protected TurnTakerController baddies { get { return FindVillain("TheBaddies"); } }
        protected HeroTurnTakerController migrant { get { return FindHero("MigrantCoder"); } }

        [Test()]
        public void TestModWorks()
        {
            SetupGameController("Workshopping.TheBaddies", "Workshopping.MigrantCoder", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(baddies);
            Assert.IsInstanceOf(typeof(TheBaddiesTurnTakerController), baddies);
            Assert.IsInstanceOf(typeof(TheBaddiesCharacterCardController), baddies.CharacterCardController);

            Assert.IsNotNull(migrant);
            Assert.IsInstanceOf(typeof(MigrantCoderTurnTakerController), migrant);
            Assert.IsInstanceOf(typeof(MigrantCoderCharacterCardController), migrant.CharacterCardController);

            Assert.AreEqual(40, baddies.CharacterCard.HitPoints);
            Assert.AreEqual(39, migrant.CharacterCard.HitPoints);

            // Always deals 5 psychic!
            PlayTopCard(baddies);

            Assert.AreEqual(35, baddies.CharacterCard.HitPoints);
            Assert.AreEqual(33, migrant.CharacterCard.HitPoints); // Nemesis!
        }

        [Test()]
        public void TestPunchingBag()
        {
            SetupGameController("BaronBlade", "Workshopping.MigrantCoder", "Megalopolis");

            StartGame();

            GoToUsePowerPhase(migrant);

            // Punching Bag does 1 damage!
            QuickHPStorage(migrant);
            PlayCard("PunchingBag");
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestInnatePower()
        {
            SetupGameController("BaronBlade", "Workshopping.MigrantCoder", "Megalopolis");

            StartGame();

            var mdp = GetCardInPlay("MobileDefensePlatform");

            // Base power draws 3 cards! Deals 1 target 2 damage!
            QuickHandStorage(migrant.ToHero());
            DecisionSelectTarget = mdp;
            QuickHPStorage(mdp);

            UsePower(migrant.CharacterCard);

            QuickHandCheck(3);
            QuickHPCheck(-2);

        }
    }
}
