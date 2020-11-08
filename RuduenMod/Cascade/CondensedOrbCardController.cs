using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using Workshopping.Cascade;

namespace Workshopping.Cascade
{
    // TODO: TEST!
    public class CondensedOrbCardController : CascadeRiverSharedCardController
    {
        public CondensedOrbCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            yield return null;
        }
    }
}