using UnityEngine;

namespace Scripts
{
    public class VisualEffect : Effect
    {
        public ParticleSystem particleSystem;
        
        public Transform StartPoint;
        public Transform EndPoint;
        public Vector3 StartPosition;
        public Vector3 EndPosition;

        public override void Apply(EffectActivationType activationTrigger, CharacterController user, CharacterController targetCharacter)
        {
            particleSystem.transform.position = StartPosition;
            particleSystem.transform.rotation = Quaternion.LookRotation(EndPosition - StartPosition, Vector3.up);
            particleSystem.Play();
        }

        void Update()
        {
            if (particleSystem.isPlaying)
            {
                if (StartPoint != null)
                {
                    particleSystem.transform.position = StartPoint.position;
                    particleSystem.transform.rotation = Quaternion.LookRotation(EndPoint.position - StartPoint.position, Vector3.up);
                }
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }
    }
}