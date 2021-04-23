using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using Handelabra.Sentinels.Engine.Controller.TheSentinels;

namespace Workshopping.TheSentinels
{
    public class TheSerpentinelsInstructionsCardController : TheSentinelsInstructionsCardController
    {
        public TheSerpentinelsInstructionsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }
    }
}
