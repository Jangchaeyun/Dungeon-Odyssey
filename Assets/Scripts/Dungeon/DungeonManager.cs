using System.Collections;
using System.Collections.Generic;
using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Enemy;
using DungeonOdyssey.Player;
using DungeonOdyssey.UI;
using DungeonOdyssey.View3D;
using UnityEngine;

namespace DungeonOdyssey.Dungeon
{
    public class DungeonManager : MonoBehaviour
    {
        public static DungeonManager Instance { get; private set; }

        [SerializeField] private DungeonGenerator generator;
        [SerializeField] private Transform worldRoot;

        private readonly List<MonsterBrain> _roomMonsters = new();
        private readonly HashSet<int> _visited = new();
        private DungeonRoomData _current;
        private Transform _roomRoot;
        private Transform _passage;
        private Transform _player;
        private PlayerStats _playerStats;
        private ClearBannerUI _clearBanner;
        private HudUI _hud;
        private MinimapUI _minimap;
        private WeaponHotbarUI _weaponHotbar;
        private BagUI _bag;
        private CameraRig3D _cameraRig;
        private bool _dungeonCleared;
        private bool _returning;
        private bool _transitioning;
        private float _hintCooldown;
        private int _difficulty;

        public bool IsCleared => _dungeonCleared;
        public DungeonRoomData CurrentRoom => _current;

        private void Awake()
        {
            Instance = this;
            if (generator == null)
            {
                generator = GetComponent<DungeonGenerator>() ?? gameObject.AddComponent<DungeonGenerator>();
            }

            if (worldRoot == null)
            {
                worldRoot = new GameObject("DungeonWorld").transform;
            }
        }

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                new GameObject("GameManager").AddComponent<GameManager>().EnsureSaveData();
            }
            else
            {
                GameManager.Instance.EnsureSaveData();
            }

            // 이전 버전에서 부모 없이 남은 라벨 잔상 제거
            foreach (var orphan in FindObjectsByType<WorldLabel>(FindObjectsSortMode.None))
            {
                if (orphan != null && orphan.transform.parent == null)
                {
                    Destroy(orphan.gameObject);
                }
            }

            CombatAudio.PlayDungeonAmbience();
            QuestCatalog.BeginDungeonRun();
            StyleCombo.Reset();
            ScreenFade.SetTheme(true);
            var save = GameManager.Instance.CurrentSave;
            var threat = DifficultyScaler.BeginRun(save);
            SessionStats.BeginRun(threat.Label);
            _difficulty = threat.Tier;
            var ui = RuntimeUiFactory.BuildGameplayHud(includeClearBanner: true, includeMinimap: true);
            _hud = ui.hud;
            _clearBanner = ui.clear;
            _minimap = ui.minimap;
            _weaponHotbar = ui.weaponHotbar;
            _bag = ui.bag;
            _hud?.SetThreatChip($"{threat.Label}  T{threat.Tier}");

            var rooms = DifficultyScaler.SuggestedRoomCount(save.level, save.dungeonClears);
            generator.Generate(rooms);
            _current = generator.StartRoom;
            _visited.Add(_current.Id);
            _minimap?.Setup(generator, _visited);

            var cinematic = GameManager.Instance != null && GameManager.Instance.PendingDungeonEntryCinematic;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PendingDungeonEntryCinematic = false;
            }

            // 마을에서 동쪽으로 들어옴 → 시작방 서쪽 안쪽에서 동쪽을 바라보며 등장
            var entry = cinematic ? EntrySpawn(DoorDir.East) : Vector3.zero;
            var intoRoom = FaceIntoRoom(DoorDir.East);

            if (cinematic)
            {
                ScreenFade.SetTheme(true);
                ScreenFade.Set(1f);
            }

            var playerGo = SpawnPlayer(entry);
            _player = playerGo.transform;
            _playerStats = playerGo.GetComponent<PlayerStats>();
            var anim = playerGo.GetComponent<HumanoidAnimator>();
            // 입장 연출 중엔 마을 자세 → 연출 끝에서 전투 / 즉시 입장은 바로 전투 자세
            if (cinematic)
            {
                anim?.SetCombatStance(false);
            }
            else
            {
                anim?.SetCombatStance(true);
            }
            _hud?.Bind(_playerStats);
            _hud?.RefreshQuest();
            _weaponHotbar?.Bind(_playerStats);
            _bag?.Bind(_playerStats);
            if (!cinematic)
            {
                _clearBanner?.Show($"{threat.Label}  ·  위협도 {threat.Tier}  ·  Lv {save.level}",
                    ToastKind.Threat, 2.6f);
            }

            if (QuestCatalog.LuckActiveThisRun)
            {
                _hud?.SetHint("행운부적 발동 · 이번 탐험 골드 증가");
            }

            LoadRoom(_current, entry, snapCamera: !cinematic);
            if (cinematic)
            {
                OpenDoorPassage(DoorDir.West);
                StartCoroutine(DungeonEntryArrivalRoutine(entry, intoRoom, threat, save.level));
            }
        }

        private IEnumerator DungeonEntryArrivalRoutine(Vector3 entry, Vector3 intoRoom, RunThreat threat, int level)
        {
            _transitioning = true;
            GameUi.IsBlocking = true;

            var controller = _player != null ? _player.GetComponent<PlayerController>() : null;
            controller?.SetControlEnabled(false);
            TeleportPlayer(entry);
            controller?.ForceFacing(intoRoom);
            controller?.SnapPosition(entry);
            _cameraRig?.SetFacing(intoRoom);

            var inside = entry + intoRoom * 2.6f;
            controller?.BeginScriptedWalkAlong(intoRoom, inside, 3.6f);
            controller?.LockFacing(intoRoom, 1.4f);

            var fadeIn = StartCoroutine(ScreenFade.In(0.42f));
            yield return WaitScriptedArrive(controller, 0.4f, 1.15f);
            if (fadeIn != null)
            {
                yield return fadeIn;
            }

            controller?.EndScriptedWalk();
            if (_player != null && !IsInsideRoom(_player.position))
            {
                TeleportPlayer(entry + intoRoom * 1.6f);
            }

            _cameraRig?.SoftSnap(0.4f);
            controller?.LockFacing(intoRoom, 0.7f);
            // 입장 후 칼을 전투 대기 위치로
            _player?.GetComponent<HumanoidAnimator>()?.SetCombatStance(true);
            yield return new WaitForSeconds(0.35f);
            controller?.SetControlEnabled(true);
            _clearBanner?.Show($"{threat.Label}  ·  위협도 {threat.Tier}  ·  Lv {level}",
                ToastKind.Threat, 2.6f);
            GameUi.IsBlocking = false;
            _transitioning = false;
        }

        private void Update()
        {
            if (_hintCooldown > 0f)
            {
                _hintCooldown -= Time.deltaTime;
            }
        }

        public void EnterDoor(DoorDir dir)
        {
            if (_transitioning || _current == null || GameUi.IsBlocking || GameUi.IsPaused)
            {
                return;
            }

            var next = generator.GetNeighbor(_current, dir);
            if (next == null)
            {
                return;
            }

            StartCoroutine(TransitionRoutine(next, dir));
        }

        private IEnumerator TransitionRoutine(DungeonRoomData next, DoorDir exitDir)
        {
            _transitioning = true;
            GameUi.IsBlocking = true;

            var controller = _player != null ? _player.GetComponent<PlayerController>() : null;
            controller?.SetControlEnabled(false);

            var intoRoom = FaceIntoRoom(exitDir);
            var doorPos = DoorWorldPos(exitDir);
            // 문 앞(안쪽) → 문턱까지
            var approach = doorPos - intoRoom * 1.35f;
            var threshold = doorPos - intoRoom * 0.55f;

            ClearPassage();

            // 1) 문 쪽으로 고개 돌리고, 문이 열리기 시작
            controller?.ForceFacing(intoRoom);
            controller?.LockFacing(intoRoom, 2.5f);
            _cameraRig?.SetFacing(intoRoom);
            var doorOpen = StartCoroutine(OpenDoorPassageAnimated(exitDir, 0.5f));

            // 문짝이 조금 열린 뒤 걷기 시작 (사람처럼)
            yield return new WaitForSeconds(0.12f);
            controller?.BeginScriptedWalkTo(approach, 3.7f);
            yield return WaitScriptedArrive(controller, 0.38f, 1.25f);

            // 2) 문턱으로 들어서며 암전 (걷기와 겹침)
            controller?.BeginScriptedWalkTo(threshold, 3.4f);
            var fadeOut = StartCoroutine(ScreenFade.To(1f, 0.32f));
            yield return WaitScriptedArrive(controller, 0.35f, 0.85f);
            if (fadeOut != null)
            {
                yield return fadeOut;
            }

            if (doorOpen != null)
            {
                // 남는 문 연출은 끊어도 됨 — 곧 방 교체
                StopCoroutine(doorOpen);
            }

            controller?.EndScriptedWalk();

            // 3) 새 방 — 들어온 문 바로 앞에 서서 대기 (적 쪽으로 돌진하지 않음)
            var entry = StandAtEntryDoor(exitDir);
            var oldPos = _player != null ? _player.position : entry;

            _current = next;
            _visited.Add(_current.Id);
            LoadRoom(_current, entry, snapCamera: false);
            TeleportPlayer(entry);
            _cameraRig?.PreserveRelativeToPlayer(oldPos, entry);
            OpenDoorAsEntry(Opposite(exitDir));

            controller?.ForceFacing(intoRoom);
            controller?.LockFacing(intoRoom, 0.9f);
            _cameraRig?.SetFacing(intoRoom);

            // 4) 그 자리에 선 채로 밝아짐
            yield return ScreenFade.In(0.36f);

            if (_player != null && !IsInsideRoom(_player.position))
            {
                TeleportPlayer(entry);
            }

            _cameraRig?.SoftSnap(0.4f);
            controller?.LockFacing(intoRoom, 0.55f);
            controller?.SetControlEnabled(true);
            GameUi.IsBlocking = false;
            _transitioning = false;
        }

        private IEnumerator WaitScriptedArrive(PlayerController controller, float stopDist, float timeout)
        {
            var t = 0f;
            while (t < timeout)
            {
                if (controller == null || controller.ScriptedNearTarget(stopDist))
                {
                    yield break;
                }

                t += Time.deltaTime;
                yield return null;
            }
        }

        private void TeleportPlayer(Vector3 pos)
        {
            if (_player == null)
            {
                return;
            }

            var rb = _player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = pos;
            }

            _player.position = pos;
        }

        private static bool IsInsideRoom(Vector3 pos) =>
            Mathf.Abs(pos.x) < 7.2f && Mathf.Abs(pos.z) < 5.2f;

        private void ClearPassage()
        {
            if (_passage != null)
            {
                Destroy(_passage.gameObject);
                _passage = null;
            }
        }

        private IEnumerator WalkUntilNear(Vector3 target, float stopDist, float timeout)
        {
            var t = 0f;
            while (t < timeout && _player != null)
            {
                var flat = _player.position;
                flat.y = target.y;
                if ((flat - target).sqrMagnitude <= stopDist * stopDist)
                {
                    yield break;
                }

                t += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator OpenDoorPassageAnimated(DoorDir dir, float duration)
        {
            if (_roomRoot == null)
            {
                yield break;
            }

            RoomDoor target = null;
            foreach (var door in _roomRoot.GetComponentsInChildren<RoomDoor>())
            {
                if (door.Direction == dir)
                {
                    target = door;
                    break;
                }
            }

            if (target != null)
            {
                yield return target.OpenAnimated(duration);
            }
        }

        private void OpenDoorPassage(DoorDir dir)
        {
            if (_roomRoot == null)
            {
                return;
            }

            foreach (var door in _roomRoot.GetComponentsInChildren<RoomDoor>())
            {
                if (door.Direction == dir)
                {
                    door.OpenPassage();
                }
            }
        }

        private void OpenDoorAsEntry(DoorDir dir)
        {
            if (_roomRoot == null)
            {
                return;
            }

            foreach (var door in _roomRoot.GetComponentsInChildren<RoomDoor>())
            {
                if (door.Direction == dir)
                {
                    // 들어온 문은 잠금 해제 — 같은 길로 되돌아가기 가능
                    door.OpenAsEntry(0.6f);
                }
            }
        }

        private static DoorDir Opposite(DoorDir dir) => dir switch
        {
            DoorDir.North => DoorDir.South,
            DoorDir.South => DoorDir.North,
            DoorDir.East => DoorDir.West,
            _ => DoorDir.East
        };

        private static Vector3 DoorWorldPos(DoorDir dir) => dir switch
        {
            DoorDir.North => new Vector3(0f, 0f, 4.55f),
            DoorDir.South => new Vector3(0f, 0f, -4.55f),
            DoorDir.East => new Vector3(6.55f, 0f, 0f),
            _ => new Vector3(-6.55f, 0f, 0f)
        };

        /// <summary>새 방 스폰 — 들어온 문 바로 안쪽 (방 중앙으로 돌진하지 않음).</summary>
        private static Vector3 StandAtEntryDoor(DoorDir exitDir)
        {
            // exitDir로 나감 → 반대쪽 문으로 들어옴
            var enterDoor = Opposite(exitDir);
            var door = DoorWorldPos(enterDoor);
            var intoRoom = FaceIntoRoom(exitDir);
            return door + intoRoom * 1.05f;
        }

        /// <summary>마을→던전 등장용 — 조금 더 안쪽.</summary>
        private static Vector3 EntrySpawn(DoorDir exitDir) => exitDir switch
        {
            DoorDir.North => new Vector3(0f, 0f, -2.5f),
            DoorDir.South => new Vector3(0f, 0f, 2.5f),
            DoorDir.East => new Vector3(-3.8f, 0f, 0f),
            _ => new Vector3(3.8f, 0f, 0f)
        };

        private static Vector3 FaceIntoRoom(DoorDir exitDir) => exitDir switch
        {
            DoorDir.North => Vector3.forward,
            DoorDir.South => Vector3.back,
            DoorDir.East => Vector3.right,
            _ => Vector3.left
        };

        private void LoadRoom(DungeonRoomData room, Vector3 playerSpawn, bool snapCamera = true)
        {
            if (_roomRoot != null)
            {
                Destroy(_roomRoot.gameObject);
            }

            _roomMonsters.Clear();

            var lockDoors = room.Type != RoomType.Start && !room.Cleared;
            _roomRoot = EnvironmentFactory3D.BuildDungeonRoom(worldRoot, room, lockDoors);

            var depth = Mathf.Max(1, _visited.Count);
            GameManager.Instance?.NoteDepth(depth);
            QuestCatalog.NotifyDepth(depth);
            SessionStats.NoteRoom();
            var roomLabel = room.Type switch
            {
                RoomType.Start => "입구",
                RoomType.Exit => "출구",
                RoomType.Treasure => "보물방",
                RoomType.Boss => "보스",
                RoomType.Event => "성소",
                _ => "전투"
            };
            _hud?.SetRoomTitle($"깊이 {depth}  ·  {roomLabel}  ·  {DifficultyScaler.Current.Label}");
            _hud?.SetThreatChip($"{DifficultyScaler.Current.Label}  T{DifficultyScaler.Current.Tier}");
            CombatAudio.RoomEnter();

            if (room.Type == RoomType.Event)
            {
                // 이벤트 방은 전투 없이 성소만
                if (!room.Cleared)
                {
                    room.Cleared = true;
                }

                UnlockDoors();
                WireEventShrine();
                _hud?.SetHint("성소 [E] — 축복·도박·안식 중 선택");
            }
            else if (!room.Cleared && (room.Type == RoomType.Combat || room.Type == RoomType.Treasure
                                       || room.Type == RoomType.Exit || room.Type == RoomType.Boss))
            {
                SpawnRoomEnemies(room);
                _hud?.SetHint(room.Type switch
                {
                    RoomType.Treasure => "목표: 적 처치 후 보물상자 [E]",
                    RoomType.Boss => "목표: 보스를 처치하라!",
                    _ => "목표: 적을 모두 처치하면 문 봉인이 풀립니다"
                });
            }
            else
            {
                room.Cleared = true;
                UnlockDoors();
                _hud?.SetHint("문 [E] 또는 방향키로 이동");
            }

            if (room.Type == RoomType.Treasure)
            {
                foreach (var chest in _roomRoot.GetComponentsInChildren<TreasureChest>())
                {
                    chest.Configure(DifficultyScaler.ScaleGold(40 + _difficulty * 6 + Random.Range(0, 25)), 60);
                }
            }

            if (room.Type == RoomType.Exit && room.Cleared)
            {
                SpawnExitPortal();
            }

            if (_player != null)
            {
                var rb = _player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.position = playerSpawn;
                }

                _player.position = playerSpawn;

                if (snapCamera)
                {
                    _cameraRig?.SoftSnap(0.55f);
                }
            }

            _minimap?.SetCurrent(room);
            RefreshProgressHint();
        }

        private void SpawnRoomEnemies(DungeonRoomData room)
        {
            if (room.Type == RoomType.Boss)
            {
                SpawnSlime(new Vector3(0f, 0f, 1.5f), elite: false, boss: true);
                var adds = DifficultyScaler.BossAdds();
                for (var i = 0; i < adds; i++)
                {
                    var offset = Random.insideUnitCircle * 2.6f;
                    SpawnSlime(new Vector3(offset.x, 0f, offset.y), elite: true);
                }

                room.MonsterCount = _roomMonsters.Count;
                return;
            }

            var baseCount = room.Type == RoomType.Treasure ? 1 : room.Type == RoomType.Exit ? 3 : Random.Range(2, 4);
            var count = DifficultyScaler.RoomEnemyCount(room.Type, baseCount);
            room.MonsterCount = count;
            for (var i = 0; i < count; i++)
            {
                var offset = Random.insideUnitCircle * 2.2f;
                var elite = DifficultyScaler.RollElite(room.Type, i);
                SpawnSlime(new Vector3(offset.x, 0f, offset.y), elite);
            }
        }

        private void SpawnSlime(Vector3 position, bool elite = false, bool boss = false)
        {
            var slime = ProceduralFactory.CreateSlime(position, _roomRoot);
            var brain = slime.GetComponent<MonsterBrain>();
            if (brain == null)
            {
                return;
            }

            var baseHp = 36 + _difficulty * 5;
            var baseDmg = 9 + _difficulty;
            var baseExp = 12 + _difficulty * 2;
            var baseSpd = 2.85f + _difficulty * 0.06f;

            var hp = DifficultyScaler.ScaleHp(baseHp, elite, boss);
            var dmg = DifficultyScaler.ScaleDamage(baseDmg, elite, boss);
            var exp = DifficultyScaler.ScaleExp(baseExp, elite, boss);
            var spd = DifficultyScaler.ScaleSpeed(baseSpd, elite, boss);

            brain.Configure(hp, dmg, exp, spd, elite, boss, DifficultyScaler.Current.Style);
            _roomMonsters.Add(brain);
        }

        private void WireEventShrine()
        {
            if (_roomRoot == null)
            {
                return;
            }

            var shrine = _roomRoot.Find("EventShrine");
            if (shrine == null)
            {
                return;
            }

            var ui = FindFirstObjectByType<DialogueUI>();
            var ev = shrine.GetComponent<DungeonEventInteractable>()
                     ?? shrine.gameObject.AddComponent<DungeonEventInteractable>();
            var label = shrine.GetComponentInChildren<WorldLabel>();
            ev.Configure(ui, label);
        }

        private void SpawnExitPortal()
        {
            // 제단/상자와 XZ가 겹치지 않게 살짝 앞쪽에 배치
            var marker = ProceduralFactory.CreatePortal(new Vector3(0f, 0f, -1.8f), _roomRoot);
            marker.AddComponent<ExitPortal>();
            WorldLabel.Attach(marker.transform, "마을 복귀 [E]", new Vector3(0f, 3.1f, 0f),
                new Color(0.7f, 1f, 1f), 0.042f, LabelShowMode.Proximity, 3.2f, priority: 3);
            _dungeonCleared = true;
            var bonus = 40 + _difficulty * 10;
            Combat.CombatAudio.Portal();
            _clearBanner?.Show($"던전 클리어! 포탈에서 [E] 귀환 (+{bonus}G)", ToastKind.Success, 2.8f);
            _hud?.SetHint("출구 포탈에서 [E] — 마을로 귀환");
        }

        public void NotifyMonsterDefeated(MonsterBrain monster)
        {
            _roomMonsters.Remove(monster);
            _cameraRig?.Shake(0.1f, 0.1f);

            var isBoss = monster != null && monster.IsBoss;
            QuestCatalog.NotifyKill(isBoss);
            SessionStats.NoteKill(isBoss);
            StyleCombo.NoteKill(isBoss);

            var drop = DifficultyScaler.ScaleGold(3 + _difficulty + Random.Range(0, 4));
            drop = QuestCatalog.ApplyGoldBonus(drop);
            drop = Mathf.RoundToInt(drop * StyleCombo.GoldMul);
            if (QuestCatalog.KillStreak >= 3)
            {
                drop += 1 + QuestCatalog.KillStreak / 3;
            }

            GameManager.Instance?.AddGold(drop);
            SessionStats.NoteGold(drop);
            Combat.CombatAudio.Coin();
            if (monster != null)
            {
                FloatingText.Gold(monster.transform.position + Vector3.up * 1.2f, drop);
                if (QuestCatalog.KillStreak >= 3)
                {
                    FloatingText.Spawn(monster.transform.position + Vector3.up * 1.85f,
                        $"×{QuestCatalog.KillStreak}", FloatTextKind.Info);
                }
            }

            AchievementCatalog.TryUnlock(AchievementId.FirstBlood, "첫 처치");
            _playerStats?.NotifyMetaChanged();
            _hud?.RefreshQuest();

            if (_roomMonsters.Count > 0 || _current == null)
            {
                return;
            }

            _current.Cleared = true;
            UnlockDoors();
            CombatAudio.DoorUnlock();
            _clearBanner?.Show("방 클리어  ·  문이 열렸다", ToastKind.Success, 1.6f);
            _hud?.SetHint(_current.Type switch
            {
                RoomType.Treasure => "방 클리어! 보물상자[E]를 열고 다음 방으로",
                RoomType.Boss => "보스 처치! 출구로 향하세요",
                _ => "방 클리어! 문으로 다음 방에 들어가세요"
            });
            _cameraRig?.Shake(_current.Type == RoomType.Boss ? 0.28f : 0.14f, 0.14f);
            if (_current.Type == RoomType.Boss)
            {
                var bossGold = QuestCatalog.ApplyGoldBonus(
                    DifficultyScaler.ScaleGold(35 + _difficulty * 6));
                GameManager.Instance?.AddGold(bossGold);
                if (monster != null)
                {
                    FloatingText.Gold(monster.transform.position + Vector3.up * 1.6f, bossGold);
                }

                AchievementCatalog.TryUnlock(AchievementId.FirstBoss, "보스 격파");
                _clearBanner?.Show("보스 격파!", ToastKind.Danger, 1.8f);
            }

            if (_current.Type == RoomType.Exit)
            {
                SpawnExitPortal();
            }

            _minimap?.SetCurrent(_current);
            RefreshProgressHint();
        }

        private void UnlockDoors()
        {
            if (_roomRoot == null)
            {
                return;
            }

            foreach (var door in _roomRoot.GetComponentsInChildren<RoomDoor>())
            {
                door.SetLocked(false);
            }
        }

        private void RefreshProgressHint()
        {
            var cleared = 0;
            foreach (var r in generator.Rooms)
            {
                if (r.Cleared)
                {
                    cleared++;
                }
            }

            if (_current != null && _current.Cleared && !_dungeonCleared)
            {
                _hud?.SetHint($"탐험 {_visited.Count}/{generator.Rooms.Count}방 · 클리어 {cleared} · 밝은 문으로 [E]");
            }
        }

        public void ShowDoorLockedHint()
        {
            if (_hintCooldown > 0f)
            {
                return;
            }

            _hintCooldown = 0.8f;
            Combat.CombatAudio.Locked();
            _hud?.SetHint("문이 잠겨 있다! 방의 적을 모두 처치하세요");
        }

        private GameObject SpawnPlayer(Vector3 position)
        {
            var player = ProceduralFactory.CreatePlayer(position);

            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = false;
                cam.fieldOfView = 42f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.04f, 0.04f, 0.06f);
                cam.nearClipPlane = 0.05f;
                cam.farClipPlane = 80f;

                _cameraRig = cam.GetComponent<CameraRig3D>() ?? cam.gameObject.AddComponent<CameraRig3D>();
                _cameraRig.SetTarget(player.transform);
            }

            return player;
        }

        public void TryReturnToTown()
        {
            if (!_dungeonCleared || _returning || _transitioning || GameUi.IsPaused)
            {
                return;
            }

            StartCoroutine(ReturnToTownRoutine());
        }

        private IEnumerator ReturnToTownRoutine()
        {
            _returning = true;
            _transitioning = true;
            GameUi.IsBlocking = true;

            var controller = _player != null ? _player.GetComponent<PlayerController>() : null;
            controller?.SetControlEnabled(false);

            var portal = FindFirstObjectByType<ExitPortal>();
            if (portal != null && _player != null && controller != null)
            {
                var target = portal.transform.position;
                target.y = 0f;
                var approach = Vector3.Lerp(_player.position, target, 0.85f);
                approach.y = 0f;
                controller.BeginScriptedWalkTo(approach, 3.6f);
                _cameraRig?.SetFacing((target - _player.position).normalized);
                yield return WaitScriptedArrive(controller, 0.5f, 1.2f);

                var fadeOut = StartCoroutine(ScreenFade.Out(0.36f));
                controller.BeginScriptedWalkTo(target, 2.8f);
                yield return WaitScriptedArrive(controller, 0.55f, 0.7f);
                controller.EndScriptedWalk();
                if (fadeOut != null)
                {
                    yield return fadeOut;
                }
            }
            else
            {
                yield return ScreenFade.Out(0.34f);
            }

            _playerStats?.SyncToSave();
            QuestCatalog.NotifyDungeonClear();
            var bonus = QuestCatalog.ApplyGoldBonus(
                DifficultyScaler.ScaleGold(40 + _difficulty * 8));
            bonus += QuestCatalog.StreakBonusGold();
            GameManager.Instance?.ReturnToTownAfterClear(bonus, cinematic: true);
        }
    }
}
