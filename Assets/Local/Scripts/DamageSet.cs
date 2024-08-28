using System;

namespace Scripts
{
    [Serializable]
    public class DamageSet
    {
        [Serializable]
        public class DamageSetPart
        {
            public StatModifierEnum StatModifier;
            public bool FromTarget;
            public float Value;
        }

        public DamageTypeEnum DamageType;
        public DamageSetPart[] Components = new DamageSetPart[0];

        public float GetValue(CharacterController user, CharacterController targetCharacter)
        {
            var value = 0f;
            foreach (var component in Components)
            {
                switch (component.StatModifier)
                {
                    case StatModifierEnum.AsIs:
                        value += component.Value;
                    break;
                    case StatModifierEnum.Strenght:
                        value += component.Value * (component.FromTarget ? targetCharacter : user).Strenght;
                    break;
                    case StatModifierEnum.Agility:
                        value += component.Value * (component.FromTarget ? targetCharacter : user).Agility;
                    break;
                    case StatModifierEnum.Intelligence:
                        value += component.Value * (component.FromTarget ? targetCharacter : user).Intelligence;
                    break;
                    case StatModifierEnum.Vitality:
                        value += component.Value * (component.FromTarget ? targetCharacter : user).Vitality;
                    break;
                }
            }
            switch (DamageType)
            {
                case DamageTypeEnum.Physic:
                    value *= (2000 + targetCharacter.ArmorClass) / (2000 + targetCharacter.ArmorClass * 10);
                break;
                case DamageTypeEnum.Dark:
                case DamageTypeEnum.Fire:
                case DamageTypeEnum.Frost:
                case DamageTypeEnum.Light:
                case DamageTypeEnum.Lightning:
                case DamageTypeEnum.Nature:
                case DamageTypeEnum.Venom:
                
                break;
            }
            var resistance = targetCharacter.Resistance[(int) DamageType];
            value *= (500 + resistance) / (500 + resistance * 5);
            return value;
        }
    }
}