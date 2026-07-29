using System;
using UnityEngine;

namespace DungeonOdyssey.Save
{
    [Serializable]
    public class SaveData
    {
        public int slotIndex = 0;
        public string playerName = "Adventurer";
        /// <summary>로컬 계정 아이디 (로그인 시 기록).</summary>
        public string accountUsername = "";
        public int level = 1;
        public int experience = 0;
        public int experienceToNext = 20;
        public int maxHp = 100;
        public int currentHp = 100;
        public int attackPower = 10;
        public int equippedWeaponId = 0;
        public int weaponUpgradeLevel = 0; // legacy — 장착 무기 슬롯으로 이관
        public int[] weaponUpgrades;
        public int[] weaponTempers;
        public int weaponSigilMask;
        public int ownedWeaponsMask = 1;
        public int equippedSkillId = 0;
        /// <summary>-1이면 장착 무기 스킬을 따름. 0 이상이면 해당 무기 스킬을 R로 사용.</summary>
        public int skillSourceWeaponId = -1;
        public int ownedSkillsMask = 3; // 돌진 + 회오리
        public int metaBlessing;
        public int gold = 30;
        public int potions = 2;
        public int dungeonClears = 0;
        public int deepestDepth = 1;
        public int achievementMask;
        public int activeQuestId = -1;
        public int questProgress;
        public int completedQuestMask;
        public int dailyQuestId;
        public int dailyQuestDay;
        public int dailyProgress;
        public bool dailyClaimed;
        public int bestKillStreak;
        public int luckCharges;
        public bool hasQuestPrefs;
        /// <summary>true면 의뢰는 미라 수락만 유효. false면 구버전 자동수락 의뢰를 한 번 비움.</summary>
        public bool hasManualQuestGate;
        public float moveSpeedBonus = 0f;
        public float critChanceBonus = 0f;
        public float masterVolume = 0.8f;
        public float sfxVolume = 1f;
        public float shakeIntensity = 1f;
        /// <summary>구버전 세이브 마이그레이션용 — false면 SFX/셰이크 기본값 적용.</summary>
        public bool hasAudioPrefs;
        public bool tutorialDone;
        public string lastScene = "Town";
        public string lastSavedAtUtc = "";
        public float playTimeSeconds = 0f;

        /// <summary>가방 슬롯 (Empty 포함 고정 길이 = bagCapacity).</summary>
        public Player.InvSlot[] inventory;
        public int bagCapacity = 10;
        public int equippedInvSlot;

        public static SaveData CreateNew(int slotIndex = 0)
        {
            return new SaveData
            {
                slotIndex = slotIndex,
                playerName = "Adventurer",
                accountUsername = "",
                level = 1,
                experience = 0,
                experienceToNext = 20,
                maxHp = 100,
                currentHp = 100,
                attackPower = 10,
                equippedWeaponId = 0,
                weaponUpgradeLevel = 0,
                weaponUpgrades = new int[6],
                weaponTempers = new int[6],
                weaponSigilMask = 0,
                ownedWeaponsMask = 1,
                equippedSkillId = 0,
                skillSourceWeaponId = -1,
                ownedSkillsMask = 3, // 돌진 + 회오리
                metaBlessing = 0,
                gold = 30,
                potions = 2,
                dungeonClears = 0,
                deepestDepth = 1,
                achievementMask = 0,
                activeQuestId = -1,
                questProgress = 0,
                completedQuestMask = 0,
                dailyQuestId = 0,
                dailyQuestDay = 0,
                dailyProgress = 0,
                dailyClaimed = false,
                bestKillStreak = 0,
                luckCharges = 0,
                hasQuestPrefs = true,
                hasManualQuestGate = true,
                moveSpeedBonus = 0f,
                critChanceBonus = 0f,
                masterVolume = 0.8f,
                sfxVolume = 1f,
                shakeIntensity = 1f,
                hasAudioPrefs = true,
                tutorialDone = false,
                lastScene = "Town",
                lastSavedAtUtc = DateTime.UtcNow.ToString("o"),
                playTimeSeconds = 0f,
                bagCapacity = Player.InventoryRules.BaseSlots,
                equippedInvSlot = 0,
                inventory = CreateStarterInventory()
            };
        }

        private static Player.InvSlot[] CreateStarterInventory()
        {
            var cap = Player.InventoryRules.BaseSlots;
            var slots = new Player.InvSlot[cap];
            for (var i = 0; i < cap; i++)
            {
                slots[i] = Player.InvSlot.Empty();
            }

            slots[0] = Player.InvSlot.MakeWeapon(Player.WeaponId.IronSword);
            slots[1] = Player.InvSlot.MakePotion(2);
            return slots;
        }

        public void Normalize()
        {
            if (deepestDepth < 1)
            {
                deepestDepth = 1;
            }

            if (!hasAudioPrefs)
            {
                if (sfxVolume <= 0f)
                {
                    sfxVolume = 1f;
                }

                if (shakeIntensity <= 0f)
                {
                    shakeIntensity = 1f;
                }

                hasAudioPrefs = true;
            }

            sfxVolume = Mathf.Clamp01(sfxVolume);
            shakeIntensity = Mathf.Clamp01(shakeIntensity);
            masterVolume = Mathf.Clamp01(masterVolume);

            weaponUpgrades = Player.WeaponCatalog.EnsureUpgradeArray(
                weaponUpgrades, weaponUpgradeLevel, equippedWeaponId);
            weaponTempers = Player.WeaponCatalog.EnsureTemperArray(weaponTempers);
            // legacy 동기화
            var eq = Mathf.Clamp(equippedWeaponId, 0, Player.WeaponCatalog.WeaponCount - 1);
            weaponUpgradeLevel = weaponUpgrades[eq];

            if (!hasQuestPrefs)
            {
                activeQuestId = -1;
                questProgress = 0;
                dailyQuestDay = 0;
                dailyProgress = 0;
                dailyClaimed = false;
                hasQuestPrefs = true;
            }

            // 예전 튜토리얼이 「첫 귀환」을 자동 수락하던 세이브 → 미수락 상태로 되돌림
            if (!hasManualQuestGate)
            {
                activeQuestId = -1;
                questProgress = 0;
                hasManualQuestGate = true;
            }

            MigrateInventoryIfNeeded();
        }

        /// <summary>구세이브: 마스크·포션 → 가방 슬롯.</summary>
        private void MigrateInventoryIfNeeded()
        {
            bagCapacity = Mathf.Max(bagCapacity, Player.InventoryRules.CapacityForLevel(level));
            var needsMigrate = inventory == null || inventory.Length == 0;
            if (!needsMigrate)
            {
                // 용량만 맞춤
                EnsureInventorySize(bagCapacity);
                return;
            }

            inventory = new Player.InvSlot[bagCapacity];
            for (var i = 0; i < bagCapacity; i++)
            {
                inventory[i] = Player.InvSlot.Empty();
            }

            var slot = 0;
            var mask = ownedWeaponsMask == 0 ? 1 : ownedWeaponsMask;
            mask |= 1; // IronSword
            for (var wi = 0; wi < Player.WeaponCatalog.WeaponCount && slot < bagCapacity; wi++)
            {
                var id = (Player.WeaponId)wi;
                if (!Player.WeaponCatalog.Owns(mask, id))
                {
                    continue;
                }

                var up = weaponUpgrades != null && wi < weaponUpgrades.Length ? weaponUpgrades[wi] : 0;
                var temper = weaponTempers != null && wi < weaponTempers.Length ? weaponTempers[wi] : 0;
                var sigil = (weaponSigilMask & (1 << wi)) != 0;
                inventory[slot] = Player.InvSlot.MakeWeapon(id, up, temper, sigil);
                if (wi == equippedWeaponId)
                {
                    equippedInvSlot = slot;
                }

                slot++;
            }

            if (slot < bagCapacity && potions > 0)
            {
                inventory[slot] = Player.InvSlot.MakePotion(potions);
                slot++;
            }

            if (slot < bagCapacity && luckCharges > 0)
            {
                inventory[slot] = Player.InvSlot.MakeLuck(luckCharges);
            }

            if (inventory[0].IsEmpty)
            {
                inventory[0] = Player.InvSlot.MakeWeapon(Player.WeaponId.IronSword);
                equippedInvSlot = 0;
            }
        }

        private void EnsureInventorySize(int cap)
        {
            cap = Mathf.Max(Player.InventoryRules.BaseSlots, cap);
            bagCapacity = cap;
            if (inventory == null)
            {
                inventory = new Player.InvSlot[cap];
                for (var i = 0; i < cap; i++)
                {
                    inventory[i] = Player.InvSlot.Empty();
                }

                inventory[0] = Player.InvSlot.MakeWeapon(Player.WeaponId.IronSword);
                return;
            }

            if (inventory.Length == cap)
            {
                return;
            }

            var next = new Player.InvSlot[cap];
            for (var i = 0; i < cap; i++)
            {
                next[i] = i < inventory.Length && inventory[i] != null
                    ? inventory[i]
                    : Player.InvSlot.Empty();
            }

            inventory = next;
            equippedInvSlot = Mathf.Clamp(equippedInvSlot, 0, cap - 1);
        }
    }
}
