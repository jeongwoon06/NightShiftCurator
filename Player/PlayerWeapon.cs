using UnityEngine;
using NightShiftCurator.Combat;

namespace NightShiftCurator.Player
{
    public class PlayerWeapon : MonoBehaviour
    {
        public Camera eye;
        public float range = 30f;
        public float damage = 20f;
        public float fireCooldown = 0.25f;
        public LayerMask hitMask = ~0;
        public ParticleSystem muzzleFlash;

        private float _cooldownTimer;
        private bool _combatActive; // 경비원에게 발각되어 총싸움이 시작되면 true

        public void SetCombatActive(bool active) => _combatActive = active;

        void Update()
        {
            _cooldownTimer -= Time.deltaTime;

            if (!_combatActive) return; // 발각 전에는 총 발사 불가 (잠입 컨셉 유지)

            if (Input.GetButtonDown("Fire1") && _cooldownTimer <= 0f)
            {
                Fire();
                _cooldownTimer = fireCooldown;
            }
        }

        void Fire()
        {
            if (muzzleFlash != null) muzzleFlash.Play();

            if (Physics.Raycast(eye.transform.position, eye.transform.forward, out RaycastHit hit, range, hitMask))
            {
                var health = hit.collider.GetComponentInParent<Health>();
                if (health != null) health.TakeDamage(damage);
            }
        }
    }
}
