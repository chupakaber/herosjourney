using System.Collections;
using UnityEngine;

namespace Scripts
{
    public class RestoreEffect : Effect
    {
        public enum StatType
        {
            NONE = 0,
            Health = 1,
            Rage = 2
        }

        public StatType Stat = StatType.NONE;
        public float Value;

        public override void Apply(EffectActivationType activationTrigger, CharacterController user, CharacterController targetCharacter)
        {
            if (ActivationTrigger != activationTrigger)
                return;

            switch (Stat)
            {
                case StatType.Health:
                    user.RestoreHealth(Value);
                break;
                case StatType.Rage:
                break;
            }
        }
    }
}