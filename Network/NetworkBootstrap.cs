using Fusion;
using System.Threading.Tasks;
using UnityEngine;

namespace NightShiftCurator.Network
{
    // 주의: Assets > Fusion SDK가 이미 임포트되어 있어야 컴파일됩니다.
    // 지금 단계의 다른 스크립트(Player/Cart/Guard 등)는 전부 순수 MonoBehaviour라
    // Fusion 없이도 싱글플레이로 바로 테스트 가능합니다.
    // 이 스크립트는 나중에 NetworkRunner를 실제로 붙일 때 사용하세요.
    public class NetworkBootstrap : MonoBehaviour
    {
        [SerializeField] private NetworkRunner runnerPrefab;
        [SerializeField] private string sessionName = "MuseumHeistRoom";
        [SerializeField] private int maxPlayers = 4;

        private NetworkRunner _runner;

        async void Start()
        {
            if (runnerPrefab == null)
            {
                Debug.LogWarning("[NetworkBootstrap] runnerPrefab이 비어있어 네트워크 시작을 건너뜁니다. 싱글플레이로 진행하세요.");
                return;
            }
            await StartHost();
        }

        async Task StartHost()
        {
            _runner = Instantiate(runnerPrefab);
            _runner.name = "NetworkRunner";

            var sceneManager = _runner.GetComponent<NetworkSceneManagerDefault>();
            if (sceneManager == null)
                sceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

            var startArgs = new StartGameArgs()
            {
                GameMode = GameMode.Host,
                SessionName = sessionName,
                PlayerCount = maxPlayers,
                SceneManager = sceneManager
            };

            var result = await _runner.StartGame(startArgs);

            if (result.Ok)
                Debug.Log("[NetworkBootstrap] 호스트 시작 성공");
            else
                Debug.LogError($"[NetworkBootstrap] 시작 실패: {result.ShutdownReason}");
        }
    }
}
