namespace Scripts
{
    public class Damage
    {
        public bool IsCritical;
        public float[] Value = new float[10];

        public void Prepare(CharacterController dealer, CharacterController receiver, DamageSet[] damageSets)
        {
            foreach (var damageSet in damageSets)
            {
                Value[(int) damageSet.DamageType] = damageSet.GetValue(dealer, receiver, IsCritical);
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