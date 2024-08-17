using UnityEngine;

namespace Scripts
{
    public class Ability : MonoBehaviour
    {
        [Header("Stats:")]
        public bool UseWeapon;
        public float BaseDamage;
        public float DamageMultiplier;
        public float Range = 0f;
        public float Radius = 1.6f;
        public float Angle = 120f;

        public float GetDamage(CharacterController selfCharacter, CharacterController targetCharacter)
        {
            var damage = BaseDamage;
            
            if (UseWeapon)
            {
                damage += selfCharacter.EquipedWeapon.GetDamage(selfCharacter, targetCharacter) * DamageMultiplier;
            }

            return damage;
        }

        public Ability Equip(CharacterController character, int slot)
        {
            var instance = Instantiate(gameObject).GetComponent<Ability>();
            
            character.EquipedAbilities[slot] = instance;
            
            return instance;
        }
    }
}