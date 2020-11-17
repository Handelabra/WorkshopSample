using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Workshopping.TheBaddies
{
    public class TheBaddiesCharacterCardController : VillainCharacterCardController
    {
        public TheBaddiesCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }
    }
}
