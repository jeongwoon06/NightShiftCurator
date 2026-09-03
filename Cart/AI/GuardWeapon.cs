using UnityEngine;
using NightShiftCurator.Combat;

namespace NightShiftCurator.AI
{
    public class GuardWeapon : MonoBehaviour
    {
        public Transform muzzle;
        public float range = 20f;
        public float damage = 12f;
        public float fireCooldown = 1.1f;
        [Range(0f, 15f)] public float accuracySpreadDeg = 4f;
        public LayerMask hitMask = ~0;

        private float _timer;

        public void TryFireAt(Transform target)
        {
            _timer -= Time.deltaTime;
            if (_timer > 0f || target == null) return;

            Vector3 dir = (target.position - muzzle.position).normalized;
            dir = Quaternion.Euler(
                Random.Range(-accuracySpreadDeg, accuracySpreadDeg),
                Random.Range(-accuracySpreadDeg, accuracySpreadDeg),
                0f) * dir;

            if (Physics.Raycast(muzzle.position, dir, out RaycastHit hit, range, hitMask))
            {
                var health = hit.collider.GetComponentInParent<Health>();
                if (health != null) health.TakeDamage(damage);
            }

            _timer = fireCooldown;
        }
    }
}
