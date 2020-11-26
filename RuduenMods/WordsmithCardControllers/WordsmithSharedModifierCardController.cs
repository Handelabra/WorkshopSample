using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace RuduenWorkshop.Wordsmith
{
    public abstract class WordsmithSharedModifierCardController : CardController
    {
        public WordsmithSharedModifierCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

            this.OriginalTargets = new List<Card>();
        }

        private readonly List<Card> OriginalTargets;

        public virtual ITrigger AddModifierTrigger(CardSource cardSource)
        {
            this.OriginalTargets.Clear();
            return null;
        }

        protected virtual IEnumerator TrackOriginalTargetsAndRunResponse(DealDamageAction dd, CardSource cardSource, params object[] otherItems)
        {
            if (!this.OriginalTargets.Contains(dd.OriginalTarget))
            {
                // Best way to 'track' to make sure only base instances of damage trigger the effect. 
                if (!dd.IsPretend)
                {
                    OriginalTargets.Add(dd.OriginalTarget);
                }
                IEnumerator coroutine = RunResponse(dd, cardSource, otherItems);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        protected virtual IEnumerator RunResponse(DealDamageAction dd, CardSource cardSource, params object[] otherObjects)
        {
            // Override in each! 
            yield break;
        }
    }
}