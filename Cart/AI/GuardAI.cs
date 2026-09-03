using UnityEngine;
using UnityEngine.AI;
using NightShiftCurator.Player;

namespace NightShiftCurator.AI
{
    public enum GuardState { Patrol, Investigate, Chase, Combat }

    [RequireComponent(typeof(NavMeshAgent))]
    public class GuardAI : MonoBehaviour
    {
        [Header("Patrol")]
        public Transform[] patrolPoints;
        public float waitAtPoint = 2f;

        [Header("Vision")]
        public float viewDistance = 12f;
        public float viewAngle = 70f;
        public Transform eyes;
        public LayerMask visionBlockMask;

        [Header("Hearing")]
        public float hearingRadius = 8f; // 소음 최대치(플레이어 sprint)일 때 들리는 거리

        [Header("Alert")]
        public float suspicionBuildRate = 1f;
        public float suspicionDecayRate = 0.4f;
        public float investigateTime = 4f;

        public GuardState State { get; private set; } = GuardState.Patrol;

        private NavMeshAgent _agent;
        private GuardWeapon _weapon;
        private int _patrolIndex;
        private float _waitTimer;
        private float _suspicion; // 0~1
        private Transform _target;
        private Vector3 _lastKnownPos;
        private float _investigateTimer;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _weapon = GetComponent<GuardWeapon>();
            if (eyes == null) eyes = transform;
        }

        void Start()
        {
            if (!_agent.isOnNavMesh)
            {
                if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out var hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    _agent.Warp(hit.position);
                }
                else
                {
                    Debug.LogWarning($"[GuardAI] {name}: 근처에 NavMesh가 없습니다. NavMesh를 베이크했는지 확인하세요.");
                }
            }

            if (patrolPoints != null && patrolPoints.Length > 0)
                GoToPatrolPoint();
        }

        void Update()
        {
            var player = FindVisiblePlayer();

            if (player != null)
            {
                _suspicion += suspicionBuildRate * Time.deltaTime;
                _target = player;
                _lastKnownPos = player.position;
            }
            else
            {
                _suspicion -= suspicionDecayRate * Time.deltaTime;
            }
            _suspicion = Mathf.Clamp01(_suspicion);

            switch (State)
            {
                case GuardState.Patrol:
                    UpdatePatrol();
                    if (_suspicion > 0.5f) ChangeState(GuardState.Investigate);
                    break;

                case GuardState.Investigate:
                    UpdateInvestigate();
                    if (_suspicion >= 1f) ChangeState(GuardState.Combat);
                    if (_suspicion <= 0f) ChangeState(GuardState.Patrol);
                    break;

                case GuardState.Chase:
                    UpdateChase();
                    break;

                case GuardState.Combat:
                    UpdateCombat();
                    if (_suspicion <= 0f) ChangeState(GuardState.Patrol);
                    break;
            }
        }

        void ChangeState(GuardState newState)
        {
            State = newState;
            if (newState == GuardState.Investigate)
            {
                _investigateTimer = investigateTime;
                if (_agent.isOnNavMesh)
                    _agent.SetDestination(_lastKnownPos);
            }
        }

        void UpdatePatrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;
            if (!_agent.isOnNavMesh) return; // NavMesh 밖이면 이동 판정 스킵

            if (!_agent.pathPending && _agent.remainingDistance < 0.3f)
            {
                _waitTimer += Time.deltaTime;
                if (_waitTimer >= waitAtPoint)
                {
                    _waitTimer = 0f;
                    _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
                    GoToPatrolPoint();
                }
            }
        }

        void GoToPatrolPoint()
        {
            if (_agent.isOnNavMesh)
                _agent.SetDestination(patrolPoints[_patrolIndex].position);
        }

        void UpdateInvestigate()
        {
            _investigateTimer -= Time.deltaTime;
            if (_target != null && _agent.isOnNavMesh) _agent.SetDestination(_target.position);

            if (_investigateTimer <= 0f && _suspicion < 1f)
            {
                _suspicion = 0f;
                ChangeState(GuardState.Patrol);
            }
        }

        void UpdateChase()
        {
            if (_target == null) { ChangeState(GuardState.Patrol); return; }
            if (_agent.isOnNavMesh) _agent.SetDestination(_target.position);
        }

        void UpdateCombat()
        {
            if (_target == null) { ChangeState(GuardState.Patrol); return; }

            float dist = Vector3.Distance(transform.position, _target.position);
            if (dist > _weapon.range * 1.2f)
            {
                if (_agent.isOnNavMesh) _agent.SetDestination(_target.position);
            }
            else
            {
                if (_agent.isOnNavMesh) _agent.SetDestination(transform.position); // 멈추고 사격
                transform.LookAt(new Vector3(_target.position.x, transform.position.y, _target.position.z));
                _weapon.TryFireAt(_target);
            }

            // 발각된 플레이어 쪽에 총싸움 시작 알림
            var pw = _target.GetComponentInParent<PlayerWeapon>();
            if (pw != null) pw.SetCombatActive(true);
        }

        Transform FindVisiblePlayer()
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var p in players)
            {
                Vector3 dirToPlayer = p.transform.position - eyes.position;
                float dist = dirToPlayer.magnitude;

                // 소음 체크 (거리 안이면 무조건 인지)
                var pc = p.GetComponent<PlayerController>();
                float noise = pc != null ? pc.currentNoiseLevel : 0f;
                if (dist <= hearingRadius * noise)
                    return p.transform;

                if (dist > viewDistance) continue;

                float angle = Vector3.Angle(eyes.forward, dirToPlayer);
                if (angle > viewAngle * 0.5f) continue;

                if (Physics.Raycast(eyes.position, dirToPlayer.normalized, out RaycastHit hit, dist, visionBlockMask))
                {
                    if (!hit.collider.CompareTag("Player")) continue; // 시야 차단됨
                }

                return p.transform;
            }
            return null;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, viewDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, hearingRadius);
        }
    }
}
