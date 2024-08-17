namespace Scripts
{
    public class Damage
    {
        public DamageTypeEnum DamageType;
        public float[] Value = new float[10];

        public void Prepare(CharacterController dealer, CharacterController receiver)
        {
            Value[(int) DamageTypeEnum.Physic] = dealer.EquipedWeapon.GetDamage(dealer, receiver);
        }

        public void Apply(CharacterController dealer, CharacterController receiver)
        {
            receiver.Health -= Value[(int) DamageTypeEnum.Physic];
        }
    }
}