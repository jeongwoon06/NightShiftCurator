using System.Collections.Generic;
using UnityEngine;
using NightShiftCurator.Loot;

namespace NightShiftCurator.Cart
{
    // 카트는 "유물을 손에 든 것"과 같은 원리로 동작한다.
    // 다른 점은 딱 하나: 위치/회전을 그 자리에서 바로 따라가지 않고,
    // 초당 이동/회전 가능한 최대량(maxFollowSpeed, maxTurnRate)만큼만 서서히 따라간다.
    // = "무거워서 느리게 끌려오는" 느낌.
    [RequireComponent(typeof(Rigidbody))]
    public class CartController : MonoBehaviour
    {
        [Header("따라오기 (E로 잡으면 플레이어 앞쪽을 계속 목표 지점으로 삼음)")]
        public float followDistance = 1.3f;   // 플레이어 앞쪽 이 거리 지점이 목표
        public float maxFollowSpeed = 1.5f;   // 초당 최대 이동 거리(m/s) - 낮출수록 느리게 끌림
        public float maxTurnRate = 40f;       // 초당 최대 회전 각도(도/초) - 낮출수록 천천히 돎

        public float pushRange = 1.6f; // E로 잡을 수 있는 최대 거리 (PlayerInteraction에서 사용)

        [Header("Capacity")]
        public int maxSlots = 8;
        public Transform[] loadSlots;

        public bool IsGrabbed => _followTarget != null;
        public int LoadedCount => _loaded.Count;
        public int TotalValue
        {
            get { int sum = 0; foreach (var item in _loaded) sum += item.value; return sum; }
        }

        private Rigidbody _rb;
        private readonly List<LootItem> _loaded = new List<LootItem>();
        private Transform _followTarget; // 카트를 잡고 있는 플레이어. null이면 안 잡힌 상태
        private Vector3 _lastPosition;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.centerOfMass = new Vector3(0f, -0.3f, 0f);
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _lastPosition = _rb.position;
        }

        void FixedUpdate()
        {
            if (_followTarget != null)
            {
                // 목표 지점: 플레이어 앞쪽 followDistance 만큼
                Vector3 targetPos = _followTarget.position + _followTarget.forward * followDistance;
                targetPos.y = _rb.position.y; // 높이는 카트 자기 높이 유지 (플레이어 눈높이 영향 안 받음)

                // 초당 최대 이동거리만큼만 다가감 - 이게 "느리게 끌려오는" 느낌의 핵심
                Vector3 newPos = Vector3.MoveTowards(_rb.position, targetPos, maxFollowSpeed * Time.fixedDeltaTime);
                _rb.MovePosition(newPos);

                // 회전도 초당 최대 각도만큼만 따라감 (순간 스냅 방지)
                Quaternion targetRot = Quaternion.Euler(0f, _followTarget.eulerAngles.y, 0f);
                Quaternion newRot = Quaternion.RotateTowards(_rb.rotation, targetRot, maxTurnRate * Time.fixedDeltaTime);
                _rb.MoveRotation(newRot);
            }

            // 급격한 위치 변화(벽 충돌 등) 시 적재물 파손 판정
            float movedDist = Vector3.Distance(_rb.position, _lastPosition);
            float impliedSpeed = movedDist / Mathf.Max(Time.fixedDeltaTime, 0.0001f);
            if (impliedSpeed > maxFollowSpeed * 2.5f) // 물리 충돌 등으로 비정상적으로 튀었을 때만
                foreach (var item in _loaded) item.ApplyImpact(impliedSpeed * 0.3f);
            _lastPosition = _rb.position;
        }

        // PlayerInteraction이 E를 누르면 호출
        public void Grab(Transform player)
        {
            _followTarget = player;
        }

        public void Release()
        {
            _followTarget = null;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        public bool TryLoadItem(LootItem item)
        {
            if (_loaded.Count >= maxSlots) return false;

            Transform slot = (loadSlots != null && _loaded.Count < loadSlots.Length)
                ? loadSlots[_loaded.Count]
                : transform;

            item.PlaceInCart(slot);
            _loaded.Add(item);
            return true;
        }

        public float CurrentNoiseLevel01()
        {
            return _followTarget != null ? 1f : 0f;
        }
    }
}
