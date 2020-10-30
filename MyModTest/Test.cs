using NUnit.Framework;
using System;
using Workshopping;
using Workshopping.MigrantCoder;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Linq;
using System.Reflection;
using System.Collections;

namespace MyModTest
{
    [TestFixture()]
    public class Test
    {
        private bool _continueRunningGame;
        private GameController GameController;

        protected GameController SetupGameController(Game game)
        {
            GameController gameController = new GameController(game);
            gameController.StartCoroutine = StartCoroutine;
            gameController.ExhaustCoroutine = RunCoroutine;
            gameController.OnMakeDecisions -= this.MakeDecisions;
            gameController.OnMakeDecisions += this.MakeDecisions;
            gameController.OnSendMessage += this.ReceiveMessage;
            gameController.OnWillPerformAction += this.WillPerformAction;
            gameController.OnWillApplyActionChanges += this.WillApplyActionChanges;
            gameController.OnDidPerformAction += this.DidPerformAction;

            this.GameController = gameController;
            this._continueRunningGame = true;

            return gameController;
        }

    
        [Test()]
        public void TestCase()
        {
            var a = Assembly.GetAssembly(typeof(MigrantCoderCharacterCardController));
            Assert.IsNotNull(a);

            ModHelper.AddAssembly("Workshopping", a);

            var game = new Game(new string[] { "BaronBlade", "Workshopping.MigrantCoder", "Megalopolis" });

            var gc = SetupGameController(game);

            Assert.AreEqual(3, gc.TurnTakerControllers.Count());

            var migrant = gc.FindTurnTakerController("MigrantCoder");
            Assert.IsNotNull(migrant);
            Assert.IsInstanceOf(typeof(MigrantCoderTurnTakerController), migrant);
            Assert.IsInstanceOf(typeof(MigrantCoderCharacterCardController), migrant.CharacterCardController);

            Assert.AreEqual(39, migrant.CharacterCard.HitPoints);

            StartGame();

            var bag = this.GameController.FindCard("PunchingBag");
            RunCoroutine(this.GameController.PlayCard(migrant, bag));
            Assert.AreEqual(38, migrant.CharacterCard.HitPoints);

            var worst = this.GameController.FindCard("WorstCardEver");
            RunCoroutine(this.GameController.PlayCard(migrant, worst));

            RunCoroutine(this.GameController.UsePower(migrant.CharacterCard, 0));
        }

        protected void StartGame()
        {

            RunCoroutine(this.GameController.StartGame());


            EnterNextTurnPhase();
        }

        protected void EnterNextTurnPhase(int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                this.RunCoroutine(this.GameController.EnterNextTurnPhase());
            }
        }

        protected void RunCoroutine(IEnumerator e)
        {
            while (_continueRunningGame && e.MoveNext())
            {

            }
        }

        protected IEnumerator StartCoroutine(IEnumerator e)
        {
            while (_continueRunningGame && e.MoveNext())
            {

            }

            yield return null;
        }

        // You can override this, or just set the properties that it will use to make decisions.
        protected virtual IEnumerator MakeDecisions(IDecision decision)
        {
            // Make sure we are not allowing fast coroutines!
            if (this.GameController.PeekFastCoroutines())
            {
                Assert.Fail("MakeDecisions was forcing fast coroutines!");
            }

            Console.WriteLine("MakeDecisions: " + decision.ToStringForMultiplayerDebugging());

            yield return null;
        }

        private IEnumerator MakeDecisions(IDecision decision, string failAfter)
        {
            this.StartCoroutine(MakeDecisions(decision));

            if (failAfter != null)
            {
                Assert.Fail(failAfter);
            }

            yield return null;
        }

        public IEnumerator ReceiveMessage(MessageAction message)
        {
            string msg = message.Message;
            Console.WriteLine("Message: " + msg);
            yield return null;
        }

        private IEnumerator WillPerformAction(GameAction gameAction)
        {
            yield return null;
        }

        private IEnumerator WillApplyActionChanges(GameAction gameAction)
        {
            yield return null;
        }

        private IEnumerator DidPerformAction(GameAction gameAction)
        {
            if (gameAction is GameOverAction && gameAction.IsSuccessful)
            {
                Console.WriteLine("GAME OVER");
                this._continueRunningGame = false;
            }

            yield return null;
        }
    }
}
