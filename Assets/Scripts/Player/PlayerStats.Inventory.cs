using System;
using System.Collections.Generic;
using DungeonOdyssey.Core;
using DungeonOdyssey.Save;
using DungeonOdyssey.UI;
using UnityEngine;

namespace DungeonOdyssey.Player
{
    public partial class PlayerStats
    {
        private InvSlot[] _inventory = Array.Empty<InvSlot>();
        private int _bagCapacity = InventoryRules.BaseSlots;
        private int _equippedInvSlot;

        public int BagCapacity => _bagCapacity;
        public int EquippedInvSlot => _equippedInvSlot;
        public InvSlot[] Inventory => _inventory;

        public InvSlot EquippedInv =>
            IsValidSlot(_equippedInvSlot) ? _inventory[_equippedInvSlot] : null;

        public int UsedBagSlots
        {
            get
            {
                var n = 0;
                for (var i = 0; i < _inventory.Length; i++)
                {
                    if (_inventory[i] != null && !_inventory[i].IsEmpty)
                    {
                        n++;
                    }
                }

                return n;
            }
        }

        public int EmptyBagSlots => Mathf.Max(0, _bagCapacity - UsedBagSlots);

        public bool IsValidSlot(int index) =>
            index >= 0 && index < _inventory.Length && _inventory[index] != null;

        public InvSlot GetInvSlot(int index) =>
            IsValidSlot(index) ? _inventory[index] : InvSlot.Empty();

        private void LoadInventoryFromSave(SaveData save)
        {
            _bagCapacity = Mathf.Max(
                InventoryRules.CapacityForLevel(level),
                save.bagCapacity > 0 ? save.bagCapacity : InventoryRules.BaseSlots);
            EnsureInventoryCapacity(_bagCapacity, preserveContents: false);
            var src = save.inventory;
            if (src == null || src.Length == 0)
            {
                _inventory[0] = InvSlot.MakeWeapon(WeaponId.IronSword);
                _inventory[1] = InvSlot.MakePotion(Mathf.Max(1, save.potions > 0 ? save.potions : 2));
                _equippedInvSlot = 0;
            }
            else
            {
                for (var i = 0; i < _bagCapacity; i++)
                {
                    _inventory[i] = i < src.Length && src[i] != null
                        ? src[i].Clone()
                        : InvSlot.Empty();
                }

                _equippedInvSlot = Mathf.Clamp(save.equippedInvSlot, 0, _bagCapacity - 1);
            }

            if (!IsWeaponSlot(_equippedInvSlot))
            {
                _equippedInvSlot = FindFirstWeaponSlot();
            }

            CompactInventory();
            SyncCombatFromEquippedSlot();
            RebuildOwnedMaskFromInventory();
            SyncConsumableScalars();
        }

        private void SaveInventoryToSave(SaveData save)
        {
            RebuildOwnedMaskFromInventory();
            SyncConsumableScalars();
            save.bagCapacity = _bagCapacity;
            save.equippedInvSlot = _equippedInvSlot;
            save.inventory = new InvSlot[_bagCapacity];
            for (var i = 0; i < _bagCapacity; i++)
            {
                save.inventory[i] = _inventory[i] != null ? _inventory[i].Clone() : InvSlot.Empty();
            }

            save.ownedWeaponsMask = ownedWeaponsMask;
            save.potions = potions;
        }

        private void EnsureInventoryCapacity(int capacity, bool preserveContents)
        {
            capacity = Mathf.Max(InventoryRules.BaseSlots, capacity);
            _bagCapacity = capacity;
            if (!preserveContents || _inventory == null || _inventory.Length == 0)
            {
                _inventory = new InvSlot[capacity];
                for (var i = 0; i < capacity; i++)
                {
                    _inventory[i] = InvSlot.Empty();
                }

                return;
            }

            if (_inventory.Length == capacity)
            {
                return;
            }

            var next = new InvSlot[capacity];
            for (var i = 0; i < capacity; i++)
            {
                next[i] = i < _inventory.Length && _inventory[i] != null
                    ? _inventory[i]
                    : InvSlot.Empty();
            }

            _inventory = next;
            _equippedInvSlot = Mathf.Clamp(_equippedInvSlot, 0, capacity - 1);
        }

        /// <summary>레벨업 시 가방 슬롯 확장. 증가분이 있으면 힌트.</summary>
        public int RefreshBagCapacityForLevel()
        {
            var target = InventoryRules.CapacityForLevel(level);
            var gained = target - _bagCapacity;
            if (gained <= 0)
            {
                return 0;
            }

            EnsureInventoryCapacity(target, preserveContents: true);
            FindFirstObjectByType<HudUI>()?.SetHint(
                $"가방 확장! 슬롯 +{gained}  (총 {_bagCapacity}칸)");
            return gained;
        }

        private void RebuildOwnedMaskFromInventory()
        {
            var mask = 0;
            for (var i = 0; i < _inventory.Length; i++)
            {
                var s = _inventory[i];
                if (s != null && s.Kind == InvItemKind.Weapon)
                {
                    mask = WeaponCatalog.WithOwned(mask, s.Weapon);
                }
            }

            if (mask == 0)
            {
                // 안전장치: 빈 가방이면 철검 1개
                if (_inventory.Length > 0)
                {
                    _inventory[0] = InvSlot.MakeWeapon(WeaponId.IronSword);
                    _equippedInvSlot = 0;
                }

                mask = WeaponCatalog.WithOwned(0, WeaponId.IronSword);
            }

            ownedWeaponsMask = mask;
        }

        private void SyncConsumableScalars()
        {
            potions = CountKind(InvItemKind.Potion);
            var luck = CountKind(InvItemKind.Luck);
            var save = GameManager.Instance?.CurrentSave;
            if (save != null)
            {
                save.luckCharges = luck;
            }
        }

        private int CountKind(InvItemKind kind)
        {
            var n = 0;
            for (var i = 0; i < _inventory.Length; i++)
            {
                var s = _inventory[i];
                if (s != null && s.Kind == kind)
                {
                    n += Mathf.Max(0, s.count);
                }
            }

            return n;
        }

        public bool IsWeaponInInventory(WeaponId id)
        {
            for (var i = 0; i < _inventory.Length; i++)
            {
                if (IsWeaponSlot(i) && _inventory[i].Weapon == id)
                {
                    return true;
                }
            }

            return false;
        }

        public int CountItem(InvItemKind kind) => CountKind(kind);

        public bool TryConsumeLuck(int amount)
        {
            if (!TryConsumeKind(InvItemKind.Luck, amount))
            {
                return false;
            }

            SyncToSave();
            RaiseMeta();
            return true;
        }

        public bool IsWeaponSlot(int index)
        {
            var s = GetInvSlot(index);
            return s.Kind == InvItemKind.Weapon;
        }

        private int FindFirstWeaponSlot()
        {
            for (var i = 0; i < _inventory.Length; i++)
            {
                if (IsWeaponSlot(i))
                {
                    return i;
                }
            }

            return 0;
        }

        private int FindEmptySlot()
        {
            for (var i = 0; i < _inventory.Length; i++)
            {
                if (_inventory[i] == null || _inventory[i].IsEmpty)
                {
                    return i;
                }
            }

            return -1;
        }

        private int FindStackSlot(InvItemKind kind)
        {
            var max = InventoryRules.MaxStack(kind);
            for (var i = 0; i < _inventory.Length; i++)
            {
                var s = _inventory[i];
                if (s != null && s.Kind == kind && s.count < max)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>빈 칸 또는 스택에 아이템 추가. 가방 가득이면 false.</summary>
        public bool TryAddInvItem(InvSlot item, out string message)
        {
            if (item == null || item.IsEmpty)
            {
                message = "잘못된 아이템이다.";
                return false;
            }

            if (item.Kind == InvItemKind.Potion || item.Kind == InvItemKind.Luck)
            {
                var stack = FindStackSlot(item.Kind);
                if (stack >= 0)
                {
                    var max = InventoryRules.MaxStack(item.Kind);
                    var room = max - _inventory[stack].count;
                    var add = Mathf.Min(room, item.count);
                    _inventory[stack].count += add;
                    item.count -= add;
                    if (item.count <= 0)
                    {
                        AfterInventoryMutate();
                        message = $"{InventoryRules.SlotLabel(_inventory[stack])} 획득";
                        return true;
                    }
                }
            }

            var empty = FindEmptySlot();
            if (empty < 0)
            {
                message = $"가방이 가득 찼다. ({UsedBagSlots}/{_bagCapacity})";
                return false;
            }

            _inventory[empty] = item.Clone();
            AfterInventoryMutate();
            message = $"{InventoryRules.SlotLabel(_inventory[empty])} 가방에 넣었다.";
            return true;
        }

        public bool TryEquipInvSlot(int index, out string message)
        {
            if (!IsWeaponSlot(index))
            {
                message = "장착할 수 있는 무기가 아니다.";
                return false;
            }

            if (index == _equippedInvSlot)
            {
                message = $"{WeaponLabel} 이미 장착 중";
                return true;
            }

            _equippedInvSlot = index;
            SyncCombatFromEquippedSlot();
            ApplyWeaponVisual();
            AfterInventoryMutate();
            message = $"{WeaponLabel}  (+{WeaponBonus})  ATK {AttackPower}  · 가방 {index + 1}/{_bagCapacity}";
            return true;
        }

        private void SyncCombatFromEquippedSlot()
        {
            var slot = EquippedInv;
            if (slot == null || slot.Kind != InvItemKind.Weapon)
            {
                equippedWeaponId = (int)WeaponId.IronSword;
                weaponUpgradeLevel = 0;
                equippedSkillId = (int)SkillId.DashSlash;
                return;
            }

            equippedWeaponId = slot.weaponId;
            EnsureWeaponArrays();
            _weaponUpgrades[equippedWeaponId] = slot.upgrade;
            _weaponTempers[equippedWeaponId] = slot.temper;
            if (slot.HasSigil)
            {
                weaponSigilMask = WeaponCatalog.WithSigil(weaponSigilMask, slot.Weapon);
            }
            else
            {
                weaponSigilMask = WeaponCatalog.WithoutSigil(weaponSigilMask, slot.Weapon);
            }

            weaponUpgradeLevel = slot.upgrade;
            // 스킬 오버라이드가 없으면 장착 무기 스킬을 따름
            if (SkillFollowsWeapon)
            {
                equippedSkillId = (int)SkillCatalog.ForWeapon(slot.Weapon);
            }

            ValidateSkillSource();
        }

        private void WriteEquippedSlotFromArrays()
        {
            if (!IsWeaponSlot(_equippedInvSlot))
            {
                return;
            }

            var s = _inventory[_equippedInvSlot];
            s.upgrade = GetUpgrade(s.Weapon);
            s.temper = GetTemper(s.Weapon);
            s.sigil = HasSigilOf(s.Weapon) ? 1 : 0;
        }

        private void WriteSlotWeaponStats(int index)
        {
            if (!IsWeaponSlot(index))
            {
                return;
            }

            var s = _inventory[index];
            EnsureWeaponArrays();
            _weaponUpgrades[s.weaponId] = s.upgrade;
            _weaponTempers[s.weaponId] = s.temper;
            if (s.HasSigil)
            {
                weaponSigilMask = WeaponCatalog.WithSigil(weaponSigilMask, s.Weapon);
            }
        }

        private void AfterInventoryMutate()
        {
            CompactInventory();
            RebuildOwnedMaskFromInventory();
            SyncConsumableScalars();
            ValidateSkillSource();
            SyncToSave();
            RaiseMeta();
            _skill?.RefreshFromStats();
        }

        public int CountWeaponSlots()
        {
            var n = 0;
            for (var i = 0; i < _inventory.Length; i++)
            {
                if (IsWeaponSlot(i))
                {
                    n++;
                }
            }

            return n;
        }

        public int NextWeaponInvSlot(int from, int direction)
        {
            if (CountWeaponSlots() <= 1)
            {
                return from;
            }

            var dir = direction < 0 ? -1 : 1;
            var i = from;
            for (var step = 0; step < _inventory.Length; step++)
            {
                i = (i + dir + _inventory.Length) % _inventory.Length;
                if (IsWeaponSlot(i))
                {
                    return i;
                }
            }

            return from;
        }

        /// <summary>슬롯 기준 합성 (인스턴스 소모).</summary>
        public bool TrySynthesizeInvSlots(int slotA, int slotB, out string message, out WeaponId? gained)
        {
            gained = null;
            if (!IsWeaponSlot(slotA) || !IsWeaponSlot(slotB) || slotA == slotB)
            {
                message = "서로 다른 무기 슬롯 두 개가 필요하다.";
                return false;
            }

            var a = _inventory[slotA];
            var b = _inventory[slotB];
            var idA = a.Weapon;
            var idB = b.Weapon;
            var ra = WeaponCatalog.GetRarity(idA);
            var rb = WeaponCatalog.GetRarity(idB);
            if (ra != rb)
            {
                message =
                    $"같은 등급끼리만 합성할 수 있다. ({WeaponCatalog.RarityName(ra)} ≠ {WeaponCatalog.RarityName(rb)})";
                return false;
            }

            if (ra >= WeaponRarity.Legendary)
            {
                message = "전설 등급은 더 이상 합성할 수 없다.";
                return false;
            }

            if (CountWeaponSlots() < 3)
            {
                message = "합성하려면 무기를 3개 이상 보유해야 한다. (재료 2개 소모)";
                return false;
            }

            var targetRarity = WeaponCatalog.NextRarity(ra);
            var wasEquipped = _equippedInvSlot == slotA || _equippedInvSlot == slotB;
            var upA = Mathf.Max(0, a.upgrade);
            var upB = Mathf.Max(0, b.upgrade);
            var temperA = Mathf.Max(0, a.temper);
            var temperB = Mathf.Max(0, b.temper);
            var combinedUpgrade = upA + upB;
            var combinedTemper = Mathf.Min(WeaponCatalog.MaxTemper, temperA + temperB);

            // 임시로 재료 제거 후 랜덤 결과 뽑기
            _inventory[slotA] = InvSlot.Empty();
            _inventory[slotB] = InvSlot.Empty();
            RebuildOwnedMaskFromInventory();

            gained = WeaponCatalog.RandomUnownedOfRarity(ownedWeaponsMask, targetRarity, level);
            if (!gained.HasValue)
            {
                gained = WeaponCatalog.RandomUnownedOfRarity(ownedWeaponsMask, targetRarity, 99);
            }

            // 미보유가 없으면 같은 등급 중복 인스턴스라도 생성
            var duplicate = false;
            if (!gained.HasValue)
            {
                gained = WeaponCatalog.RandomOfRarity(targetRarity, level)
                         ?? WeaponCatalog.RandomOfRarity(targetRarity, 99);
                duplicate = gained.HasValue;
            }

            var place = slotA; // 재료 자리에 결과 배치
            if (gained.HasValue)
            {
                var max = WeaponCatalog.MaxUpgradeAllowed(gained.Value, level);
                var finalUp = Mathf.Min(max, combinedUpgrade);
                _inventory[place] = InvSlot.MakeWeapon(gained.Value, finalUp, combinedTemper);
                if (wasEquipped)
                {
                    _equippedInvSlot = place;
                }

                CompactInventory();
                SyncCombatFromEquippedSlot();
                ApplyWeaponVisual();
                AfterInventoryMutate();
                var gdef = WeaponCatalog.Get(gained.Value);
                var levelNote = combinedUpgrade > 0
                    ? $"\n강화 합산 +{upA}+{upB} → +{finalUp}" +
                      (finalUp < combinedUpgrade ? $" (상한 +{max})" : "")
                    : "";
                var dupNote = duplicate ? "\n(이미 가진 등급 · 새 인스턴스)" : "";
                message =
                    $"합성 성공! [{WeaponCatalog.RarityName(ra)}] → [{WeaponCatalog.RarityName(targetRarity)}]\n" +
                    $"{WeaponCatalog.Get(idA).Name}+{upA} + {WeaponCatalog.Get(idB).Name}+{upB}\n" +
                    $"→ {gdef.Name}+{finalUp} 가방에 저장!" + levelNote + dupNote;
                return true;
            }

            var refund = 80 + (int)ra * 60 + (upA + upB) * 10;
            GameManager.Instance?.AddGold(refund);
            var boostSlot = FindFirstWeaponSlot();
            var boost = _inventory[boostSlot];
            var cur = boost.upgrade;
            var maxBoost = WeaponCatalog.MaxUpgradeAllowed(boost.Weapon, level);
            var inherit = Mathf.Max(3 + (int)ra, combinedUpgrade / 2);
            boost.upgrade = Mathf.Min(maxBoost, cur + inherit);
            if (wasEquipped)
            {
                _equippedInvSlot = boostSlot;
            }

            CompactInventory();
            SyncCombatFromEquippedSlot();
            ApplyWeaponVisual();
            AfterInventoryMutate();
            message =
                $"합성 재료 소모 · 해당 등급 무기 풀이 비어 있음\n" +
                $"보상: {refund}G + {WeaponCatalog.Get(boost.Weapon).Name} 강화 +{inherit}";
            gained = boost.Weapon;
            return true;
        }

        /// <summary>빈 칸을 뒤로 밀어 아이템이 앞에서부터 연속되게 정렬. 장착 슬롯도 함께 보정.</summary>
        public void CompactInventory()
        {
            if (_inventory == null || _inventory.Length == 0)
            {
                return;
            }

            var eqWeaponId = -1;
            var eqUpgrade = 0;
            var eqTemper = 0;
            var eqSigil = 0;
            if (IsWeaponSlot(_equippedInvSlot))
            {
                var eq = _inventory[_equippedInvSlot];
                eqWeaponId = eq.weaponId;
                eqUpgrade = eq.upgrade;
                eqTemper = eq.temper;
                eqSigil = eq.sigil;
            }

            var packed = new List<InvSlot>(_inventory.Length);
            for (var i = 0; i < _inventory.Length; i++)
            {
                var s = _inventory[i];
                if (s != null && !s.IsEmpty)
                {
                    packed.Add(s);
                }
            }

            for (var i = 0; i < _inventory.Length; i++)
            {
                _inventory[i] = i < packed.Count ? packed[i] : InvSlot.Empty();
            }

            if (eqWeaponId >= 0)
            {
                var found = false;
                for (var i = 0; i < packed.Count; i++)
                {
                    var s = _inventory[i];
                    if (s.Kind == InvItemKind.Weapon &&
                        s.weaponId == eqWeaponId &&
                        s.upgrade == eqUpgrade &&
                        s.temper == eqTemper &&
                        s.sigil == eqSigil)
                    {
                        _equippedInvSlot = i;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    _equippedInvSlot = FindFirstWeaponSlot();
                }
            }
            else if (!IsWeaponSlot(_equippedInvSlot))
            {
                _equippedInvSlot = FindFirstWeaponSlot();
            }
        }

        /// <summary>선택 슬롯만 강화 (동일 WeaponId 다른 인스턴스와 수치를 섞지 않음).</summary>
        public bool TryForgeInvSlot(int index, bool precision, out string message,
            out ForgeResult result, out int stepsGained) =>
            TryForgeInvSlot(index, precision, out message, out result, out stepsGained, announce: true);

        public bool TryForgeInvSlot(int index, bool precision, out string message,
            out ForgeResult result, out int stepsGained, bool announce)
        {
            result = ForgeResult.Fail;
            stepsGained = 0;
            if (!IsWeaponSlot(index))
            {
                message = "보유하지 않은 무기다.";
                return false;
            }

            var slot = _inventory[index];
            var id = slot.Weapon;
            var def = WeaponCatalog.Get(id);
            var current = Mathf.Max(0, slot.upgrade);
            var allowed = WeaponCatalog.MaxUpgradeAllowed(id, level);
            if (current >= allowed)
            {
                message = slot.HasSigil
                    ? $"{def.Name}은(는) 이미 레벨 상한(+{allowed})·각인 상태다."
                    : $"{def.Name}은(는) 현재 레벨 상한(+{allowed}). 레벨업 후 더 강화할 수 있다.";
                return false;
            }

            if (precision && slot.temper >= WeaponCatalog.MaxTemper)
            {
                message = $"템퍼가 이미 최대(⋆{WeaponCatalog.MaxTemper})다. 일반 강화를 사용하세요.";
                return false;
            }

            var cost = precision
                ? WeaponCatalog.PrecisionCost(current, level)
                : WeaponCatalog.UpgradeCost(current, level);
            if (GameManager.Instance == null || !GameManager.Instance.TrySpendGold(cost))
            {
                message = $"골드가 부족하다. ({(precision ? "정밀 " : "")}강화 {cost}G)";
                return false;
            }

            result = WeaponCatalog.RollForge(current, precision);
            var steps = WeaponCatalog.ForgeSteps(result);
            var room = allowed - current;
            stepsGained = Mathf.Min(steps, room);
            var beforeBonus = WeaponCatalog.GetWeaponBonus(id, current, slot.temper);
            slot.upgrade = current + stepsGained;
            if (precision)
            {
                slot.temper = Mathf.Min(WeaponCatalog.MaxTemper, slot.temper + 1);
            }

            if (index == _equippedInvSlot)
            {
                SyncCombatFromEquippedSlot();
                ApplyWeaponVisual();
            }

            SyncToSave();
            RaiseMeta();

            if (WeaponCatalog.IsFullyUpgraded(slot.upgrade, level))
            {
                AchievementCatalog.TryUnlock(AchievementId.FullForge, "최대 강화");
            }

            var afterBonus = WeaponCatalog.GetWeaponBonus(id, slot.upgrade, slot.temper);
            var tag = result switch
            {
                ForgeResult.Perfect => "✦ 완벽 강화!",
                ForgeResult.Great => "◆ 대성공!",
                _ => precision ? "정밀 강화" : "강화 성공"
            };
            var label = WeaponCatalog.Label(id, slot.upgrade, slot.temper, slot.HasSigil);
            message =
                $"{tag}  {label}  ·  +{current}→+{slot.upgrade} (+{stepsGained})  ·  " +
                $"보너스 +{beforeBonus}→+{afterBonus}  ·  ATK {AttackPower}" +
                (slot.temper > 0 ? $"  ·  템퍼 ⋆{slot.temper}" : "");

            if (announce)
            {
                FindFirstObjectByType<ClearBannerUI>()?.Show(
                    $"{tag}  {def.Name}+{slot.upgrade}",
                    result is ForgeResult.Perfect or ForgeResult.Great
                        ? ToastKind.Style
                        : ToastKind.Success,
                    1.8f);
            }

            return true;
        }

        /// <summary>한 슬롯을 여러 번 연속 강화. 상한·골드 부족 시 중단.</summary>
        public bool TryForgeInvSlotBatch(int index, bool precision, int attempts,
            out string message, out ForgeResult bestResult, out int totalSteps,
            out int attemptsDone, out int goldSpent)
        {
            bestResult = ForgeResult.Fail;
            totalSteps = 0;
            attemptsDone = 0;
            goldSpent = 0;
            attempts = Mathf.Clamp(attempts, 1, WeaponCatalog.AbsoluteMaxUpgrade);

            if (!IsWeaponSlot(index))
            {
                message = "보유하지 않은 무기다.";
                return false;
            }

            var slot = _inventory[index];
            var id = slot.Weapon;
            var def = WeaponCatalog.Get(id);
            var before = slot.upgrade;
            var beforeTemper = slot.temper;
            var beforeBonus = WeaponCatalog.GetWeaponBonus(id, before, beforeTemper);
            var goldStart = Gold;
            string lastFail = null;

            for (var i = 0; i < attempts; i++)
            {
                if (!TryForgeInvSlot(index, precision, out var stepMsg, out var stepResult,
                        out var steps, announce: false))
                {
                    lastFail = stepMsg;
                    break;
                }

                attemptsDone++;
                totalSteps += steps;
                if (stepResult == ForgeResult.Perfect ||
                    (stepResult == ForgeResult.Great && bestResult != ForgeResult.Perfect) ||
                    bestResult == ForgeResult.Fail)
                {
                    bestResult = stepResult;
                }
            }

            goldSpent = Mathf.Max(0, goldStart - Gold);
            slot = _inventory[index];
            var afterBonus = WeaponCatalog.GetWeaponBonus(id, slot.upgrade, slot.temper);

            if (attemptsDone <= 0)
            {
                message = lastFail ?? "강화할 수 없다.";
                return false;
            }

            var tag = bestResult switch
            {
                ForgeResult.Perfect => "✦ 완벽 포함!",
                ForgeResult.Great => "◆ 대성공 포함!",
                _ => precision ? "정밀 연속 강화" : "연속 강화"
            };
            var label = WeaponCatalog.Label(id, slot.upgrade, slot.temper, slot.HasSigil);
            message =
                $"{tag}  {label}  ·  +{before}→+{slot.upgrade} (+{totalSteps})  ·  " +
                $"{attemptsDone}회  ·  {goldSpent}G  ·  보너스 +{beforeBonus}→+{afterBonus}" +
                (slot.temper != beforeTemper ? $"  ·  템퍼 ⋆{beforeTemper}→⋆{slot.temper}" : "");

            FindFirstObjectByType<ClearBannerUI>()?.Show(
                $"{tag}  {def.Name}+{slot.upgrade}  ({attemptsDone}회)",
                bestResult is ForgeResult.Perfect or ForgeResult.Great
                    ? ToastKind.Style
                    : ToastKind.Success,
                2f);
            return true;
        }

        /// <summary>가방 슬롯 판매. 장착 중·마지막 무기는 보호.</summary>
        public bool TrySellInvSlot(int index, out string message, out int goldGained)
        {
            goldGained = 0;
            if (!IsValidSlot(index) || _inventory[index] == null || _inventory[index].IsEmpty)
            {
                message = "비어 있는 칸이다.";
                return false;
            }

            var slot = _inventory[index];
            if (slot.Kind == InvItemKind.Weapon)
            {
                if (CountWeaponSlots() <= 1)
                {
                    message = "마지막 무기는 팔 수 없다.";
                    return false;
                }

                if (index == _equippedInvSlot)
                {
                    message = "장착 중인 무기다. 다른 무기를 장착한 뒤 판매하세요.";
                    return false;
                }
            }

            goldGained = InventoryRules.SellPrice(slot);
            var label = InventoryRules.SlotLabel(slot);
            _inventory[index] = InvSlot.Empty();
            AfterInventoryMutate();
            if (goldGained > 0)
            {
                GameManager.Instance?.AddGold(goldGained);
            }

            message = $"{label} 판매  ·  +{goldGained}G";
            return true;
        }

        public int PreviewSellPrice(int index)
        {
            if (!IsValidSlot(index) || _inventory[index] == null)
            {
                return 0;
            }

            return InventoryRules.SellPrice(_inventory[index]);
        }

        /// <summary>가방 가득 여부만 검사하는 구매 전 체크.</summary>
        public bool HasBagSpaceForNewItem(InvItemKind kind)
        {
            if (kind == InvItemKind.Potion || kind == InvItemKind.Luck)
            {
                if (FindStackSlot(kind) >= 0)
                {
                    return true;
                }
            }

            return FindEmptySlot() >= 0;
        }
    }
}
