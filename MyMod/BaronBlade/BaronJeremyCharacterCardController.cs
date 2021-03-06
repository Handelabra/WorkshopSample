using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Workshopping.BaronBlade
{
    public class BaronJeremyCharacterCardController : VillainCharacterCardController
    {
        public BaronJeremyCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }
    }
}
