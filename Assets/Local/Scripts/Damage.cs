namespace Scripts
{
    public class Damage
    {
        public float[] Value = new float[10];

        public void Prepare(CharacterController dealer, CharacterController receiver)
        {
            foreach (var damageSet in dealer.EquipedWeapon.DamageSets)
            {
                Value[(int) damageSet.DamageType] = damageSet.GetValue(dealer, receiver);
            }
        }

        public void Apply(CharacterController dealer, CharacterController receiver)
        {
            for (var i = 0; i < Value.Length; i++)
            {
                var damage = Value[i];
                if (damage > 0f)
                {
                    receiver.Health -= damage;
                }
            }
        }
    }
}