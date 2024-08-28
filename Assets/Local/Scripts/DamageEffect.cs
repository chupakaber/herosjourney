namespace Scripts
{
    public class DamageEffect : Effect
    {
        public DamageSet[] DamageSets = new DamageSet[0];
        public VisualEffect[] VisualEffects = new VisualEffect[0];
        
        public override void Apply(EffectActivationType activationTrigger, CharacterController user, CharacterController targetCharacter)
        {
            if (activationTrigger != ActivationTrigger)
                return;

            var damage = new Damage();
            damage.Prepare(user, targetCharacter, DamageSets);
            damage.Apply(user, targetCharacter);

            foreach (var visualEffectPrefab in VisualEffects)
            {
                var visualEffect = Instantiate(visualEffectPrefab.gameObject).GetComponent<VisualEffect>();
                visualEffect.StartPosition = targetCharacter.transform.position;
                visualEffect.EndPosition = user.transform.position;
                visualEffect.Apply(activationTrigger, user, targetCharacter);
            }
        }
    }
}