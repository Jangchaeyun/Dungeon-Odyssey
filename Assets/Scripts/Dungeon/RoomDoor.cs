using System.Collections;
using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.View3D;
using UnityEngine;

namespace DungeonOdyssey.Dungeon
{
    public enum DoorDir
    {
        North,
        South,
        East,
        West
    }

    public class RoomDoor : MonoBehaviour
    {
        [SerializeField] private DoorDir direction;
        [SerializeField] private bool locked;

        private bool _playerInside;
        private bool _passageOpen;
        private MeshRenderer[] _tintRenderers;
        private Light _sealLight;
        private Transform _seal;
        private WorldLabel _label;
        private Transform _hinge;
        private Color _openColor = new(0.22f, 0.38f, 0.4f);
        private Color _lockedColor = new(0.42f, 0.14f, 0.16f);
        private float _enterCooldown;

        public DoorDir Direction => direction;
        public bool Locked => locked;
        public bool PassageOpen => _passageOpen;

        public void Setup(DoorDir dir, bool isLocked)
        {
            direction = dir;
            locked = isLocked;
            if (_tintRenderers == null || _tintRenderers.Length == 0)
            {
                AutoCollectVisuals();
            }

            RefreshVisual();
        }

        public void BindVisuals(MeshRenderer[] tintRenderers, Light sealLight = null, Transform seal = null)
        {
            _tintRenderers = tintRenderers;
            _sealLight = sealLight;
            _seal = seal;
            RefreshVisual();
        }

        public void BindLabel(WorldLabel label)
        {
            _label = label;
            _label?.SetVisible(false);
        }

        public void SetLocked(bool value)
        {
            var wasLocked = locked;
            locked = value;
            RefreshVisual();
            if (wasLocked && !value)
            {
                CombatAudio.DoorUnlock();
            }
        }

        private void AutoCollectVisuals()
        {
            var list = new System.Collections.Generic.List<MeshRenderer>();
            foreach (Transform child in transform)
            {
                if (child.name is "Leaf" or "BandA" or "BandB" or "BandC" or "Handle")
                {
                    var mr = child.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        list.Add(mr);
                    }
                }

                if (child.name == "Seal")
                {
                    _seal = child;
                    _sealLight = child.GetComponent<Light>() ?? child.GetComponentInChildren<Light>();
                    var smr = child.GetComponent<MeshRenderer>();
                    if (smr != null)
                    {
                        list.Add(smr);
                    }
                }
            }

            var self = GetComponent<MeshRenderer>();
            if (self != null)
            {
                list.Add(self);
            }

            _tintRenderers = list.ToArray();
        }

        private void RefreshVisual()
        {
            if (_passageOpen)
            {
                return;
            }

            if (_tintRenderers == null || _tintRenderers.Length == 0)
            {
                AutoCollectVisuals();
            }

            var leafColor = locked ? _lockedColor : _openColor;
            var bandColor = locked
                ? new Color(0.55f, 0.22f, 0.22f)
                : new Color(0.45f, 0.55f, 0.52f);

            if (_tintRenderers != null)
            {
                foreach (var r in _tintRenderers)
                {
                    if (r == null)
                    {
                        continue;
                    }

                    var isBand = r.name.StartsWith("Band") || r.name == "Handle";
                    var isSeal = r.name == "Seal";
                    if (isSeal)
                    {
                        r.sharedMaterial = locked
                            ? MatLib.GetEmissive(MatLib.AccentWine, new Color(1f, 0.25f, 0.2f), 1.8f, 0.55f)
                            : MatLib.GetEmissive(MatLib.AccentTeal, new Color(0.4f, 1f, 0.85f), 1.4f, 0.55f);
                    }
                    else
                    {
                        r.sharedMaterial = MatLib.Get(isBand ? bandColor : leafColor,
                            isBand ? 0.55f : 0.28f, isBand ? 0.55f : 0.05f);
                    }
                }
            }

            if (_seal != null)
            {
                _seal.gameObject.SetActive(true);
                _seal.localScale = locked ? Vector3.one : Vector3.one * 0.7f;
            }

            if (_sealLight != null)
            {
                _sealLight.enabled = true;
                _sealLight.color = locked
                    ? new Color(1f, 0.3f, 0.25f)
                    : new Color(0.45f, 1f, 0.85f);
                _sealLight.intensity = locked ? 1.8f : 1.1f;
            }

            if (_label != null)
            {
                var dirKo = DirKorean(direction);
                _label.SetText(locked ? $"{dirKo} 문 · 봉인" : $"{dirKo} 문 [E]");
                _label.SetColor(locked ? new Color(1f, 0.4f, 0.35f) : new Color(0.5f, 1f, 0.85f));
            }
        }

        /// <summary>즉시 통로 확보 (도착 방 등).</summary>
        public void OpenPassage()
        {
            if (_passageOpen)
            {
                return;
            }

            _passageOpen = true;
            EnsureHinge();
            if (_hinge != null)
            {
                _hinge.localRotation = Quaternion.Euler(0f, -92f, 0f);
            }

            HideSeal();
            DisableLeafCollider();
            RefreshOpenLabel();
        }

        /// <summary>들어온 문 — 열리고 잠금 해제되어 바로 되돌아갈 수 있음.</summary>
        public void OpenAsEntry(float interactDelay = 0.55f)
        {
            locked = false;
            OpenPassage();
            _enterCooldown = Mathf.Max(0.2f, interactDelay);
            RefreshOpenLabel();
        }

        private void RefreshOpenLabel()
        {
            if (_label == null)
            {
                return;
            }

            var dirKo = DirKorean(direction);
            if (locked)
            {
                _label.SetText($"{dirKo} 문 · 봉인");
                _label.SetColor(new Color(1f, 0.4f, 0.35f));
            }
            else
            {
                _label.SetText($"{dirKo} 문 [E]");
                _label.SetColor(new Color(0.5f, 1f, 0.85f));
            }
        }

        /// <summary>문짝이 경첩으로 열리며 통로가 생김.</summary>
        public IEnumerator OpenAnimated(float duration = 0.48f)
        {
            if (_passageOpen)
            {
                yield break;
            }

            _passageOpen = true;
            CombatAudio.DoorOpen();
            HideSeal();
            EnsureHinge();
            DisableLeafCollider();
            RefreshOpenLabel();

            if (_hinge == null)
            {
                yield break;
            }

            var start = _hinge.localRotation;
            var end = Quaternion.Euler(0f, -92f, 0f);
            duration = Mathf.Max(0.2f, duration);
            var t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                var u = Mathf.Clamp01(t / duration);
                // ease-out cubic — 처음 빠르게 열리다 끝에서 안착
                u = 1f - Mathf.Pow(1f - u, 3f);
                _hinge.localRotation = Quaternion.Slerp(start, end, u);
                yield return null;
            }

            _hinge.localRotation = end;
        }

        private void EnsureHinge()
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

            var hingeGo = new GameObject("Hinge");
            _hinge = hingeGo.transform;
            _hinge.SetParent(transform, false);
            _hinge.localPosition = new Vector3(-1.0f, 1.45f, 0f);
            _hinge.localRotation = Quaternion.identity;

            foreach (var name in new[] { "Leaf", "BandA", "BandB", "BandC", "PanelL", "PanelR", "Handle" })
            {
                var part = transform.Find(name);
                if (part != null)
                {
                    part.SetParent(_hinge, true);
                }
            }
        }

        private void HideSeal()
        {
            if (_seal != null)
            {
                _seal.gameObject.SetActive(false);
            }

            if (_sealLight != null)
            {
                _sealLight.enabled = false;
            }

            var sealChild = transform.Find("Seal");
            if (sealChild != null)
            {
                sealChild.gameObject.SetActive(false);
            }
        }

        private void DisableLeafCollider()
        {
            var leaf = _hinge != null ? _hinge.Find("Leaf") : transform.Find("Leaf");
            if (leaf == null)
            {
                return;
            }

            var col = leaf.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }
        }

        public static string DirKorean(DoorDir dir) => dir switch
        {
            DoorDir.North => "북쪽",
            DoorDir.South => "남쪽",
            DoorDir.East => "동쪽",
            DoorDir.West => "서쪽",
            _ => "문"
        };

        private void Update()
        {
            if (_enterCooldown > 0f)
            {
                _enterCooldown -= Time.deltaTime;
            }

            if (!_playerInside || _enterCooldown > 0f)
            {
                return;
            }

            if (GameInput.InteractDown)
            {
                TryEnter();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _playerInside = true;
            if (!locked)
            {
                _label?.SetVisible(true);
                _label?.SetHighlight(true);
            }

            if (_enterCooldown > 0f)
            {
                return;
            }

            var moveToward = direction switch
            {
                DoorDir.North => GameInput.MoveHeld(DoorHint.North),
                DoorDir.South => GameInput.MoveHeld(DoorHint.South),
                DoorDir.East => GameInput.MoveHeld(DoorHint.East),
                DoorDir.West => GameInput.MoveHeld(DoorHint.West),
                _ => false
            };

            if (moveToward)
            {
                TryEnter();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInside = false;
                _label?.SetHighlight(false);
                _label?.SetVisible(false);
            }
        }

        private void TryEnter()
        {
            if (locked)
            {
                DungeonManager.Instance?.ShowDoorLockedHint();
                return;
            }

            _enterCooldown = 1.2f;
            DungeonManager.Instance?.EnterDoor(direction);
        }
    }
}
