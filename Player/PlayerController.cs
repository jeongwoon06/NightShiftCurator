using UnityEngine;

namespace NightShiftCurator.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Move")]
        public float walkSpeed = 4.5f;
        public float sprintSpeed = 7f;
        public float gravity = -20f;
        public float jumpHeight = 1.1f;

        [Header("Look")]
        public Camera playerCamera;
        public float mouseSensitivity = 2.2f;
        public float minPitch = -80f;
        public float maxPitch = 80f;

        [Header("Noise (guard detection uses this)")]
        [Tooltip("0~1, 걷기/뛰기에 따라 자동으로 채워짐. 경비원 AI가 이 값을 읽어감")]
        public float currentNoiseLevel;

        [HideInInspector]
        [Tooltip("카트를 미는 동안 PlayerInteraction이 이 값을 줄여서 이동속도에 곱함")]
        public float speedMultiplier = 1f;

        private CharacterController _cc;
        private Vector3 _velocity;
        private float _pitch;

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
            Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            HandleLook();
            HandleMove();
        }

        void HandleLook()
        {
            float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
            float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mx);

            _pitch -= my;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
            if (playerCamera != null)
                playerCamera.transform.localEulerAngles = new Vector3(_pitch, 0f, 0f);
        }

        void HandleMove()
        {
            bool grounded = _cc.isGrounded;
            if (grounded && _velocity.y < 0) _velocity.y = -2f;

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            bool sprint = Input.GetKey(KeyCode.LeftShift) && v > 0.1f;

            float speed = (sprint ? sprintSpeed : walkSpeed) * speedMultiplier;
            Vector3 move = (transform.right * h + transform.forward * v);
            if (move.sqrMagnitude > 1f) move.Normalize();

            _cc.Move(move * speed * Time.deltaTime);

            // 경비원 감지용 소음 레벨 (걷기/뛰기/정지)
            float targetNoise = move.sqrMagnitude < 0.01f ? 0f : (sprint ? 1f : 0.45f);
            currentNoiseLevel = Mathf.Lerp(currentNoiseLevel, targetNoise, Time.deltaTime * 8f);

            if (Input.GetButtonDown("Jump") && grounded)
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            _velocity.y += gravity * Time.deltaTime;
            _cc.Move(_velocity * Time.deltaTime);
        }
    }
}
