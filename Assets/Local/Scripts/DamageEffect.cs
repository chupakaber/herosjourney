using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scripts
{
    public class DamageEffect : Effect
    {
        public DamageSet DamageSet;
        
        public override void Apply(EffectActivationType activationTrigger, CharacterController user, CharacterController targetCharacter)
        {
            if (activationTrigger != ActivationTrigger)
                return;

            var damage = DamageSet.GetValue(user, targetCharacter);

        }
    }
}