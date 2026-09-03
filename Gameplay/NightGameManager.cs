using UnityEngine;

namespace NightShiftCurator.Gameplay
{
    public class NightGameManager : MonoBehaviour
    {
        public static NightGameManager Instance { get; private set; }

        [Header("일출 타이머 (초)")]
        public float nightDurationSeconds = 300f; // 5분
        public float TimeRemaining { get; private set; }

        public bool NightEnded { get; private set; }

        void Awake()
        {
            Instance = this;
            TimeRemaining = nightDurationSeconds;
        }

        void Update()
        {
            if (NightEnded) return;

            TimeRemaining -= Time.deltaTime;
            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                ForceSunrise();
            }
        }

        void ForceSunrise()
        {
            NightEnded = true;
            Debug.Log("[일출] 시간 종료 - 강제 체포 처리");
        }

        public void OnExtractionSuccess()
        {
            NightEnded = true;
            Debug.Log("[결과] 탈출 성공");
        }

        // 0(자정 느낌) ~ 1(일출 직전) - UI 게이지에 바인딩
        public float SunriseProgress01()
        {
            return 1f - Mathf.Clamp01(TimeRemaining / nightDurationSeconds);
        }
    }
}
