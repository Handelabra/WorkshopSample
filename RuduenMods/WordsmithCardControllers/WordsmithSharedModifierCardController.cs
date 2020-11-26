using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Wordsmith
{
    public abstract class WordsmithSharedModifierCardController : CardController
    {
        public WordsmithSharedModifierCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        private CardSource baseCardSource;

        public virtual ITrigger AddModifierTrigger(CardSource cardSource)
        {
            baseCardSource = this.GetCardSource(); // "Base" is a single instance of the card at moment of discard. Used to prevent self-triggers.
            return AddModifierTriggerOverride(cardSource);
        }

        protected virtual ITrigger AddModifierTriggerOverride(CardSource cardSource)
        {
            return null;
        }

        protected virtual IEnumerator TrackOriginalTargetsAndRunResponse(DealDamageAction dd, CardSource cardSource, params object[] otherItems)
        {
            if (!cardSource.AssociatedCardSources.Contains(baseCardSource)) 
            {
                // Indicate this is a related card for that damage instance only!
                if (this.IsRealAction())
                {
                    cardSource.AddAssociatedCardSource(baseCardSource);
                }

                // Best way to 'track' to make sure only base instances of damage trigger the effect. 
                IEnumerator coroutine = RunResponse(dd, cardSource, otherItems);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                if (this.IsRealAction())
                {
                    cardSource.RemoveAssociatedCardSourcesWhere((CardSource cs) => cs == baseCardSource);
                }

            }
        }

        protected virtual IEnumerator RunResponse(DealDamageAction dd, CardSource cardSource, params object[] otherObjects)
        {
            // Override in each! 
            yield break;
        }
    }
}