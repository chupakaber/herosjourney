using UnityEngine;

namespace Scripts
{
    public class EnemyAI : MonoBehaviour
    {
        public CharacterController character;
        public CharacterController target;

        public Vector3 SpawnPoint;
        public float ActivityRadius = 30f;
        public float HomeRadius = 10f;
        public float AggressiveRadius = 3f;

        [Header("Runtime")]
        public int State = 0;
        public Vector3 IdleTargetPoint;

        private float _lastActivityTime = 0f;
        private float _idleActivityCooldown;
        private float _lastRestorationTime;

        void Start()
        {
            character = GetComponent<CharacterController>();
            
            var characters = FindObjectsOfType<CharacterController>();
            foreach (var other in characters)
            {
                if (other.MainCamera != null)
                {
                    target = other;
                }
            }

            SpawnPoint = transform.position;
            IdleTargetPoint = SpawnPoint;
            _idleActivityCooldown = Random.Range(3f, 10f);
        }

        void FixedUpdate()
        {
            if (State == 0)
            {
                if (Time.time - _lastRestorationTime > 1f)
                {
                    if (character.Health < character.MaxHealth)
                    {
                        character.Health = Mathf.Min(character.Health + character.MaxHealth * 0.2f, character.MaxHealth);
                        character.OnDamaged(new Damage(), character);
                    }
                    _lastRestorationTime = Time.time;
                }

                if (Time.time - _lastActivityTime > _idleActivityCooldown)
                {
                    IdleTargetPoint = SpawnPoint + Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * new Vector3(0f, 0f, 1f) * Random.Range(0f, HomeRadius);
                    var delta = IdleTargetPoint - transform.position;
                    IdleTargetPoint = transform.position + delta.normalized * Mathf.Min(delta.magnitude, 7f);
                    _idleActivityCooldown = Random.Range(3f, 10f);
                    _lastActivityTime = Time.time;
                }

                var direction = IdleTargetPoint - transform.position;
                direction.y = 0f;
                if (direction.magnitude < 2f)
                {
                    direction = Vector3.zero;
                }
                character.PointMovementDirection = direction;

                var targetDirection = target.transform.position - transform.position;
                targetDirection.y = 0f;
                if (targetDirection.magnitude < AggressiveRadius)
                {
                    State = 1;
                }
            }
            else if (State == 1)
            {
                var spawnPointDelta = SpawnPoint - transform.position;
                spawnPointDelta.y = 0f;
                
                var direction = target.transform.position - transform.position;
                direction.y = 0f;

                if (spawnPointDelta.magnitude > ActivityRadius)
                {
                    State = 2;
                }
                else if (direction.magnitude > character.EquipedWeapon.AttackRange + character.EquipedWeapon.AttackRadius)
                {
                    character.PointMovementDirection = direction;
                    character.AttackMode = false;
                    character.TargetCharacter = null;
                }
                else
                {
                    character.PointMovementDirection = Vector3.zero;
                    character.AttackMode = true;
                    character.InvokeAttack();
                    character.TargetCharacter = target;
                }
            }
            else if (State == 2)
            {
                var spawnPointDelta = SpawnPoint - transform.position;
                spawnPointDelta.y = 0f;

                if (spawnPointDelta.magnitude > HomeRadius)
                {
                    character.PointMovementDirection = spawnPointDelta;
                    character.AttackMode = false;
                    character.TargetCharacter = null;
                }
                else
                {
                    character.PointMovementDirection = Vector3.zero;
                    State = 0;
                }
            }
        }
    }
}