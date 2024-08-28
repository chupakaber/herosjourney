using System.Collections;
using UnityEngine;

namespace Scripts
{
    public class Effect : MonoBehaviour
    {
        public enum EffectActivationType
        {
            NONE = 0,
            Use = 1,
            DamageDeal = 2,
            StartAttack = 3
        }

        public EffectActivationType ActivationTrigger = EffectActivationType.NONE;

        public virtual void Apply(EffectActivationType activationTrigger, CharacterController user, CharacterController targetCharacter)
        {
        }
    }
}