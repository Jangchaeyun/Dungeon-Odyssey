using System.Collections.Generic;
using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Dungeon;
using DungeonOdyssey.Player;
using DungeonOdyssey.UI;
using UnityEngine;

namespace DungeonOdyssey.Town
{
    public class ShopInteractable : MonoBehaviour
    {
        [SerializeField] private DialogueUI dialogueUI;
        private ForgeShopUI _forge;
        private bool _playerInside;
        private PlayerStats _player;
        private WorldLabel _label;
        private readonly List<ShopAction> _actions = new();
        private bool _shopSessionOpen;

        public bool IsShopUiOpen
        {
            get
            {
                if (_shopSessionOpen && (_forge == null || !_forge.IsOpen))
                {
                    _shopSessionOpen = false;
                }

                return _shopSessionOpen;
            }
        }

        private struct ShopAction
        {
            public string Kind; // heal, potion, forge, precision, sigil, skill, weapon, luck
            public WeaponId Weapon;
        }

        public void Configure(DialogueUI ui, WorldLabel label = null)
        {
            dialogueUI = ui;
            _label = label;
        }

        private void Update()
        {
            if (!_playerInside || GameUi.IsPaused || _player == null)
            {
                return;
            }

            if (_forge != null && _forge.IsOpen)
            {
                return;
            }

            if (GameUi.IsBlocking)
            {
                return;
            }

            if (GameInput.InteractDown)
            {
                OpenShop();
            }
        }

        /// <summary>상단 상점 아이콘 / E 상호작용에서 호출.</summary>
        public void OpenShop()
        {
            EnsureForge();
            EnsurePlayer();
            if (_forge == null)
            {
                dialogueUI ??= FindFirstObjectByType<DialogueUI>();
                CombatAudio.ShopOpen();
                dialogueUI?.Show("대장장이 · Borin", BuildLegacyCatalogText());
                return;
            }

            _shopSessionOpen = true;
            RefreshForge();
        }

        /// <summary>상단 아이콘 토글 — 열려 있으면 닫고, 아니면 연다.</summary>
        public void ToggleFromUi()
        {
            EnsureForge();
            if (_forge != null && _forge.IsOpen && _shopSessionOpen)
            {
                _shopSessionOpen = false;
                _forge.Hide();
                return;
            }

            OpenShop();
        }

        private void RefreshForge()
        {
            EnsureForge();
            if (_forge == null)
            {
                return;
            }

            BuildShopContent(out var status, out var rows);
            _forge.Show("대장간 · Borin", status, rows, OnForgeSelect);
        }

        private void OnForgeSelect(int index)
        {
            var i = index - 1;
            if (i < 0 || i >= _actions.Count)
            {
                return;
            }

            var act = _actions[i];
            switch (act.Kind)
            {
                case "heal":
                    BuyHeal();
                    break;
                case "potion":
                    BuyPotion();
                    break;
                case "luck":
                    BuyLuckCharm();
                    break;
                case "forge":
                    UpgradeWeapon();
                    break;
                case "precision":
                    PrecisionForge();
                    break;
                case "sigil":
                    ForgeSpecialOrWeapon();
                    break;
                case "skill":
                    BuySkill();
                    break;
                case "weapon":
                    BuyWeapon(act.Weapon);
                    break;
            }
        }

        private void BuyWeapon(WeaponId id)
        {
            EnsurePlayer();
            if (_player == null)
            {
                Fail("상점을 이용할 수 없다.");
                return;
            }

            if (_player.TryBuyWeapon(id, out var msg))
            {
                FinishBuy(msg);
                CombatAudio.Coin();
            }
            else
            {
                Fail(msg);
            }
        }

        private void BuyLuckCharm()
        {
            EnsurePlayer();
            if (_player == null)
            {
                Fail("상점을 이용할 수 없다.");
                return;
            }

            if (!_player.HasBagSpaceForNewItem(InvItemKind.Luck))
            {
                Fail($"가방이 가득 찼다. ({_player.UsedBagSlots}/{_player.BagCapacity})");
                return;
            }

            if (!TryPay(50, "골드가 부족하다. (행운 부적 50G)"))
            {
                return;
            }

            if (!_player.TryAddInvItem(InvSlot.MakeLuck(1), out var msg))
            {
                GameManager.Instance?.AddGold(50);
                Fail(msg);
                return;
            }

            FinishBuy($"행운 부적을 가방에 넣었다. (보유 ×{_player.CountItem(InvItemKind.Luck)})");
            CombatAudio.Coin();
        }

        private void BuildShopContent(out string status,
            out List<(string label, bool enabled, Color? tint, WeaponRarity? rarity, string iconKind, WeaponId weapon)>
                rows)
        {
            EnsurePlayer();
            _actions.Clear();
            rows = new List<(string, bool, Color?, WeaponRarity?, string, WeaponId)>();

            var gold = GameManager.Instance != null ? GameManager.Instance.CurrentSave.gold : 0;
            var atk = _player != null ? _player.AttackPower : 0;
            var pot = _player != null ? _player.Potions : 0;
            var hp = _player != null ? $"{_player.Health.CurrentHp}/{_player.Health.MaxHp}" : "-";
            var def = _player != null ? _player.EquippedWeaponDef : WeaponCatalog.Get(WeaponId.IronSword);
            var up = _player != null ? _player.WeaponUpgradeLevel : 0;
            var allowed = _player != null
                ? WeaponCatalog.MaxUpgradeAllowed(_player.EquippedWeapon, _player.Level)
                : 1;
            var upCost = _player != null ? WeaponCatalog.UpgradeCost(up, _player.Level) : 0;
            var precCost = _player != null ? WeaponCatalog.PrecisionCost(up, _player.Level) : 0;
            var sigilCost = _player != null ? WeaponCatalog.SigilCost(_player.EquippedWeapon, _player.Level) : 0;
            var lv = _player != null ? _player.Level : 1;
            var weapon = _player != null ? _player.WeaponLabel : "-";
            var rarity = _player != null
                ? WeaponCatalog.RarityName(WeaponCatalog.GetRarity(_player.EquippedWeapon))
                : "일반";
            var bless = _player != null ? _player.MetaBlessing : 0;
            var luck = GameManager.Instance?.CurrentSave?.luckCharges ?? 0;
            var bag = _player != null
                ? $"가방 {_player.UsedBagSlots}/{_player.BagCapacity}"
                : "";

            status =
                $"장착  [{rarity}]  {weapon}\n" +
                $"ATK {atk}  ·  Lv {lv}  ·  축복 +{bless}  ·  {gold}G  ·  HP {hp}  ·  포션 ×{pot}" +
                (luck > 0 ? $"  ·  행운 ×{luck}" : "") +
                (string.IsNullOrEmpty(bag) ? "" : $"  ·  {bag}") +
                "\n가방(B) · 인벤 강화·합성   |   대장간 · 장착 연마·각인·구매";

            // —— 아이템 (은은한 틴트) ——
            AddRow(rows, "heal", default, $"회복 키트  ·  15G", gold >= 15,
                new Color(0.22f, 0.32f, 0.24f, 0.9f), null);
            AddRow(rows, "potion", default, $"회복 포션  ·  20G", gold >= 20,
                new Color(0.2f, 0.34f, 0.26f, 0.9f), null);
            AddRow(rows, "luck", default, $"행운 부적  ·  50G", gold >= 50,
                new Color(0.28f, 0.22f, 0.34f, 0.9f), null);

            // —— 장착 무기 연마 (가방은 인벤 슬롯 강화·합성) ——
            if (_player != null && up >= allowed)
            {
                AddRow(rows, "forge", default, $"장착 연마  ·  완료 (+{allowed})", false,
                    new Color(0.22f, 0.16f, 0.12f, 0.85f), null);
                AddRow(rows, "precision", default,
                    _player.HasWeaponSigil ? "장착 정밀  ·  완료" : "장착 정밀  ·  상한", false,
                    new Color(0.22f, 0.16f, 0.12f, 0.85f), null);
            }
            else
            {
                AddRow(rows, "forge", default, $"장착 연마  ·  {upCost}G", gold >= upCost,
                    new Color(0.28f, 0.2f, 0.12f, 0.9f), null);
                AddRow(rows, "precision", default, $"장착 정밀  ·  {precCost}G", gold >= precCost,
                    new Color(0.3f, 0.22f, 0.12f, 0.9f), null);
            }

            if (_player != null && up >= allowed && !_player.HasWeaponSigil)
            {
                AddRow(rows, "sigil", default, $"각인 『{def.SigilName}』  ·  {sigilCost}G",
                    gold >= sigilCost, new Color(0.32f, 0.24f, 0.12f, 0.9f), null);
            }

            // 스킬은 무기에 귀속 — 상점 스킬 판매 없음

            // —— 무기 판매 (등급·가격 낮은 순 → 높은 순) ——
            if (_player != null)
            {
                var weaponIds = new List<WeaponId>(WeaponCatalog.WeaponCount);
                for (var wi = 0; wi < WeaponCatalog.WeaponCount; wi++)
                {
                    weaponIds.Add((WeaponId)wi);
                }

                weaponIds.Sort((a, b) =>
                {
                    var ra = (int)WeaponCatalog.GetRarity(a);
                    var rb = (int)WeaponCatalog.GetRarity(b);
                    if (ra != rb)
                    {
                        return ra.CompareTo(rb);
                    }

                    var pa = WeaponCatalog.ShopPrice(a);
                    var pb = WeaponCatalog.ShopPrice(b);
                    if (pa != pb)
                    {
                        return pa.CompareTo(pb);
                    }

                    return WeaponCatalog.Get(a).UnlockLevel.CompareTo(WeaponCatalog.Get(b).UnlockLevel);
                });

                foreach (var id in weaponIds)
                {
                    var wdef = WeaponCatalog.Get(id);
                    var wr = WeaponCatalog.GetRarity(id);
                    var price = WeaponCatalog.ShopPrice(id);
                    var unlocked = WeaponCatalog.IsUnlocked(id, _player.Level);
                    var bagFull = !_player.HasBagSpaceForNewItem(InvItemKind.Weapon);
                    var label = !unlocked
                        ? $"{wdef.Name}  ·  Lv{wdef.UnlockLevel}"
                        : bagFull
                            ? $"{wdef.Name}  ·  가방 가득"
                            : $"{wdef.Name}  ·  {price}G";
                    AddRow(rows, "weapon", id, label, unlocked && !bagFull && gold >= price,
                        WeaponCatalog.RarityBg(wr), wr);
                }
            }
        }

        private void AddRow(
            List<(string label, bool enabled, Color? tint, WeaponRarity? rarity, string iconKind, WeaponId weapon)> rows,
            string kind, WeaponId weapon, string label, bool enabled, Color? tint, WeaponRarity? rarity)
        {
            _actions.Add(new ShopAction { Kind = kind, Weapon = weapon });
            rows.Add((label, enabled, tint, rarity, kind, weapon));
        }

        /// <summary>ForgeShopUI 없을 때 폴백 (레거시 대화창).</summary>
        private string BuildLegacyCatalogText()
        {
            BuildShopContent(out var status, out var actions);
            var sb = status + "\n\n";
            for (var i = 0; i < actions.Count; i++)
            {
                sb += $"[{i + 1}] {actions[i].label}\n";
            }

            return sb.TrimEnd();
        }

        private void BuySkill()
        {
            EnsurePlayer();
            if (_player == null)
            {
                Fail("상점을 이용할 수 없다.");
                return;
            }

            if (_player.TryBuySkill(out var msg))
            {
                FinishBuy(msg);
                CombatAudio.Coin();
            }
            else
            {
                Fail(msg);
            }
        }

        private void BuyHeal()
        {
            if (!TryPay(15, "골드가 부족하다. (회복 15G)"))
            {
                return;
            }

            _player.FullHeal();
            FloatingText.Heal(_player.transform.position + Vector3.up * 1.4f, _player.Health.MaxHp);
            FinishBuy("상처가 아물었다. 체력이 회복됐다.");
            CombatAudio.Heal();
        }

        private void BuyPotion()
        {
            EnsurePlayer();
            if (_player == null)
            {
                Fail("상점을 이용할 수 없다.");
                return;
            }

            if (!_player.HasBagSpaceForNewItem(InvItemKind.Potion))
            {
                Fail($"가방이 가득 찼다. ({_player.UsedBagSlots}/{_player.BagCapacity})");
                return;
            }

            if (!TryPay(20, "골드가 부족하다. (포션 20G)"))
            {
                return;
            }

            if (!_player.TryAddInvItem(InvSlot.MakePotion(1), out var msg))
            {
                GameManager.Instance?.AddGold(20);
                Fail(msg);
                return;
            }

            FinishBuy($"포션을 가방에 넣었다. (보유 {_player.Potions}개)");
            CombatAudio.Coin();
        }

        private void UpgradeWeapon()
        {
            EnsurePlayer();
            if (_player == null)
            {
                Fail("상점을 이용할 수 없다.");
                return;
            }

            if (_player.TryUpgradeWeapon(out var msg))
            {
                QuestCatalog.NotifyUpgrade();
                FindFirstObjectByType<HudUI>()?.RefreshQuest();
                FinishBuy(msg);
                CombatAudio.Coin();
            }
            else
            {
                Fail(msg);
            }
        }

        private void PrecisionForge()
        {
            EnsurePlayer();
            if (_player == null)
            {
                Fail("상점을 이용할 수 없다.");
                return;
            }

            if (_player.TryPrecisionForge(out var msg))
            {
                QuestCatalog.NotifyUpgrade();
                FindFirstObjectByType<HudUI>()?.RefreshQuest();
                FinishBuy(msg);
                CombatAudio.LevelUp();
            }
            else
            {
                Fail(msg);
            }
        }

        private void ForgeSpecialOrWeapon()
        {
            EnsurePlayer();
            if (_player == null)
            {
                Fail("상점을 이용할 수 없다.");
                return;
            }

            var allowed = WeaponCatalog.MaxUpgradeAllowed(_player.EquippedWeapon, _player.Level);
            if (_player.WeaponUpgradeLevel >= allowed && !_player.HasWeaponSigil)
            {
                if (_player.TryInscribeSigil(out var sigilMsg))
                {
                    FinishBuy(sigilMsg);
                }
                else
                {
                    Fail(sigilMsg);
                }

                return;
            }

            if (_player.TryBuyOrCycleWeapon(out var msg))
            {
                FinishBuy(msg);
                CombatAudio.Coin();
            }
            else
            {
                Fail(msg);
            }
        }

        private bool TryPay(int cost, string failMsg)
        {
            EnsurePlayer();
            if (_player == null || GameManager.Instance == null)
            {
                Fail("상점을 이용할 수 없다.");
                return false;
            }

            if (!GameManager.Instance.TrySpendGold(cost))
            {
                Fail(failMsg);
                return false;
            }

            return true;
        }

        private void FinishBuy(string message)
        {
            _player.NotifyMetaChanged();
            var oneLine = message;
            var nl = message.IndexOf('\n');
            if (nl > 0)
            {
                oneLine = message.Substring(0, nl).Trim();
            }

            FindFirstObjectByType<ClearBannerUI>()?.Show(oneLine, ToastKind.Success, 1.5f);
            if (_forge != null && _forge.IsOpen)
            {
                RefreshForge();
            }
            else
            {
                OpenShop();
            }
        }

        private void Fail(string msg)
        {
            CombatAudio.Locked();
            FindFirstObjectByType<ClearBannerUI>()?.Show(msg, ToastKind.Danger, 1.6f);
            if (_forge != null && _forge.IsOpen)
            {
                RefreshForge();
            }
        }

        private void EnsureForge()
        {
            if (_forge == null)
            {
                _forge = FindFirstObjectByType<ForgeShopUI>();
            }
        }

        private void EnsurePlayer()
        {
            if (_player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                _player = p != null ? p.GetComponent<PlayerStats>() : null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _playerInside = true;
            EnsurePlayer();
            _label?.SetVisible(true);
            _label?.SetHighlight(true);
            FindFirstObjectByType<HudUI>()?.SetHint("상단 [상점] 아이콘 또는 E — Borin 대장간");
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _playerInside = false;
            _label?.SetHighlight(false);
            _label?.SetVisible(false);
            if (_forge != null && _forge.IsOpen)
            {
                _forge.Hide();
            }

            FindFirstObjectByType<HudUI>()?.SetHint("WASD 이동 · Space 공격 · R 스킬 · V 무기 · Q 포션 · E 대화");
        }
    }
}
