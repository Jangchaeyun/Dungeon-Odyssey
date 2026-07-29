using System.Collections;
using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Player;
using DungeonOdyssey.UI;
using DungeonOdyssey.View3D;
using UnityEngine;

namespace DungeonOdyssey.Town
{
    /// <summary>마을 동쪽 문 — 동쪽으로 걸어 들어간 뒤 암전 → 던전.</summary>
    public class DungeonEntrance : MonoBehaviour
    {
        private const float InteractRadius = 4.5f;

        private bool _entering;
        private bool _playerNear;
        private Transform _player;
        private Transform _hinge;
        private Collider _doorBlock;

        private void Start()
        {
            var sphere = GetComponent<SphereCollider>();
            if (sphere == null)
            {
                sphere = gameObject.AddComponent<SphereCollider>();
            }

            sphere.isTrigger = true;
            sphere.radius = InteractRadius;
            sphere.center = new Vector3(0f, 1.2f, 0f);

            foreach (var col in GetComponentsInChildren<Collider>(true))
            {
                if (col == sphere || col.isTrigger)
                {
                    continue;
                }

                if (col.gameObject.name.StartsWith("Block"))
                {
                    continue;
                }

                col.enabled = false;
            }

            var doorBlockGo = transform.Find("BlockDoor");
            if (doorBlockGo != null)
            {
                _doorBlock = doorBlockGo.GetComponent<Collider>();
            }
        }

        private void Update()
        {
            if (_entering || GameUi.IsPaused)
            {
                return;
            }

            EnsurePlayer();
            var near = _player != null && IsPlayerNear();
            if (near != _playerNear)
            {
                _playerNear = near;
                if (near)
                {
                    FindFirstObjectByType<HudUI>()?.SetHint("E — 던전 입장");
                }
            }

            if (!_playerNear)
            {
                return;
            }

            // 다른 UI가 막아도 E로 던전 입장은 우선
            if (GameInput.InteractDown)
            {
                if (GameUi.IsBlocking)
                {
                    FindFirstObjectByType<ForgeShopUI>()?.Hide();
                    FindFirstObjectByType<DialogueUI>()?.Hide();
                    GameUi.IsBlocking = false;
                }

                StartCoroutine(EnterRoutine());
            }
        }

        private void EnsurePlayer()
        {
            if (_player != null)
            {
                return;
            }

            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
            {
                _player = go.transform;
            }
        }

        private bool IsPlayerNear()
        {
            if (_player == null)
            {
                return false;
            }

            var delta = _player.position - transform.position;
            delta.y = 0f;
            // 라벨과 동일하게 거리만 본다 (방향 제한으로 E가 무시되던 문제 수정)
            return delta.sqrMagnitude <= InteractRadius * InteractRadius;
        }

        private IEnumerator EnterRoutine()
        {
            if (_entering)
            {
                yield break;
            }

            _entering = true;
            GameUi.IsBlocking = true;
            SetDoorBlockEnabled(false);

            EnsurePlayer();
            var controller = _player != null ? _player.GetComponent<PlayerController>() : null;
            var cam = Camera.main != null ? Camera.main.GetComponent<CameraRig3D>() : null;
            controller?.SetControlEnabled(false);

            var intoGate = Vector3.right;
            var gatePos = transform.position;
            gatePos.y = 0f;

            controller?.ForceFacing(intoGate);
            controller?.LockFacing(intoGate, 2.5f);
            cam?.SetFacing(intoGate);

            // 문 중심선(Z)에 맞춘 뒤 동쪽으로 직선
            var aligned = new Vector3(Mathf.Min(_player.position.x, gatePos.x - 1.6f), 0f, gatePos.z);
            controller?.SnapPosition(aligned);

            var openCo = StartCoroutine(OpenGateAnimated(0.45f));
            yield return null;

            var inside = new Vector3(gatePos.x + 1.15f, 0f, gatePos.z);
            controller?.BeginScriptedWalkAlong(intoGate, inside, 3.6f);
            var fadeOut = StartCoroutine(ScreenFade.Out(0.34f));
            yield return WaitArrive(controller, 0.4f, 1.4f);
            if (fadeOut != null)
            {
                yield return fadeOut;
            }

            if (openCo != null)
            {
                StopCoroutine(openCo);
            }

            controller?.EndScriptedWalk();

            if (GameManager.Instance == null)
            {
                new GameObject("GameManager").AddComponent<GameManager>();
            }

            // 입장 확정 — 실패해도 막히지 않게 플래그는 LoadScene에서 해제됨
            GameManager.Instance.EnterDungeon(cinematic: true);
        }

        private void SetDoorBlockEnabled(bool enabled)
        {
            if (_doorBlock == null)
            {
                var doorBlockGo = transform.Find("BlockDoor");
                if (doorBlockGo != null)
                {
                    _doorBlock = doorBlockGo.GetComponent<Collider>();
                }
            }

            if (_doorBlock != null)
            {
                _doorBlock.enabled = enabled;
            }
        }

        private IEnumerator OpenGateAnimated(float duration)
        {
            CombatAudio.DoorOpen();
            EnsureGateHinge();
            if (_hinge == null)
            {
                HideGateParts();
                SetDoorBlockEnabled(false);
                yield break;
            }

            var start = _hinge.localRotation;
            var end = Quaternion.Euler(0f, -92f, 0f);
            duration = Mathf.Max(0.2f, duration);
            var t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                var u = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / duration), 3f);
                _hinge.localRotation = Quaternion.Slerp(start, end, u);
                yield return null;
            }

            _hinge.localRotation = end;
            SetDoorBlockEnabled(false);
        }

        private void EnsureGateHinge()
        {
            if (_hinge != null)
            {
                return;
            }

            var existing = transform.Find("Hinge");
            if (existing != null)
            {
                _hinge = existing;
                return;
            }

            var gate = transform.Find("Gate");
            if (gate == null)
            {
                return;
            }

            var hingeGo = new GameObject("Hinge");
            _hinge = hingeGo.transform;
            _hinge.SetParent(transform, false);
            _hinge.localPosition = new Vector3(-1.1f, 1.55f, 0f);
            _hinge.localRotation = Quaternion.identity;

            foreach (var name in new[] { "Gate", "BandA", "BandB", "BandC", "Seal" })
            {
                var part = transform.Find(name);
                if (part != null)
                {
                    part.SetParent(_hinge, true);
                }
            }
        }

        private void HideGateParts()
        {
            foreach (Transform child in transform)
            {
                if (child.name is "Gate" or "BandA" or "BandB" or "BandC" or "Seal")
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private static IEnumerator WaitArrive(PlayerController controller, float stopDist, float timeout)
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
    }
}
