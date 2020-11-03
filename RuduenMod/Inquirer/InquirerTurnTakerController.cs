using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.Inquirer
{
    public class InquirerTurnTakerController : HeroTurnTakerController
    {
        public InquirerTurnTakerController(TurnTaker turnTaker, GameController gameController)
            : base(turnTaker, gameController)
        {
        }
    }
}
