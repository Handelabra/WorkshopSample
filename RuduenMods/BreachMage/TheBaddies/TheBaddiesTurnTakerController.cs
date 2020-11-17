using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;

namespace Workshopping.TheBaddies
{
    public class TheBaddiesTurnTakerController:TurnTakerController
    {
        public TheBaddiesTurnTakerController(TurnTaker turnTaker, GameController gameController)
            : base(turnTaker, gameController)
        {
        }
    }
}
