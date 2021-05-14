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
using System.Collections.Generic;
namespace MyModTest
{
    [TestFixture()]
    public class MigrantCoderTest : BaseTest
    {
        protected HeroTurnTakerController migrant { get { return FindHero("MigrantCoder"); } }

        [Test()]
        public void TestPunchingBag()
        {
            SetupGameController("BaronBlade", "Workshopping.MigrantCoder", "Megalopolis");

            StartGame();

            GoToPlayCardPhase(migrant);

            // Punching Bag does 1 damage! But don't play a card.
            DecisionYesNo = true;
            DecisionDoNotSelectCard = SelectionType.PlayCard;
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

            // Base power draws 1 card! Deals 1 target 2 damage!
            QuickHandStorage(migrant.ToHero());
            DecisionSelectTarget = mdp;
            QuickHPStorage(mdp);

            UsePower(migrant.CharacterCard);

            QuickHandCheck(1);
            QuickHPCheck(-2);

        }

        [Test()]
        public void TestMigrantCoderLockdown()
        {
            SetupGameController("BaronBlade", "Workshopping.MigrantCoder/MigrantCoderLockdown", "Megalopolis");

            StartGame();

            Assert.IsInstanceOf(typeof(MigrantCoderLockdownCharacterCardController), migrant.CharacterCardController);

            GoToUsePowerPhase(migrant);

            // Use power to reduce damage by 1
            QuickHPStorage(migrant);
            UsePower(migrant);
            PlayCard("PunchingBag");
            QuickHPCheck(0);

            SaveAndLoad();

            Assert.IsInstanceOf(typeof(MigrantCoderLockdownCharacterCardController), migrant.CharacterCardController);
        }
    }
}
