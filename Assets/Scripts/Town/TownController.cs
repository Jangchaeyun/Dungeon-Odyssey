using System.Collections;
using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Dungeon;
using DungeonOdyssey.Player;
using DungeonOdyssey.UI;
using DungeonOdyssey.View3D;
using UnityEngine;

namespace DungeonOdyssey.Town
{
    public class TownController : MonoBehaviour
    {
        private void Start()
        {
            EnsureGameManager();
            CombatAudio.PlayTownAmbience();
            ScreenFade.SetTheme(false);
            var ui = RuntimeUiFactory.BuildGameplayHud(includeClearBanner: true);

            var root = new GameObject("TownWorld").transform;
            EnvironmentFactory3D.BuildTown(root);
            DungeonOdyssey.Art.AmbientDust.Create(root, new Color(0.55f, 0.48f, 0.4f, 0.4f), 6);
            SpawnInteractables(root, ui.dialogue);

            var cinematic = GameManager.Instance != null && GameManager.Instance.PendingTownArrivalCinematic;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PendingTownArrivalCinematic = false;
            }

            // 던전에서 나오면 입구 앞(겹치지 않게)에서 마을 안쪽으로
            var spawnPos = cinematic ? new Vector3(15.5f, 0f, 0.5f) : new Vector3(-14f, 0f, 0.2f);
            if (cinematic)
            {
                ScreenFade.Set(1f);
            }

            var player = SpawnPlayer(spawnPos);
            var stats = player.GetComponent<PlayerStats>();
            stats?.LoadFromSave();
            ui.hud.Bind(stats);
            ui.hud.RefreshQuest();
            ui.weaponHotbar?.Bind(stats);
            ui.bag?.Bind(stats);
            ui.minimap?.SetupTown(player.transform);

            var save = GameManager.Instance.CurrentSave;
            QuestCatalog.EnsureDaily(save);

            if (save != null && !save.tutorialDone)
            {
                ui.hud.SetHint("① Mira에게 의뢰 수락 [E] → ② Borin 상점 → ③ 동쪽 던전");
                ui.dialogue.Show("안내",
                    "조작 요약\n" +
                    "이동 WASD · 공격 Space/J · 무기 스킬 R · B 가방 · 휠/V 무기 · 포션 Q · 말걸기 E · 메뉴 Esc\n\n" +
                    "청록색 Mira 의뢰판에서 퀘스트를 수락해야 진행에 표시됩니다.\n" +
                    "의뢰를 받은 뒤 동쪽 던전으로 출발하세요.");
                save.tutorialDone = true;
                GameManager.Instance.Persist(SceneNames.Town);
                ui.hud.RefreshQuest();
            }
            else if (!string.IsNullOrEmpty(GameManager.Instance.PendingTownBanner)
                     || !string.IsNullOrEmpty(GameManager.Instance.PendingRunReportDetail))
            {
                if (!cinematic)
                {
                    ShowPendingClearReport(ui);
                }
            }
            else if (GameManager.Instance.LastDeathGoldLost > 0)
            {
                var lost = GameManager.Instance.ConsumeLastDeathGoldLost();
                ui.hud.SetHint($"귀환 · 골드 -{lost}G · Mira 의뢰·Borin 재정비");
            }
            else
            {
                var clears = save != null ? save.dungeonClears : 0;
                ui.hud.SetHint(clears > 0
                    ? $"클리어 {clears}회 · {QuestCatalog.HudLine(save)} · 동쪽 던전"
                    : "Mira에게 의뢰 수락 [E] · Borin 대장간 · 동쪽 던전");
            }

            GameManager.Instance?.Persist(SceneNames.Town);

            if (cinematic)
            {
                StartCoroutine(TownArrivalRoutine(player, ui));
            }
        }

        private IEnumerator TownArrivalRoutine(GameObject player, RuntimeUiFactory.GameplayUiBundle ui)
        {
            GameUi.IsBlocking = true;
            var controller = player != null ? player.GetComponent<PlayerController>() : null;
            var cam = Camera.main != null ? Camera.main.GetComponent<CameraRig3D>() : null;
            controller?.SetControlEnabled(false);

            var intoTown = Vector3.left;
            controller?.ForceFacing(intoTown);
            cam?.SetFacing(intoTown);

            var target = new Vector3(7.6f, 0f, 0.5f);
            controller?.BeginScriptedWalkAlong(intoTown, target, 3.8f);
            controller?.LockFacing(intoTown, 1.2f);

            var fadeIn = StartCoroutine(ScreenFade.In(0.42f));
            var t = 0f;
            while (t < 1.15f && player != null)
            {
                if (controller != null && controller.ScriptedNearTarget(0.4f))
                {
                    break;
                }

                t += Time.deltaTime;
                yield return null;
            }

            if (fadeIn != null)
            {
                yield return fadeIn;
            }

            controller?.EndScriptedWalk();
            controller?.LockFacing(intoTown, 0.55f);
            controller?.SetControlEnabled(true);

            ShowPendingClearReport(ui);
            GameUi.IsBlocking = false;
        }

        private static void ShowPendingClearReport(RuntimeUiFactory.GameplayUiBundle ui)
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            var banner = GameManager.Instance.PendingTownBanner;
            var detail = GameManager.Instance.PendingRunReportDetail;
            GameManager.Instance.PendingTownBanner = null;
            GameManager.Instance.PendingRunReportDetail = null;
            if (string.IsNullOrEmpty(banner) && string.IsNullOrEmpty(detail))
            {
                return;
            }

            ui.clear?.Show(string.IsNullOrEmpty(detail) ? banner : detail, ToastKind.Achievement, 4.2f);
            ui.hud?.SetHint(string.IsNullOrEmpty(banner) ? "마을에 귀환했다" : banner);
        }

        private static void EnsureGameManager()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EnsureSaveData();
                return;
            }

            new GameObject("GameManager").AddComponent<GameManager>().EnsureSaveData();
        }

        private void SpawnInteractables(Transform root, DialogueUI dialogueUi)
        {
            var npc = ProceduralFactory.CreateNpc(new Vector3(-2.2f, 0f, 2.2f), root);
            // 의뢰판 비주얼 (빨간 깃발처럼 보이던 장식과 구분)
            EnvironmentFactory3D.BuildQuestNoticeBoard(root, new Vector3(-0.6f, 0f, 3.4f));

            var triggerGo = new GameObject("InteractTrigger");
            triggerGo.transform.SetParent(npc.transform, false);
            triggerGo.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            var npcTrigger = triggerGo.AddComponent<SphereCollider>();
            npcTrigger.isTrigger = true;
            npcTrigger.radius = 2.2f;

            var npcLabel = WorldLabel.Attach(npc.transform, "가이드 Mira [E]", new Vector3(0f, 2.35f, 0f),
                new Color(0.4f, 0.9f, 1f), 0.042f, LabelShowMode.Proximity, 3.2f);
            var board = triggerGo.AddComponent<QuestBoardInteractable>();
            board.Configure(dialogueUi, npcLabel, "E — Mira 의뢰판");

            var shop = ActorFactory3D.CreateBlacksmith(new Vector3(-10.5f, 0f, -2.2f), root);
            var shopTrig = new GameObject("ShopTrigger");
            shopTrig.transform.SetParent(shop.transform, false);
            shopTrig.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            var st = shopTrig.AddComponent<SphereCollider>();
            st.isTrigger = true;
            st.radius = 1.8f;
            var shopLabel = WorldLabel.Attach(shop.transform, "대장간  Borin",
                new Vector3(0f, 2.35f, 0f), new Color(0.9f, 0.72f, 0.45f), 0.04f, LabelShowMode.Proximity, 3.2f);
            var shopUi = shopTrig.AddComponent<ShopInteractable>();
            shopUi.Configure(dialogueUi, shopLabel);

            var entrance = ProceduralFactory.CreateDungeonDoor(new Vector3(20.5f, 0f, 0.5f), root);
            entrance.AddComponent<DungeonEntrance>();
            WorldLabel.Attach(entrance.transform, "던전 입구 [E]", new Vector3(0f, 3.4f, 0f),
                new Color(1f, 0.4f, 0.35f), 0.045f, LabelShowMode.Proximity, 4.5f);
            Prim.Cyl("GateBeacon", entrance.transform, new Vector3(0f, 4.2f, 0f),
                new Vector3(0.25f, 0.4f, 0.25f), new Color(1f, 0.35f, 0.3f), 0.5f);
        }

        private GameObject SpawnPlayer(Vector3 position)
        {
            var player = ProceduralFactory.CreatePlayer(position);
            // 마을: 칼을 아래로 늘어뜨린 평상 자세
            player.GetComponent<HumanoidAnimator>()?.SetCombatStance(false);
            SetupCamera3D(player.transform, new Color(0.28f, 0.26f, 0.32f));
            return player;
        }

        private static void SetupCamera3D(Transform target, Color sky)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            cam.orthographic = false;
            cam.fieldOfView = 44f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = sky;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 110f;
            var rig = cam.GetComponent<CameraRig3D>() ?? cam.gameObject.AddComponent<CameraRig3D>();
            rig.SetTarget(target);
        }
    }
}
