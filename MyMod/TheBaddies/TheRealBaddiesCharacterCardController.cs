using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Workshopping.TheBaddies
{
    public class TheRealBaddiesCharacterCardController : VillainCharacterCardController
    {
        public TheRealBaddiesCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }
    }
}
