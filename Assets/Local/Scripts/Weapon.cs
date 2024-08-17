using System.Collections;
using UnityEngine;

namespace Scripts
{
    public class Weapon : MonoBehaviour
    {
        [Header("Stats:")]
        public float BaseDamage = 20f;
        public float CritMultiplier = 2f;
        public float AttackRange = 0f;
        public float AttackRadius = 1.6f;
        public float AttackAngle = 120f;

        [Header("Links:")]
        public GameObject[] WeaponVisual = new GameObject[0];
        public TrailRenderer[] TrailRenderers;
        public Transform RightHandVisual;
        public Transform LeftHandVisual;

        [Header("Runtime:")]
        private float[] _trailRendererWidths;
        private Coroutine _hideSwordTrailCoroutine;

        public void Awake()
        {
            _trailRendererWidths = new float[TrailRenderers.Length];
            for (var i = 0; i < _trailRendererWidths.Length; i++)
            {
                TrailRenderers[i].enabled = false;
                _trailRendererWidths[i] = TrailRenderers[i].widthMultiplier;
            }
        }

        public float GetDamage(CharacterController selfCharacter, CharacterController targetCharacter)
        {
            var damage = BaseDamage + selfCharacter.Strenght * 0.5f;

            damage *= (2000 + targetCharacter.ArmorClass) / (2000 + targetCharacter.ArmorClass * 10);

            return damage;
        }

        public Weapon Equip(CharacterController character)
        {
            var instance = Instantiate(gameObject).GetComponent<Weapon>();
            
            character.EquipedWeapon = instance;
            
            if (instance.RightHandVisual != null)
            {
                instance.RightHandVisual.transform.parent = character.RightHandPivot.transform;
                instance.RightHandVisual.transform.localPosition = Vector3.zero;
                instance.RightHandVisual.transform.localRotation = Quaternion.identity;
            }

            if (instance.LeftHandVisual != null)
            {
                instance.LeftHandVisual.transform.parent = character.LeftHandPivot.transform;
                instance.LeftHandVisual.transform.localPosition = Vector3.zero;
                instance.LeftHandVisual.transform.localRotation = Quaternion.identity;
            }

            return instance;
        }

        public void HideTrails()
        {
            foreach (var trail in TrailRenderers)
            {
                trail.enabled = true;
            }

            if (_hideSwordTrailCoroutine != null)
            {
                StopCoroutine(_hideSwordTrailCoroutine);
            }

            _hideSwordTrailCoroutine = StartCoroutine(HideTrailsAsync());
        }

        public void ShowTrails()
        {
            for (var i = 0; i < TrailRenderers.Length; i++)
            {
                var trail = TrailRenderers[i];
                _trailRendererWidths[i] = TrailRenderers[i].widthMultiplier;
                trail.enabled = true;
                trail.widthMultiplier = _trailRendererWidths[i];
                trail.Clear();
            }

            if (_hideSwordTrailCoroutine != null)
            {
                StopCoroutine(_hideSwordTrailCoroutine);
                _hideSwordTrailCoroutine = null;
            }
        }

        private IEnumerator HideTrailsAsync()
        {
            var t = Time.time;
            var d = 0f;
            while (d < 1f)
            {
                yield return new WaitForEndOfFrame();
                d = (Time.time - t) / 0.2f;
                for (var i = 0; i < TrailRenderers.Length; i++)
                {
                    TrailRenderers[i].widthMultiplier = (1f - d) * _trailRendererWidths[i];
                }
            }
            for (var i = 0; i < TrailRenderers.Length; i++)
            {
                TrailRenderers[i].enabled = false;
                TrailRenderers[i].widthMultiplier = _trailRendererWidths[i];
            }
            _hideSwordTrailCoroutine = null;
        }
    }
}