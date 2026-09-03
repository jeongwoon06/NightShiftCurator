# NightShiftCurator

Player 폴더

PlayerController.cs — 붙이는 곳: Player 루트 오브젝트
1인칭 이동/시야 처리. WASD로 걷기, 마우스로 시야 회전, 점프. 걷는 속도에 따라 currentNoiseLevel(소음값)을 계산해서 경비원 AI가 이걸 읽고 플레이어를 청각으로 감지하게 함. CharacterController 컴포넌트가 반드시 같이 있어야 함.

PlayerInteraction.cs — 붙이는 곳: Player 루트 오브젝트
E키 상호작용 전담. 레이캐스트로 뭘 보고 있는지 판단해서 유물 줍기, 카트 잡기/놓기를 처리함. eye(카메라)랑 holdPoint(든 물건 위치) 참조가 인스펙터에 연결돼 있어야 동작함.

PlayerWeapon.cs — 붙이는 곳: Player 루트 오브젝트
발각돼서 총싸움이 시작된 후에만 좌클릭으로 사격 가능한 레이캐스트 총. 평소엔 SetCombatActive(false) 상태라 총을 못 씀 — GuardAI가 전투 상태에 들어가면 이 함수를 호출해서 활성화시킴.

Combat 폴더

Health.cs — 붙이는 곳: Player와 Guard 둘 다
공용 체력 시스템. 데미지 받고 죽는 로직만 담당. PlayerWeapon이랑 GuardWeapon이 상대방의 이 컴포넌트를 찾아서 TakeDamage()를 호출하는 방식으로 연결됨.

AI 폴더

GuardWeapon.cs — 붙이는 곳: Guard 오브젝트
경비원의 사격 로직. GuardAI가 전투 상태일 때 이걸 호출해서 플레이어에게 레이캐스트 사격.

GuardAI.cs — 붙이는 곳: Guard 오브젝트 (NavMeshAgent 필수)
경비원의 두뇌. 순찰 → 의심 → 추격 → 전투 4단계 상태머신. 시야(각도+거리+레이캐스트)와 청각(플레이어 소음값)으로 플레이어를 감지하고, 전투 상태가 되면 PlayerWeapon.SetCombatActive(true)를 호출해서 총싸움을 트리거함.

Loot 폴더

LootItem.cs — 붙이는 곳: 유물(훔칠 오브젝트) 각각
"들 수 있는 물건" 하나의 상태. 손에 든 상태(SetHeld)와 카트에 들어간 상태(PlaceInCart)를 구분해서 관리하고, 카트에 들어가는 순간 자기 콜라이더/물리를 꺼서 카트랑 부딪혀 튕겨나가는 걸 방지함.

Cart 폴더

CartController.cs — 붙이는 곳: Cart 루트 오브젝트 (Rigidbody 필수)
카트 자체의 움직임과 적재 관리. Grab()으로 잡히면 플레이어를 목표점 삼아 느리게 따라가고, TryLoadItem()으로 유물을 슬롯에 저장함.

Gameplay 폴더

ExtractionZone.cs — 붙이는 곳: 탈출 지점 트리거 오브젝트
카트가 이 영역(트리거 콜라이더)에 들어오면 탈출 성공 처리.

NightGameManager.cs — 붙이는 곳: 씬에 딱 하나, 빈 오브젝트
일출 타이머(제한시간) 관리. 시간이 다 되면 강제 체포 처리하는 게임 흐름 담당.

Network 폴더

NetworkBootstrap.cs — 붙이는 곳: 씬에 딱 하나, 빈 오브젝트 (지금은 미사용 상태)
Fusion NetworkRunner를 띄워서 호스트를 시작하는 코드. 지금은 싱글플레이 스캐폴드라 이게 없어도 게임이 돌아가고, 나중에 실제 멀티플레이 붙일 때 사용하는 골조.

Editor 폴더

NightSceneBuilder.cs — 붙이는 곳: 에디터 전용, 씬에 붙이는 게 아님
Assets/Editor 폴더에 있어야만 동작하는 특수 스크립트. Tools > Night Shift Curator > Build Night Scene을 누르면 위 스크립트들을 자동으로 프리팹화하고 씬에 배치 + 연결까지 다 해주는 "빌더" 역할. 사람이 직접 뭘 붙일 필요 없게 만들어주는 스크립트예요.
