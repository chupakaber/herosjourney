using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Scripts
{
    public enum CharacterClassType
    {
        NONE = 0,
        Warrior = 1,
        Dragun = 2,
        Kirasir = 3,
        Ranger = 4,
        Sniper = 5,
        Duelist = 6,
        Wizard = 7,
        Sorcerer = 8,
        Mage = 9
    }

    public class CharacterClass 
    {
        public static CharacterClass[] CharacterClasses = new CharacterClass[10] {
            new CharacterClass() { StrenghtModifier = 0, VitalityModifier = 0, AgilityModifier = 0, IntelligenceModifier = 0 },
            new CharacterClass() { StrenghtModifier = 0, VitalityModifier = 15, AgilityModifier = 0, IntelligenceModifier = 0 },
            new CharacterClass() { StrenghtModifier = 0, VitalityModifier = 13, AgilityModifier = 0, IntelligenceModifier = 0 },
            new CharacterClass() { StrenghtModifier = 0, VitalityModifier = 20, AgilityModifier = 0, IntelligenceModifier = 0 },
            new CharacterClass() { StrenghtModifier = 0, VitalityModifier = 7, AgilityModifier = 0, IntelligenceModifier = 0 },
            new CharacterClass() { StrenghtModifier = 0, VitalityModifier = 5, AgilityModifier = 0, IntelligenceModifier = 0 },
            new CharacterClass() { StrenghtModifier = 0, VitalityModifier = 8, AgilityModifier = 0, IntelligenceModifier = 0 },
            new CharacterClass() { StrenghtModifier = 0, VitalityModifier = 3, AgilityModifier = 0, IntelligenceModifier = 0 },
            new CharacterClass() { StrenghtModifier = 0, VitalityModifier = 5, AgilityModifier = 0, IntelligenceModifier = 0 },
            new CharacterClass() { StrenghtModifier = 0, VitalityModifier = 4, AgilityModifier = 0, IntelligenceModifier = 0 }
        };

        public float StrenghtModifier;
        public float VitalityModifier;
        public float AgilityModifier;
        public float IntelligenceModifier;
    }

    public class CharacterController : MonoBehaviour
    {
        [Header("Links")]
        public Camera MainCamera;
        public Animator Animator;
        public Transform CameraPivot;
        public Transform HipsTransform;
        public Transform RightHandPivot;
        public Transform LeftHandPivot;
        public TrailRenderer DashTrail;
        public Weapon EquipedWeapon;
        public Ability[] EquipedAbilities = new Ability[6];
        public QuickSlot[] QuickSlots = new QuickSlot[4];
        public MeshRenderer HealthBarRenderer;
        public MeshRenderer DashCooldownRenderer;
        public NavMeshAgent NavMeshAgent;

        public Weapon DefaultWeaponPrefab;
        public Ability[] DefaultAbilities = new Ability[6];

        [Header("Control Config")]
        public Vector2 CameraSensitivity = Vector2.one;
        public float ScrollSensitivity = 0.1f;
        
        [Header("Stats")]
        public CharacterClassType ClassType;
        public float MovingSpeed = 3f;
        public float Strenght;
        public float Vitality;
        public float Agility;
        public float Intelligence;
        public float ArmorClass;

        public float[] Resistance = new float[10];
        
        public float MaxHealth { get { return Vitality * (5f + CharacterClass.CharacterClasses[(int) ClassType].VitalityModifier) + SkillHealthModifier; } }
        public float DashCooldown = 0.8f;

        [Header("Runtime")]
        public float Health;
        public float SkillHealthModifier;
        public float Crit;
        public float ClassResource;
        public Vector3 CameraRotationEuler;
        public Vector3 PointMovementDirection;
        public Vector3 DirectMovementDirection;
        public float MovementVelocity = 0f;
        public CharacterController TargetCharacter = null;
        public bool InStand;
        public bool AttackMode;
        public bool AttackInProgress;

        [Header("UI")]
        [SerializeField]
        private TMP_Text _stats;
        [SerializeField]
        private TMP_Text _enemyStats;
        [SerializeField]
        private TMP_Text _damageDealtLogText;
        [SerializeField]
        private TMP_Text _damageTakenLogText;
        [SerializeField]
        private Image _critImageValue;
        [SerializeField]
        private Image _rageImageValue;
        [SerializeField]
        private Image _exprerienceImageValue;

        private InputActions _inputActions;
        private int _animationKeyHorizontalVelocity = Animator.StringToHash("HorizontalVelocity");
        private int _animationKeyAttack = Animator.StringToHash("Attack");
        private int _animationKeyShooting = Animator.StringToHash("Shooting");
        private int _animationKeyDash = Animator.StringToHash("Dash");
        private int _animationKeyDamaged = Animator.StringToHash("Damaged");
        private int _animationKeyDeath = Animator.StringToHash("Death");
        private int _shaderKeyValue = Shader.PropertyToID("_Value");
        private Vector3 _baseHipsLocalPosition = Vector3.zero;
        private bool _inAttack = false;
        private bool _lookMode = false;
        private Vector2 _pointOnScreen;
        private bool _moving = false;
        private Vector3 _targetPosition;
        private Collider[] _resultColliders = new Collider[100];
        private RaycastHit[] _resultHits = new RaycastHit[100];
        private float _dashLastUseTime = 0f;
        private MaterialPropertyBlock _propertyBlock;
        private float _lastRecalculatePathTime = 0f;
        private NavMeshPath _navMeshPath;
        private Vector3[] _pathCorners = new Vector3[100];

        private LinkedList<string> _damageDealtLog = new LinkedList<string>();
        private LinkedList<string> _damageTakenLog = new LinkedList<string>();

        public void Awake()
        {
            if (MainCamera != null)
                _inputActions = new InputActions();
            _baseHipsLocalPosition = HipsTransform.localPosition;
            _targetPosition = transform.position;
            DashTrail.enabled = false;

            Health = MaxHealth;
            _propertyBlock = new MaterialPropertyBlock();
            _navMeshPath = new NavMeshPath();

            DefaultWeaponPrefab.Equip(this);
            for (var i = 0; i < DefaultAbilities.Length; i++)
            {
                if (DefaultAbilities[i] != null)
                {
                    DefaultAbilities[i].Equip(this, i);
                }
            }
        }

        public void OnEnable()
        {
            if (_inputActions != null)
            {
                _inputActions.Main.Movement.performed += OnMovement;
                _inputActions.Main.Movement.canceled += OnMovement;
                _inputActions.Main.Move.performed += OnMove;
                _inputActions.Main.Move.canceled += OnMove;
                _inputActions.Main.Point.performed += OnPoint;
                _inputActions.Main.Look.performed += OnLook;
                _inputActions.Main.LookMode.performed += OnLookMode;
                _inputActions.Main.LookMode.canceled += OnLookMode;
                _inputActions.Main.Zoom.performed += OnZoom;
                _inputActions.Main.Attack.performed += OnAttack;
                _inputActions.Main.Attack.canceled += OnAttack;
                _inputActions.Main.Stand.started += OnStand;
                _inputActions.Main.Stand.canceled += OnStand;
                _inputActions.Main.Dash.performed += OnDash;
                _inputActions.Main.QuickSlot1.performed += OnQuickSlot;
                _inputActions.Main.QuickSlot2.performed += OnQuickSlot;
                _inputActions.Main.QuickSlot3.performed += OnQuickSlot;
                _inputActions.Main.QuickSlot4.performed += OnQuickSlot;
                _inputActions.Enable();
            }
        }

        public void OnDisable()
        {
            if (_inputActions != null)
            {
                _inputActions.Main.Movement.performed -= OnMovement;
                _inputActions.Main.Movement.canceled -= OnMovement;
                _inputActions.Main.Move.performed -= OnMove;
                _inputActions.Main.Move.canceled -= OnMove;
                _inputActions.Main.Point.performed -= OnPoint;
                _inputActions.Main.Look.performed -= OnLook;
                _inputActions.Main.LookMode.performed -= OnLookMode;
                _inputActions.Main.LookMode.canceled -= OnLookMode;
                _inputActions.Main.Zoom.performed -= OnZoom;
                _inputActions.Main.Attack.performed -= OnAttack;
                _inputActions.Main.Attack.canceled -= OnAttack;
                _inputActions.Main.Stand.started -= OnStand;
                _inputActions.Main.Stand.canceled -= OnStand;
                _inputActions.Main.Dash.performed -= OnDash;
                _inputActions.Main.QuickSlot1.performed -= OnQuickSlot;
                _inputActions.Main.QuickSlot2.performed -= OnQuickSlot;
                _inputActions.Main.QuickSlot3.performed -= OnQuickSlot;
                _inputActions.Main.QuickSlot4.performed -= OnQuickSlot;
                _inputActions.Disable();
            }
        }


        public void FixedUpdate()
        {
            var targetVelocity = 0f;

            if (!_inAttack)
            {
                var worldMovementDirection = Vector3.zero;
                if (AttackInProgress)
                {
                    if (TargetCharacter != null && !InStand)
                    {
                        var delta = TargetCharacter.transform.position - transform.position;
                        if (delta.magnitude < EquipedWeapon?.AttackRange + EquipedWeapon?.AttackRadius)
                        {
                            InvokeAttack();
                        }
                        else
                        {
                            worldMovementDirection = delta.normalized;
                        }
                    }
                    else if (InStand)
                    {
                        var ray = MainCamera.ScreenPointToRay(_pointOnScreen);
                        if (_moving && new Plane(Vector3.up, transform.position).Raycast(ray, out var distance))
                        {
                            var targetPosition = ray.GetPoint(distance);
                            var delta = targetPosition - transform.position;
                            delta.y = 0f;
                            transform.rotation = Quaternion.LookRotation(delta);
                        }

                        InvokeAttack();
                    }
                }
                else if (InStand)
                {

                }
                else if (DirectMovementDirection.sqrMagnitude > float.Epsilon)
                {
                    worldMovementDirection = Quaternion.Euler(0f, CameraPivot.rotation.eulerAngles.y, 0f) * DirectMovementDirection;
                }
                else if (PointMovementDirection.sqrMagnitude > float.Epsilon)
                {
                    worldMovementDirection = PointMovementDirection;
                }
                if (worldMovementDirection.sqrMagnitude > float.Epsilon)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(worldMovementDirection), Time.fixedDeltaTime  * 5f);
                    targetVelocity = MovingSpeed;
                }
            }
            MovementVelocity = Mathf.Lerp(MovementVelocity, targetVelocity, Time.fixedDeltaTime * (targetVelocity > MovementVelocity ? 1f : 5f));
            Animator.SetFloat(_animationKeyHorizontalVelocity, MovementVelocity);
            // var storedHipsPosition = HipsTransform.position;
            // transform.position = HipsTransform.InverseTransformPoint(HipsTransform.rotation * _baseHipsLocalPosition);
            // HipsTransform.position = storedHipsPosition;

            if (AttackMode)
            {
                // ---
                if (MainCamera != null)
                {
                    var ray = MainCamera.ScreenPointToRay(_pointOnScreen);
                    var hitCount = Physics.RaycastNonAlloc(ray, _resultHits, 1000f);
                    for (var i = 0; i < hitCount; i++)
                    {
                        var hitInfo = _resultHits[i];
                        if (!hitInfo.collider.gameObject.Equals(gameObject) && hitInfo.collider.gameObject.TryGetComponent<CharacterController>(out var targetCharacterController))
                        {
                            // Debug.Log($"Target character: {targetCharacterController.name}");
                            TargetCharacter = targetCharacterController;
                            i = hitCount;
                        }
                    }
                }

                if (TargetCharacter != null)
                {
                    _targetPosition = TargetCharacter.transform.position;
                    PointMovementDirection = _targetPosition - transform.position;
                }
                // ---
            }

            ShowStats();
            ShowEnemyStats();
        }

        public void Update()
        {
            if (MainCamera != null)
            {
                var cameraPivotTargetPosition = transform.position + Vector3.up;
                CameraPivot.transform.position = Vector3.Lerp(CameraPivot.transform.position, cameraPivotTargetPosition, Time.deltaTime * 10f);
                var targetRotation = Quaternion.Euler(0f, CameraRotationEuler.y, 0f) * Quaternion.Euler(CameraRotationEuler.x, 0f, 0f);
                CameraPivot.rotation = Quaternion.Lerp(CameraPivot.rotation, targetRotation, Time.deltaTime * 10f);
            
                var ray = MainCamera.ScreenPointToRay(_pointOnScreen);
                if (_moving && new Plane(Vector3.up, transform.position).Raycast(ray, out var distance))
                {
                    _targetPosition = ray.GetPoint(distance);
                }
            }

            var newPointMovementDirection = _targetPosition - transform.position;
            newPointMovementDirection.y = 0f;
            if (Time.time - _lastRecalculatePathTime > 0.2f && newPointMovementDirection.magnitude > 0.41f)
            {
                PointMovementDirection = Vector2.zero;
                NavMeshAgent.enabled = true;
                if (NavMeshAgent.CalculatePath(_targetPosition, _navMeshPath))
                {
                    var totalCorners = _navMeshPath.GetCornersNonAlloc(_pathCorners);
                    if (totalCorners > 1)
                    {
                        PointMovementDirection = _pathCorners[1] - transform.position;
                        PointMovementDirection.y = 0f;
                        PointMovementDirection.Normalize();
                    }
                }
                NavMeshAgent.enabled = false;
            }
            else
            {
                PointMovementDirection = Vector2.zero;
            }

            var dashCooldownValue = (Time.time - _dashLastUseTime) / DashCooldown;
            DashCooldownRenderer.enabled = dashCooldownValue < 1f;
            DashCooldownRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(_shaderKeyValue, 1f - dashCooldownValue);
            DashCooldownRenderer.SetPropertyBlock(_propertyBlock);
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (!_lookMode)
                return;

            var value = context.ReadValue<Vector2>();
            CameraRotationEuler.x = Mathf.Clamp(CameraRotationEuler.x - value.y * CameraSensitivity.y, 15f, 80f);
            CameraRotationEuler.y += value.x * CameraSensitivity.x;
            if (CameraRotationEuler.y > 360f)
            {
                CameraRotationEuler.y -= 360f;
            }
            else if (CameraRotationEuler.y < 0f)
            {
                CameraRotationEuler.y += 360f;
            }
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            var value = context.ReadValue<Vector2>();
            if (context.canceled)
                DirectMovementDirection = Vector2.zero;
            else
                DirectMovementDirection = new Vector3(value.x, 0f, value.y);
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (EquipedWeapon == null)
                return;

            var ray = MainCamera.ScreenPointToRay(_pointOnScreen);
            TargetCharacter = null;
            PointMovementDirection = Vector2.zero;
            
            // ---
            var hitCount = Physics.RaycastNonAlloc(ray, _resultHits, 1000f);
            for (var i = 0; i < hitCount; i++)
            {
                var hitInfo = _resultHits[i];
                if (!hitInfo.collider.gameObject.Equals(gameObject) && hitInfo.collider.gameObject.TryGetComponent<CharacterController>(out var targetCharacterController))
                {
                    // Debug.Log($"Target character: {targetCharacterController.name}");
                    TargetCharacter = targetCharacterController;
                    _targetPosition = targetCharacterController.transform.position;
                    PointMovementDirection = _targetPosition - transform.position;
                    i = hitCount;
                }
            }
            // ---

            AttackMode = !context.canceled;

            if (AttackMode)
                AttackInProgress = true;
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (Time.time - _dashLastUseTime < DashCooldown)
                return;

            if (DirectMovementDirection.sqrMagnitude <= float.Epsilon)
            {
                var ray = MainCamera.ScreenPointToRay(_pointOnScreen);
                PointMovementDirection = Vector2.zero;
                
                if (PointMovementDirection.Equals(Vector2.zero) && new Plane(Vector3.up, transform.position).Raycast(ray, out var distance))
                {
                    _targetPosition = ray.GetPoint(distance);
                    PointMovementDirection = _targetPosition - transform.position;
                }

                if (PointMovementDirection.sqrMagnitude > float.Epsilon)
                {
                    PointMovementDirection.y = 0f;
                    PointMovementDirection.Normalize();
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(PointMovementDirection), 1f);
                }
            }

            _dashLastUseTime = Time.time;
            Animator.SetTrigger(_animationKeyDash);
            DashTrail.enabled = true;
            DashTrail.Clear();
            StartCoroutine(HideDashTrail());
        }

        public void OnLookMode(InputAction.CallbackContext context)
        {
            _lookMode = !context.canceled;
        }

        public void OnStand(InputAction.CallbackContext context)
        {
            InStand = !context.canceled;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _moving = !context.canceled;
            
            if (_moving && !AttackMode)
            {
                TargetCharacter = null;
                AttackInProgress = false;
            }
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
            _pointOnScreen = context.ReadValue<Vector2>();
        }

        public void OnZoom(InputAction.CallbackContext context)
        {
            var delta = context.ReadValue<Vector2>();
            MainCamera.transform.localPosition = new Vector3(0f, 0f, Mathf.Clamp(MainCamera.transform.localPosition.z + delta.y * ScrollSensitivity, -10f, -2f));
        }

        private void OnQuickSlot(InputAction.CallbackContext context)
        {
            if (int.TryParse(context.action.name.Substring(9), out var index))
            {
                index--;
                if (index > -1 && index < QuickSlots.Length)
                {
                    var slot = QuickSlots[index];
                    if (slot != null && slot.Item != null)
                    {
                        slot.Item.Use(this);
                    }
                }
            }
        }

        public void OnDamaged(Damage damage, CharacterController dealer)
        {
            LogDamage(_damageTakenLogText, _damageTakenLog, damage);

            var healthBefore = Health;

            damage.Apply(dealer, this);

            UpdateHealthIndicator();

            if (Health <= 0f)
            {
                if (healthBefore > 0f)
                {
                    Animator.SetTrigger(_animationKeyDeath);
                }
            }
            else
            {
                Animator.SetTrigger(_animationKeyDamaged);
            }
        }

        public void InvokeAttack()
        {
            if (EquipedWeapon != null && EquipedWeapon.AttackRange > float.Epsilon)
            {
                Animator.SetBool(_animationKeyShooting, true);
            }
            else
            {
                Animator.SetBool(_animationKeyAttack, true);
            }
        }

        public void AttackStart(int index)
        {
            var ray = new Ray();
            
            if (MainCamera != null)
            {
                ray = MainCamera.ScreenPointToRay(_pointOnScreen);
            }

            // TargetCharacter = null;
            PointMovementDirection = Vector2.zero;

            // ---
            // var hitCount = Physics.RaycastNonAlloc(ray, _resultHits, 1000f);
            // for (var i = 0; i < hitCount; i++)
            // {
            //     var hitInfo = _resultHits[i];
            //     if (!hitInfo.collider.gameObject.Equals(gameObject) && hitInfo.collider.gameObject.TryGetComponent<CharacterController>(out var targetCharacterController))
            //     {
            //         // Debug.Log($"Target character: {targetCharacterController.name}");
            //         TargetCharacter = targetCharacterController;
            //         _targetPosition = targetCharacterController.transform.position;
            //         PointMovementDirection = _targetPosition - transform.position;
            //         i = hitCount;
            //     }
            // }
            // ---

            if (TargetCharacter != null)
            {
                if (MainCamera != null)
                {
                    ray = new Ray(CameraPivot.transform.position, TargetCharacter.transform.position - CameraPivot.transform.position);
                }
                else
                {
                    ray = new Ray(TargetCharacter.transform.position + Vector3.up * 10f, -Vector3.up);
                }
            }
            
            // ---
            if (PointMovementDirection.Equals(Vector2.zero) && new Plane(Vector3.up, transform.position).Raycast(ray, out var distance))
            {
                _targetPosition = ray.GetPoint(distance);
                PointMovementDirection = _targetPosition - transform.position;
            }

            if (PointMovementDirection.sqrMagnitude > float.Epsilon)
            {
                PointMovementDirection.y = 0f;
                PointMovementDirection.Normalize();
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(PointMovementDirection), 1f);
            }
            // ---


            _inAttack = true;
            // Debug.Log($"Attack {index} start");
            EquipedWeapon?.ShowTrails();

            EquipedWeapon?.ApplyEffect(Effect.EffectActivationType.StartAttack, this, TargetCharacter);
        }

        public void AttackEnd(int index)
        {
            _inAttack = false;
            // Debug.Log($"Attack {index} end");
            EquipedWeapon?.HideTrails();

            if (!AttackMode)
            {
                TargetCharacter = null;
                AttackInProgress = false;
            }

            Animator.SetBool(_animationKeyAttack, false);
            Animator.SetBool(_animationKeyShooting, false);
        }

        public void DealDamage(int index)
        {
            if (TargetCharacter != null && (TargetCharacter.transform.position - transform.position).magnitude < EquipedWeapon.AttackRange + EquipedWeapon.AttackRadius)
            {
                var minCrit = (Agility * 0.1f + 0f) * 0.01f;
                var maxCrit = (Agility * 0.5f + 0f) * 0.01f;

                Crit = Crit + UnityEngine.Random.Range(minCrit, maxCrit);

                EquipedWeapon?.ApplyEffect(Effect.EffectActivationType.DamageDeal, this, TargetCharacter);

                var damage = new Damage();

                if (Crit >= 1f)
                {
                    damage.IsCritical = true;
                }

                damage.Prepare(this, TargetCharacter, EquipedWeapon.DamageSets);

                if (Crit >= 1f)
                {
                    if (EquipedWeapon != null)
                    {
                        EquipedWeapon.ApplyEffect(Effect.EffectActivationType.Critical, this, TargetCharacter);
                    }
                    Crit = 0f;
                }

                var healthBefore = TargetCharacter.Health;
                var targetIsLive = healthBefore > 0f;

                TargetCharacter.OnDamaged(damage, this);

                if (_critImageValue != null)
                {
                    _critImageValue.fillAmount = Crit;
                }


                if (targetIsLive)
                {
                    if (_exprerienceImageValue != null && TargetCharacter.Health <= 0f)
                    {
                        _exprerienceImageValue.fillAmount += 0.3f;
                    }

                    ClassResource += 0.03f;
                    if (_rageImageValue != null)
                    {
                        _rageImageValue.fillAmount = ClassResource;
                    }
                }
                
                // TODO: fix. not relevant value
                LogDamage(_damageDealtLogText, _damageDealtLog, damage);
            }

            MakeDamage(index);
        }

        public void RestoreHealth(float value)
        {
            Health = Mathf.Min(Health + value, MaxHealth);
            UpdateHealthIndicator();
        }

        private IEnumerator HideDashTrail()
        {
            yield return new WaitForSeconds(0.5f);
            DashTrail.enabled = false;
        }

        private void MakeDamage(int attackType)
        {
            var count = Physics.OverlapSphereNonAlloc(transform.position, EquipedWeapon.AttackRadius, _resultColliders);
            for (var i = 0; i < count; i++)
            {
                if (!_resultColliders[i].transform.Equals(transform) && _resultColliders[i].TryGetComponent<CharacterController>(out var target))
                {
                    var angle = Vector3.Angle(target.transform.position - transform.position, transform.forward);
                    if (angle < EquipedWeapon.AttackAngle / 2f)
                    {
                        Debug.Log($"Damage to {target.name}");
                    }
                }
            }
        }

        private void UpdateHealthIndicator()
        {
            HealthBarRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(_shaderKeyValue, Health / Mathf.Max(MaxHealth, 0.01f));
            HealthBarRenderer.SetPropertyBlock(_propertyBlock);
        }

        private void ShowStats()
        {
            if (_stats != null)
            {
                _stats.text = $"{ClassType}\n{Strenght}\n{Vitality}\n{Agility}\n{Intelligence}\n{ArmorClass}\n{Health:N0}/{MaxHealth:N0}\n{Crit * 100:N0}%\n\n\n{Resistance[(int)DamageTypeEnum.Venom]}\n{Resistance[(int)DamageTypeEnum.Nature]}\n{Resistance[(int)DamageTypeEnum.Fire]}\n{Resistance[(int)DamageTypeEnum.Lightning]}\n{Resistance[(int)DamageTypeEnum.Frost]}\n{Resistance[(int)DamageTypeEnum.Dark]}\n{Resistance[(int)DamageTypeEnum.Light]}\n";
            }
        }

        private void ShowEnemyStats()
        {
            if (_enemyStats != null)
            {
                if (TargetCharacter != null)
                {
                    _enemyStats.transform.parent.gameObject.SetActive(true);
                    _enemyStats.text = $"{TargetCharacter.ClassType}\n{TargetCharacter.ArmorClass}\n{TargetCharacter.Health:N0}/{TargetCharacter.MaxHealth:N0}\n\n\n{TargetCharacter.Resistance[(int)DamageTypeEnum.Venom]}\n{TargetCharacter.Resistance[(int)DamageTypeEnum.Nature]}\n{TargetCharacter.Resistance[(int)DamageTypeEnum.Fire]}\n{TargetCharacter.Resistance[(int)DamageTypeEnum.Lightning]}\n{TargetCharacter.Resistance[(int)DamageTypeEnum.Frost]}\n{TargetCharacter.Resistance[(int)DamageTypeEnum.Dark]}\n{TargetCharacter.Resistance[(int)DamageTypeEnum.Light]}\n";
                }
                else
                {
                    _enemyStats.transform.parent.gameObject.SetActive(false);
                }
            }
        }

        private void LogDamage(TMP_Text text, LinkedList<string> log, Damage damage)
        {
            if (text != null)
            {
                var damageName = "";
                for (var i = 0; i < damage.Value.Length; i++)
                {
                    var value = damage.Value[i];
                    if (value > 0f)
                    {
                        var damageType = (DamageTypeEnum) i;
                        
                        if (damageType != DamageTypeEnum.Physic)
                        {
                            damageName = damageType.ToString().ToLower();
                            switch (damageType)
                            {
                                case DamageTypeEnum.Fire:
                                    damageName = $"<color=#fc5>{damageName}</color>";
                                break;
                                case DamageTypeEnum.Dark:
                                    damageName = $"<color=#777>{damageName}</color>";
                                break;
                                case DamageTypeEnum.Frost:
                                    damageName = $"<color=#99f>{damageName}</color>";
                                break;
                                case DamageTypeEnum.Light:
                                    damageName = $"<color=#ff9>{damageName}</color>";
                                break;
                                case DamageTypeEnum.Lightning:
                                    damageName = $"<color=#ff5>{damageName}</color>";
                                break;
                                case DamageTypeEnum.Nature:
                                    damageName = $"<color=#995>{damageName}</color>";
                                break;
                                case DamageTypeEnum.Venom:
                                    damageName = $"<color=#5f5>{damageName}</color>";
                                break;
                            }
                        }

                        log.AddLast($"{value:N0} {damageName} damage");
                        while(log.Count > 8)
                        {
                            log.RemoveFirst();
                        }
                        var s = "";
                        foreach (var v in log)
                        {
                            s += $"{v}\n";
                        }
                        text.text = s;
                    }
                }
            }
        }
    }
}