using UnityEngine;

namespace NightShiftCurator.Loot
{
    [RequireComponent(typeof(Collider))]
    public class LootItem : MonoBehaviour
    {
        public string itemName = "이름없는 유물";
        public int value = 100;
        [Range(0f, 1f)] public float condition = 1f; // 보존 상태, 파손 시 감소

        public bool IsHeld { get; private set; }
        public bool IsInCart { get; private set; }

        private Rigidbody _rb;
        private Collider _col;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _col = GetComponent<Collider>();
        }

        public void SetHeld(bool held, Transform holdPoint)
        {
            IsHeld = held;
            IsInCart = false;

            if (held)
            {
                if (_rb != null)
                {
                    _rb.isKinematic = true;
                    _rb.linearVelocity = Vector3.zero;
                    _rb.angularVelocity = Vector3.zero;
                    _rb.detectCollisions = false; // 손에 든 동안은 물리 충돌 계산 안 함(다른 오브젝트와 겹쳐도 안전)
                }
                if (_col != null) _col.enabled = false;

                transform.SetParent(holdPoint);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                transform.SetParent(null);
                if (_col != null) _col.enabled = true;
                if (_rb != null)
                {
                    _rb.detectCollisions = true;
                    _rb.isKinematic = false;
                }
            }
        }

        public void PlaceInCart(Transform slot)
        {
            IsHeld = false;
            IsInCart = true;

            transform.SetParent(slot);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            if (_rb != null)
            {
                _rb.isKinematic = true;
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                // 카트 본체 콜라이더와 겹쳐서 물리엔진이 튕겨내는(폭발) 현상 방지 -
                // 카트 안에서는 순수 장식용으로 취급, 충돌 계산 자체를 끈다
                _rb.detectCollisions = false;
            }
            if (_col != null) _col.enabled = false;
        }

        // 충격량에 따른 파손 (카트 급정거/충돌 시 CartController가 호출)
        public void ApplyImpact(float impactForce)
        {
            if (impactForce < 3f) return;
            condition -= impactForce * 0.02f;
            condition = Mathf.Clamp01(condition);
        }
    }
}
