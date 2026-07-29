using System.Collections.Generic;
using DungeonOdyssey.Dungeon;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonOdyssey.UI
{
    /// <summary>
    /// 던전 미니맵 — 전체 방 배치를 실루엣으로 보여 주고, 탐험/현재 위치를 강하게 구분.
    /// </summary>
    public class MinimapUI : MonoBehaviour
    {
        private sealed class RoomVisual
        {
            public Image Floor;
            public Image Fill;
            public Image Ring;
            public Image Glow;
            public Image Pip;
            public Text Icon;
            public CanvasGroup Group;
        }

        private RectTransform _root;
        private Text _title;
        private Text _legend;
        private Image _panelChrome;
        private readonly Dictionary<int, RoomVisual> _rooms = new();
        private readonly List<(Image img, int a, int b)> _hallLinks = new();
        private DungeonGenerator _gen;
        private DungeonRoomData _current;
        private HashSet<int> _visited;
        private bool _townMode;
        private Transform _player;
        private RectTransform _playerDot;
        private Image _playerPulse;
        private Image _playerCore;
        private float _cell = 26f;
        private float _pitch = 34f;
        private float _hall = 5f;

        private static readonly Vector2 TownWorldMin = new(-24f, -16f);
        private static readonly Vector2 TownWorldMax = new(22f, 16f);

        // 고대비 팔레트 — 어두운 배경에서도 방이 또렷하게
        private static readonly Color PanelDeep = new(0.04f, 0.05f, 0.07f, 0.96f);
        private static readonly Color FogSilhouette = new(0.28f, 0.32f, 0.38f, 0.92f);
        private static readonly Color FogFill = new(0.2f, 0.23f, 0.28f, 0.95f);
        private static readonly Color FogRing = new(0.42f, 0.48f, 0.55f, 0.75f);
        private static readonly Color AdjacentFill = new(0.38f, 0.42f, 0.5f, 1f);
        private static readonly Color KnownEmpty = new(0.32f, 0.4f, 0.48f, 1f);
        private static readonly Color Cleared = new(0.28f, 0.52f, 0.48f, 1f);
        private static readonly Color CurrentFill = new(0.2f, 0.72f, 0.78f, 1f);
        private static readonly Color CurrentRing = new(0.65f, 1f, 1f, 1f);
        private static readonly Color HallFog = new(0.32f, 0.36f, 0.42f, 0.7f);
        private static readonly Color HallKnown = new(0.55f, 0.7f, 0.78f, 0.95f);
        private static readonly Color HallCurrent = new(0.7f, 0.95f, 1f, 1f);
        private static readonly Color AccentCyan = new(0.4f, 0.95f, 1f, 1f);
        private static readonly Color AccentGold = new(1f, 0.85f, 0.35f, 1f);
        private static readonly Color AccentMint = new(0.45f, 0.95f, 0.7f, 1f);
        private static readonly Color AccentCoral = new(1f, 0.42f, 0.45f, 1f);
        private static readonly Color AccentViolet = new(0.78f, 0.58f, 1f, 1f);
        private static readonly Color Hostile = new(0.95f, 0.38f, 0.4f, 1f);

        private const float MapHalfW = 118f;
        private const float MapHalfH = 62f;

        public void Bind(RectTransform root, Text title = null, Text legend = null)
        {
            _root = root;
            _title = title;
            _legend = legend;
            if (root != null && root.parent != null)
            {
                _panelChrome = root.parent.GetComponent<Image>();
            }
        }

        public void Setup(DungeonGenerator gen, HashSet<int> visited)
        {
            _townMode = false;
            _gen = gen;
            _visited = visited;
            ApplyChrome(true);

            if (_title != null)
            {
                _title.text = "던전 지도";
                _title.fontSize = 13;
                _title.fontStyle = FontStyle.Bold;
                _title.color = new Color(0.85f, 0.92f, 0.98f, 1f);
            }

            if (_legend != null)
            {
                _legend.text = "●나  ◆입구  ★보물  ✦성소  ☠보스  ▲출구  ·회색=미탐험";
                _legend.fontSize = 9;
                _legend.color = new Color(0.7f, 0.76f, 0.82f, 1f);
                _legend.horizontalOverflow = HorizontalWrapMode.Overflow;
                _legend.verticalOverflow = VerticalWrapMode.Truncate;
            }

            ClearChildren();
            RebuildDungeon();
        }

        public void SetupTown(Transform player)
        {
            _townMode = true;
            _player = player;
            _gen = null;
            ApplyChrome(false);

            if (_title != null)
            {
                _title.text = "마을 지도";
                _title.fontSize = 13;
                _title.fontStyle = FontStyle.Bold;
                _title.color = new Color(0.75f, 0.95f, 0.8f, 1f);
            }

            if (_legend != null)
            {
                _legend.text = "● 나   G 가이드   S 상점   D 던전";
                _legend.fontSize = 9;
                _legend.color = new Color(0.65f, 0.78f, 0.7f, 1f);
            }

            ClearChildren();
            BuildTownMap();
        }

        public void SetCurrent(DungeonRoomData room)
        {
            if (_townMode)
            {
                return;
            }

            _current = room;
            RefreshDungeonLook();
        }

        private void ApplyChrome(bool dungeon)
        {
            if (_panelChrome == null)
            {
                return;
            }

            _panelChrome.color = dungeon
                ? new Color(0.05f, 0.07f, 0.1f, 0.92f)
                : new Color(0.06f, 0.1f, 0.09f, 0.92f);
        }

        private void Update()
        {
            if (_townMode && _player != null && _playerDot != null)
            {
                _playerDot.anchoredPosition = WorldToMap(_player.position);
            }

            var t = Time.unscaledTime;
            if (_playerPulse != null)
            {
                var pulse = 0.35f + 0.4f * (0.5f + 0.5f * Mathf.Sin(t * 3.6f));
                var c = _playerPulse.color;
                c.a = pulse;
                _playerPulse.color = c;
                var s = 1f + 0.14f * Mathf.Sin(t * 3.6f);
                _playerPulse.rectTransform.localScale = new Vector3(s, s, 1f);
            }

            if (_playerCore != null)
            {
                var bob = 1f + 0.08f * Mathf.Sin(t * 5f);
                _playerCore.rectTransform.localScale = new Vector3(bob, bob, 1f);
            }
        }

        private void ClearChildren()
        {
            if (_root == null)
            {
                return;
            }

            foreach (Transform child in _root)
            {
                Destroy(child.gameObject);
            }

            _rooms.Clear();
            _hallLinks.Clear();
            _playerDot = null;
            _playerPulse = null;
            _playerCore = null;
        }

        private void ComputeScale()
        {
            _cell = 26f;
            _pitch = 34f;
            _hall = 5f;
            if (_gen == null || _gen.Rooms.Count == 0)
            {
                return;
            }

            var minX = int.MaxValue;
            var maxX = int.MinValue;
            var minY = int.MaxValue;
            var maxY = int.MinValue;
            foreach (var room in _gen.Rooms)
            {
                var gp = room.GridPosition;
                minX = Mathf.Min(minX, gp.x);
                maxX = Mathf.Max(maxX, gp.x);
                minY = Mathf.Min(minY, gp.y);
                maxY = Mathf.Max(maxY, gp.y);
            }

            var spanX = Mathf.Max(1, maxX - minX);
            var spanY = Mathf.Max(1, maxY - minY);
            // 맵 영역에 맞춰 방 간격 자동 축소/확대
            var pitchX = (MapHalfW * 2f - 8f) / (spanX + 0.85f);
            var pitchY = (MapHalfH * 2f - 8f) / (spanY + 0.85f);
            _pitch = Mathf.Clamp(Mathf.Min(pitchX, pitchY), 18f, 38f);
            _cell = Mathf.Clamp(_pitch * 0.78f, 14f, 30f);
            _hall = Mathf.Clamp(_pitch * 0.16f, 3.5f, 6.5f);
        }

        private void RebuildDungeon()
        {
            if (_root == null || _gen == null)
            {
                return;
            }

            ComputeScale();

            MakeImage("MapBg", Vector2.zero, new Vector2(MapHalfW * 2f + 8f, MapHalfH * 2f + 8f), PanelDeep)
                .transform.SetAsFirstSibling();

            // 배경 그리드 — 위치감
            for (var i = -3; i <= 3; i++)
            {
                MakeImage($"GridH_{i}", new Vector2(0f, i * (_pitch * 0.7f)),
                    new Vector2(MapHalfW * 2f - 4f, 1.2f),
                    new Color(0.35f, 0.42f, 0.5f, 0.18f));
                MakeImage($"GridV_{i}", new Vector2(i * (_pitch * 0.85f), 0f),
                    new Vector2(1.2f, MapHalfH * 2f - 4f),
                    new Color(0.35f, 0.42f, 0.5f, 0.18f));
            }

            var minX = int.MaxValue;
            var maxX = int.MinValue;
            var minY = int.MaxValue;
            var maxY = int.MinValue;
            foreach (var room in _gen.Rooms)
            {
                var gp = room.GridPosition;
                minX = Mathf.Min(minX, gp.x);
                maxX = Mathf.Max(maxX, gp.x);
                minY = Mathf.Min(minY, gp.y);
                maxY = Mathf.Max(maxY, gp.y);
            }

            var cx = (minX + maxX) * 0.5f;
            var cy = (minY + maxY) * 0.5f;

            foreach (var room in _gen.Rooms)
            {
                var pos = CellPos(room.GridPosition, cx, cy);
                if (room.ConnectedEast)
                {
                    var neighbor = _gen.GetNeighbor(room, DoorDir.East);
                    var hall = MakeImage($"HallE_{room.Id}", pos + new Vector2(_pitch * 0.5f, 0f),
                        new Vector2(_pitch - _cell + 6f, _hall), HallFog);
                    _hallLinks.Add((hall, room.Id, neighbor != null ? neighbor.Id : room.Id));
                }

                if (room.ConnectedNorth)
                {
                    var neighbor = _gen.GetNeighbor(room, DoorDir.North);
                    var hall = MakeImage($"HallN_{room.Id}", pos + new Vector2(0f, _pitch * 0.5f),
                        new Vector2(_hall, _pitch - _cell + 6f), HallFog);
                    _hallLinks.Add((hall, room.Id, neighbor != null ? neighbor.Id : room.Id));
                }
            }

            foreach (var room in _gen.Rooms)
            {
                _rooms[room.Id] = BuildRoomVisual(room, CellPos(room.GridPosition, cx, cy));
            }

            RefreshDungeonLook();
        }

        private RoomVisual BuildRoomVisual(DungeonRoomData room, Vector2 pos)
        {
            var root = new GameObject($"Room_{room.Id}");
            root.transform.SetParent(_root, false);
            var rr = root.AddComponent<RectTransform>();
            rr.anchoredPosition = pos;
            rr.sizeDelta = new Vector2(_cell, _cell);
            var group = root.AddComponent<CanvasGroup>();

            var glow = MakeChildImage(root.transform, "Glow", Vector2.zero,
                new Vector2(_cell + 16f, _cell + 16f), Color.clear);
            glow.transform.SetAsFirstSibling();

            var floor = MakeChildImage(root.transform, "Floor", Vector2.zero,
                new Vector2(_cell, _cell), FogSilhouette);
            var fill = MakeChildImage(root.transform, "Fill", Vector2.zero,
                new Vector2(_cell - 4f, _cell - 4f), FogFill);
            var ring = MakeChildImage(root.transform, "Ring", Vector2.zero,
                new Vector2(_cell + 3f, _cell + 3f), FogRing);
            ring.transform.SetSiblingIndex(1);

            var pip = MakeChildImage(root.transform, "Pip", Vector2.zero, new Vector2(8f, 8f), Color.clear);

            var icon = MakeChildText(root.transform, "Icon", "", 12, Vector2.zero,
                new Vector2(24f, 20f), Color.white);
            icon.fontStyle = FontStyle.Bold;
            var outline = icon.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1f, -1f);

            return new RoomVisual
            {
                Floor = floor,
                Fill = fill,
                Ring = ring,
                Glow = glow,
                Pip = pip,
                Icon = icon,
                Group = group
            };
        }

        private void RefreshDungeonLook()
        {
            if (_gen == null)
            {
                return;
            }

            _playerPulse = null;
            _playerCore = null;
            var adjacent = BuildAdjacentSet();

            if (_title != null && _current != null)
            {
                _title.text = $"던전 지도  ·  {RoomLabel(_current.Type)}";
            }

            foreach (var room in _gen.Rooms)
            {
                if (!_rooms.TryGetValue(room.Id, out var v))
                {
                    continue;
                }

                var visited = _visited != null && _visited.Contains(room.Id);
                var isCurrent = _current != null && room.Id == _current.Id;
                var near = adjacent.Contains(room.Id);

                // 모든 방을 보이게 — 미탐험도 실루엣으로 위치 파악
                v.Group.alpha = 1f;
                v.Glow.color = Color.clear;
                v.Ring.color = FogRing;
                v.Pip.color = Color.clear;
                v.Icon.text = "";
                v.Icon.color = Color.white;

                if (isCurrent)
                {
                    v.Floor.color = CurrentFill;
                    v.Fill.color = new Color(0.35f, 0.88f, 0.95f, 1f);
                    v.Ring.color = CurrentRing;
                    v.Glow.color = new Color(0.35f, 0.9f, 1f, 0.45f);
                    v.Pip.color = AccentCyan;
                    v.Pip.rectTransform.sizeDelta = new Vector2(9f, 9f);
                    v.Icon.text = "●";
                    v.Icon.fontSize = 12;
                    v.Icon.color = Color.white;
                    _playerPulse = v.Glow;
                    _playerCore = v.Pip;
                    ApplyPoi(room, v, true);
                    continue;
                }

                if (visited)
                {
                    if (room.Cleared || room.Type == RoomType.Start)
                    {
                        v.Floor.color = Cleared;
                        v.Fill.color = new Color(0.35f, 0.62f, 0.58f, 1f);
                        v.Ring.color = new Color(0.5f, 0.78f, 0.72f, 0.9f);
                    }
                    else
                    {
                        v.Floor.color = KnownEmpty;
                        v.Fill.color = new Color(0.48f, 0.28f, 0.3f, 1f);
                        v.Ring.color = new Color(0.85f, 0.4f, 0.42f, 0.85f);
                        v.Pip.color = Hostile;
                        v.Pip.rectTransform.sizeDelta = new Vector2(6f, 6f);
                    }

                    ApplyPoi(room, v, true);
                    continue;
                }

                if (near)
                {
                    // 인접 미탐험 — 갈 수 있는 방
                    v.Floor.color = AdjacentFill;
                    v.Fill.color = new Color(0.45f, 0.5f, 0.58f, 1f);
                    v.Ring.color = new Color(0.75f, 0.82f, 0.9f, 0.95f);
                    v.Icon.text = "?";
                    v.Icon.fontSize = 13;
                    v.Icon.color = new Color(0.95f, 0.98f, 1f, 1f);
                    ApplyPoi(room, v, false);
                    continue;
                }

                // 멀리 있는 미탐험 — 위치만 보이게 실루엣
                v.Floor.color = FogSilhouette;
                v.Fill.color = FogFill;
                v.Ring.color = FogRing;
                v.Group.alpha = 0.85f;

                // 특수 방은 멀리서도 아이콘만 흐리게 (목표 찾기용)
                if (room.Type is RoomType.Boss or RoomType.Exit or RoomType.Treasure
                    or RoomType.Event or RoomType.Start)
                {
                    ApplyPoi(room, v, false);
                    if (v.Icon != null && !string.IsNullOrEmpty(v.Icon.text))
                    {
                        var c = v.Icon.color;
                        c.a = 0.55f;
                        v.Icon.color = c;
                    }
                }
            }

            foreach (var (img, a, b) in _hallLinks)
            {
                if (img == null)
                {
                    continue;
                }

                var aKnown = IsRoomRevealed(a, adjacent);
                var bKnown = IsRoomRevealed(b, adjacent);
                var touchesCurrent = _current != null && (_current.Id == a || _current.Id == b);
                if (touchesCurrent && (aKnown || bKnown))
                {
                    img.color = HallCurrent;
                }
                else if (aKnown && bKnown)
                {
                    img.color = HallKnown;
                }
                else
                {
                    img.color = HallFog;
                }
            }
        }

        private bool IsRoomRevealed(int id, HashSet<int> adjacent)
        {
            if (_current != null && _current.Id == id)
            {
                return true;
            }

            if (_visited != null && _visited.Contains(id))
            {
                return true;
            }

            return adjacent.Contains(id);
        }

        private void ApplyPoi(DungeonRoomData room, RoomVisual v, bool visited)
        {
            switch (room.Type)
            {
                case RoomType.Start:
                    v.Icon.text = "◆";
                    v.Icon.fontSize = 12;
                    v.Icon.color = AccentCyan;
                    v.Ring.color = new Color(0.4f, 0.9f, 1f, 0.85f);
                    break;
                case RoomType.Treasure:
                    v.Icon.text = "★";
                    v.Icon.fontSize = 13;
                    v.Icon.color = AccentGold;
                    if (visited)
                    {
                        v.Fill.color = new Color(0.55f, 0.45f, 0.18f, 1f);
                    }

                    break;
                case RoomType.Event:
                    v.Icon.text = "✦";
                    v.Icon.fontSize = 13;
                    v.Icon.color = AccentMint;
                    if (visited)
                    {
                        v.Fill.color = new Color(0.22f, 0.52f, 0.4f, 1f);
                    }

                    break;
                case RoomType.Boss:
                    v.Icon.text = "☠";
                    v.Icon.fontSize = 13;
                    v.Icon.color = AccentCoral;
                    v.Ring.color = new Color(1f, 0.4f, 0.45f, 0.9f);
                    v.Glow.color = new Color(1f, 0.3f, 0.35f, 0.28f);
                    if (visited)
                    {
                        v.Fill.color = new Color(0.55f, 0.2f, 0.24f, 1f);
                    }

                    break;
                case RoomType.Exit:
                    v.Icon.text = "▲";
                    v.Icon.fontSize = 13;
                    v.Icon.color = AccentViolet;
                    v.Ring.color = new Color(0.75f, 0.55f, 1f, 0.85f);
                    break;
            }
        }

        private static string RoomLabel(RoomType type)
        {
            return type switch
            {
                RoomType.Start => "입구",
                RoomType.Treasure => "보물",
                RoomType.Event => "성소",
                RoomType.Boss => "보스",
                RoomType.Exit => "출구",
                RoomType.Combat => "전투",
                _ => "탐험"
            };
        }

        private HashSet<int> BuildAdjacentSet()
        {
            var set = new HashSet<int>();
            if (_current == null || _gen == null)
            {
                return set;
            }

            void TryAdd(DoorDir dir)
            {
                var r = _gen.GetNeighbor(_current, dir);
                if (r != null)
                {
                    set.Add(r.Id);
                }
            }

            if (_current.ConnectedNorth)
            {
                TryAdd(DoorDir.North);
            }

            if (_current.ConnectedSouth)
            {
                TryAdd(DoorDir.South);
            }

            if (_current.ConnectedEast)
            {
                TryAdd(DoorDir.East);
            }

            if (_current.ConnectedWest)
            {
                TryAdd(DoorDir.West);
            }

            return set;
        }

        private void BuildTownMap()
        {
            if (_root == null)
            {
                return;
            }

            MakeImage("TownBg", Vector2.zero, new Vector2(MapHalfW * 2f + 8f, MapHalfH * 2f + 8f),
                new Color(0.04f, 0.08f, 0.07f, 0.96f));

            MakeImage("Plaza", new Vector2(-8f, 4f), new Vector2(78f, 52f),
                new Color(0.22f, 0.34f, 0.28f, 0.85f));
            MakeImage("PathH", new Vector2(20f, 0f), new Vector2(108f, 10f),
                new Color(0.32f, 0.42f, 0.36f, 0.8f));
            MakeImage("PathV", new Vector2(-8f, -10f), new Vector2(10f, 54f),
                new Color(0.32f, 0.42f, 0.36f, 0.8f));

            PlaceTownPoi("Guide", new Vector3(-2.2f, 0f, 2.2f), "G", AccentMint);
            PlaceTownPoi("Shop", new Vector3(-10.5f, 0f, -2.2f), "S", AccentGold);
            PlaceTownPoi("Dungeon", new Vector3(20.5f, 0f, 0.5f), "D", AccentCoral);

            var playerGo = new GameObject("Player");
            playerGo.transform.SetParent(_root, false);
            _playerDot = playerGo.AddComponent<RectTransform>();
            _playerDot.sizeDelta = new Vector2(16f, 16f);
            _playerDot.anchoredPosition = _player != null ? WorldToMap(_player.position) : Vector2.zero;

            _playerPulse = MakeChildImage(playerGo.transform, "Pulse", Vector2.zero, new Vector2(20f, 20f),
                new Color(0.4f, 0.95f, 0.85f, 0.4f));
            _playerCore = MakeChildImage(playerGo.transform, "Core", Vector2.zero, new Vector2(9f, 9f),
                AccentCyan);
        }

        private void PlaceTownPoi(string name, Vector3 world, string mark, Color accent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = WorldToMap(world);
            rt.sizeDelta = new Vector2(22f, 22f);

            MakeChildImage(go.transform, "Glow", Vector2.zero, new Vector2(24f, 24f),
                new Color(accent.r, accent.g, accent.b, 0.3f));
            MakeChildImage(go.transform, "Node", Vector2.zero, new Vector2(18f, 18f),
                new Color(0.1f, 0.14f, 0.13f, 0.95f));
            MakeChildImage(go.transform, "Ring", Vector2.zero, new Vector2(20f, 20f),
                new Color(accent.r, accent.g, accent.b, 0.75f));
            var label = MakeChildText(go.transform, "L", mark, 12, Vector2.zero, new Vector2(18f, 16f), accent);
            label.fontStyle = FontStyle.Bold;
        }

        private Vector2 WorldToMap(Vector3 world)
        {
            var nx = Mathf.InverseLerp(TownWorldMin.x, TownWorldMax.x, world.x);
            var ny = Mathf.InverseLerp(TownWorldMin.y, TownWorldMax.y, world.z);
            return new Vector2(Mathf.Lerp(-MapHalfW + 10f, MapHalfW - 10f, nx),
                Mathf.Lerp(-MapHalfH + 8f, MapHalfH - 8f, ny));
        }

        private Vector2 CellPos(Vector2Int gp, float cx, float cy)
        {
            return new Vector2((gp.x - cx) * _pitch, (gp.y - cy) * _pitch);
        }

        private Image MakeImage(string name, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        private static Image MakeChildImage(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        private static Text MakeChildText(Transform parent, string name, string content, int size, Vector2 pos,
            Vector2 rect, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = rect;
            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = RuntimeUiFactory.UiFont();

            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }
    }
}
