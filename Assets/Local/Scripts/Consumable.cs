using System.Collections;
using UnityEngine;

namespace Scripts
{
    public class Consumable : ItemBase
    {
        public Effect[] Effects;

        public override void Use(CharacterController character)
        {
            base.Use(character);
            
            foreach (var effect in Effects)
            {
                effect.Apply(Effect.EffectActivationType.Use, character, null);
            }
        }

        public override void Remove()
        {
            base.Remove();
        }
    }
}