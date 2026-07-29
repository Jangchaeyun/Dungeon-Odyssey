using System;
using UnityEngine;

namespace DungeonOdyssey.Player
{
    public enum InvItemKind
    {
        Empty = 0,
        Weapon = 1,
        Potion = 2,
        Luck = 3
    }

    [Serializable]
    public class InvSlot
    {
        public int kind;
        public int weaponId;
        public int count = 1;
        public int upgrade;
        public int temper;
        public int sigil;

        public InvItemKind Kind
        {
            get => (InvItemKind)kind;
            set => kind = (int)value;
        }

        public bool IsEmpty =>
            Kind == InvItemKind.Empty ||
            Kind is not (InvItemKind.Weapon or InvItemKind.Potion or InvItemKind.Luck) ||
            (Kind != InvItemKind.Weapon && count <= 0);

        public WeaponId Weapon => (WeaponId)Mathf.Clamp(weaponId, 0, WeaponCatalog.WeaponCount - 1);

        public bool HasSigil => sigil != 0;

        public static InvSlot Empty() => new() { kind = (int)InvItemKind.Empty, count = 0 };

        public static InvSlot MakeWeapon(WeaponId id, int upgrade = 0, int temper = 0, bool hasSigil = false) =>
            new()
            {
                kind = (int)InvItemKind.Weapon,
                weaponId = (int)id,
                count = 1,
                upgrade = upgrade,
                temper = temper,
                sigil = hasSigil ? 1 : 0
            };

        public static InvSlot MakePotion(int amount) =>
            new() { kind = (int)InvItemKind.Potion, count = Mathf.Max(1, amount) };

        public static InvSlot MakeLuck(int amount) =>
            new() { kind = (int)InvItemKind.Luck, count = Mathf.Max(1, amount) };

        public InvSlot Clone() =>
            new()
            {
                kind = kind,
                weaponId = weaponId,
                count = count,
                upgrade = upgrade,
                temper = temper,
                sigil = sigil
            };
    }

    public static class InventoryRules
    {
        public const int BaseSlots = 10;

        /// <summary>Lv1=10, Lv5=+5, Lv10=+5, Lv20=+10 → 최대 30.</summary>
        public static int CapacityForLevel(int level)
        {
            var cap = BaseSlots;
            if (level >= 5)
            {
                cap += 5;
            }

            if (level >= 10)
            {
                cap += 5;
            }

            if (level >= 20)
            {
                cap += 10;
            }

            return cap;
        }

        public static int MaxStack(InvItemKind kind) => kind switch
        {
            InvItemKind.Potion => 99,
            InvItemKind.Luck => 9,
            _ => 1
        };

        /// <summary>가방 판매가 (상점가의 일부 + 강화 보너스).</summary>
        public static int SellPrice(InvSlot slot)
        {
            if (slot == null || slot.IsEmpty)
            {
                return 0;
            }

            return slot.Kind switch
            {
                InvItemKind.Weapon => Mathf.Max(5,
                    Mathf.RoundToInt(WeaponCatalog.ShopPrice(slot.Weapon) * 0.4f) +
                    slot.upgrade * 8 + slot.temper * 12 + (slot.HasSigil ? 40 : 0)),
                InvItemKind.Potion => 8 * Mathf.Max(1, slot.count),
                InvItemKind.Luck => 20 * Mathf.Max(1, slot.count),
                _ => 0
            };
        }

        public static string SlotLabel(InvSlot slot)
        {
            if (slot == null || slot.IsEmpty)
            {
                return "빈 칸";
            }

            return slot.Kind switch
            {
                InvItemKind.Weapon => WeaponCatalog.Label(slot.Weapon, slot.upgrade, slot.temper, slot.HasSigil),
                InvItemKind.Potion => $"회복 포션 ×{slot.count}",
                InvItemKind.Luck => $"행운 부적 ×{slot.count}",
                _ => "빈 칸"
            };
        }
    }
}
