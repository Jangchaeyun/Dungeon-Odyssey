using System;
using System.Collections;
using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.UI;
using DungeonOdyssey.View3D;
using UnityEngine;

namespace DungeonOdyssey.Player
{
    public partial class PlayerStats : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private int level = 1;
        [SerializeField] private int experience;
        [SerializeField] private int experienceToNext = 20;
        [SerializeField] private int attackPower = 10;
        [SerializeField] private int potions = 2;
        [SerializeField] private float moveSpeedBonus;
        [SerializeField] private float critChanceBonus;
        [SerializeField] private int equippedWeaponId;
        [SerializeField] private int weaponUpgradeLevel;
        [SerializeField] private int ownedWeaponsMask = 1;
        [SerializeField] private int weaponSigilMask;
        [SerializeField] private int equippedSkillId;
        [SerializeField] private int ownedSkillsMask = 1;
        /// <summary>-1: 장착 무기 스킬 따라감. 그 외: 해당 WeaponId 스킬을 R로 사용.</summary>
        [SerializeField] private int skillSourceWeaponId = -1;

        private int[] _weaponUpgrades = new int[WeaponCatalog.WeaponCount];
        private int[] _weaponTempers = new int[WeaponCatalog.WeaponCount];
        private PlayerController _controller;
        private PlayerSkill _skill;
        private bool _dying;

        public int Level => level;
        public int Experience => experience;
        public int ExperienceToNext => experienceToNext;
        public int BaseAttackPower => attackPower;
        public WeaponId EquippedWeapon => (WeaponId)Mathf.Clamp(equippedWeaponId, 0, WeaponCatalog.WeaponCount - 1);
        public int WeaponUpgradeLevel =>
            EquippedInv != null && EquippedInv.Kind == InvItemKind.Weapon
                ? EquippedInv.upgrade
                : GetUpgrade(EquippedWeapon);
        public int WeaponTemperLevel =>
            EquippedInv != null && EquippedInv.Kind == InvItemKind.Weapon
                ? EquippedInv.temper
                : GetTemper(EquippedWeapon);
        public bool HasWeaponSigil =>
            EquippedInv != null && EquippedInv.Kind == InvItemKind.Weapon
                ? EquippedInv.HasSigil
                : WeaponCatalog.HasSigil(weaponSigilMask, EquippedWeapon);
        public int OwnedWeaponsMask => ownedWeaponsMask;
        public int WeaponBonus =>
            WeaponCatalog.GetWeaponBonus(EquippedWeapon, WeaponUpgradeLevel, WeaponTemperLevel)
            + (HasWeaponSigil && EquippedWeapon == WeaponId.IronSword ? 4 : 0);
        public int MetaBlessing => GameManager.Instance != null ? GameManager.Instance.CurrentSave.metaBlessing : 0;
        public int AttackPower =>
            Mathf.RoundToInt((attackPower + WeaponBonus + MetaBlessing) * AttackMultiplier);

        private float _attackBoostMul = 1f;
        private float _attackBoostUntil;

        public float AttackMultiplier =>
            Time.time < _attackBoostUntil ? _attackBoostMul : 1f;

        public void GrantAttackBoost(float mul, float duration)
        {
            _attackBoostMul = Mathf.Max(1f, mul);
            _attackBoostUntil = Time.time + Mathf.Max(0.1f, duration);
            RaiseMeta();
        }

        public string WeaponLabel =>
            WeaponCatalog.Label(EquippedWeapon, WeaponUpgradeLevel, WeaponTemperLevel, HasWeaponSigil);
        public string WeaponGradeLine =>
            WeaponCatalog.GradeLine(EquippedWeapon, WeaponUpgradeLevel, WeaponTemperLevel, HasWeaponSigil,
                WeaponCatalog.MaxUpgradeAllowed(EquippedWeapon, level));
        public WeaponDef EquippedWeaponDef => WeaponCatalog.Get(EquippedWeapon);
        /// <summary>R 스킬의 출처 무기 (오버라이드 또는 장착 무기).</summary>
        public WeaponId SkillSourceWeapon
        {
            get
            {
                if (skillSourceWeaponId >= 0 && skillSourceWeaponId < WeaponCatalog.WeaponCount &&
                    IsWeaponInInventory((WeaponId)skillSourceWeaponId))
                {
                    return (WeaponId)skillSourceWeaponId;
                }

                return EquippedWeapon;
            }
        }

        public bool SkillFollowsWeapon =>
            skillSourceWeaponId < 0 || !IsWeaponInInventory((WeaponId)skillSourceWeaponId);

        public SkillId EquippedSkill => SkillCatalog.ForWeapon(SkillSourceWeapon);
        public int OwnedSkillsMask => ownedSkillsMask;
        public string SkillLabel => SkillCatalog.WeaponSkillName(SkillSourceWeapon);
        public int Potions => potions;
        public float MoveSpeedBonus => moveSpeedBonus;
        public float CritChanceBonus =>
            critChanceBonus + WeaponCatalog.GetCritBonus(EquippedWeapon, HasWeaponSigil);
        public Health Health => health;
        public int Gold => GameManager.Instance != null ? GameManager.Instance.CurrentSave.gold : 0;
        public int DungeonClears => GameManager.Instance != null ? GameManager.Instance.CurrentSave.dungeonClears : 0;

        private int GetUpgrade(WeaponId id)
        {
            var i = (int)id;
            if (_weaponUpgrades == null || i < 0 || i >= _weaponUpgrades.Length)
            {
                return 0;
            }

            return _weaponUpgrades[i];
        }

        private int GetTemper(WeaponId id)
        {
            var i = (int)id;
            if (_weaponTempers == null || i < 0 || i >= _weaponTempers.Length)
            {
                return 0;
            }

            return _weaponTempers[i];
        }

        private void SetUpgrade(WeaponId id, int value)
        {
            EnsureWeaponArrays();
            var i = (int)id;
            var max = WeaponCatalog.MaxUpgradeAllowed(id, level);
            _weaponUpgrades[i] = Mathf.Clamp(value, 0, max);
            if (id == EquippedWeapon)
            {
                weaponUpgradeLevel = _weaponUpgrades[i];
            }
        }

        private void SetTemper(WeaponId id, int value)
        {
            EnsureWeaponArrays();
            _weaponTempers[(int)id] = Mathf.Clamp(value, 0, WeaponCatalog.MaxTemper);
        }

        private void EnsureWeaponArrays()
        {
            _weaponUpgrades = WeaponCatalog.EnsureUpgradeArray(_weaponUpgrades, weaponUpgradeLevel, equippedWeaponId);
            _weaponTempers = WeaponCatalog.EnsureTemperArray(_weaponTempers);
        }

        public event Action<int> OnLevelUp;
        public event Action<int, int, int> OnExperienceChanged;
        public event Action OnMetaChanged;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            _controller = GetComponent<PlayerController>();
            _skill = GetComponent<PlayerSkill>();
        }

        private void Start()
        {
            LoadFromSave();
            health.OnDied += HandleDeath;
            health.OnHealthChanged += HandleHealthChanged;
            health.OnDamaged += HandleDamagedForQuest;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnDied -= HandleDeath;
                health.OnHealthChanged -= HandleHealthChanged;
                health.OnDamaged -= HandleDamagedForQuest;
            }
        }

        private void HandleDamagedForQuest(int amount, Vector2 _)
        {
            if (amount > 0)
            {
                QuestCatalog.NotifyPlayerHurt();
            }
        }

        private void HandleHealthChanged(int current, int max)
        {
            SyncToSave();
        }

        public void LoadFromSave()
        {
            var save = GameManager.Instance != null ? GameManager.Instance.CurrentSave : null;
            if (save == null)
            {
                health.Initialize(100, 100);
                SyncToSave();
                RaiseExpChanged();
                RaiseMeta();
                return;
            }

            level = save.level;
            experience = save.experience;
            experienceToNext = save.experienceToNext;
            attackPower = save.attackPower;
            potions = save.potions;
            moveSpeedBonus = save.moveSpeedBonus;
            critChanceBonus = save.critChanceBonus;
            // 구세이브·저성장 캐릭터: 레벨 대비 최소 스탯 보정
            EnsureLevelFloor(ref attackPower, ref moveSpeedBonus, ref critChanceBonus, level);
            equippedWeaponId = Mathf.Clamp(save.equippedWeaponId, 0, WeaponCatalog.WeaponCount - 1);
            _weaponUpgrades = WeaponCatalog.EnsureUpgradeArray(
                save.weaponUpgrades, save.weaponUpgradeLevel, equippedWeaponId);
            _weaponTempers = WeaponCatalog.EnsureTemperArray(save.weaponTempers);
            weaponSigilMask = save.weaponSigilMask;
            weaponUpgradeLevel = GetUpgrade(EquippedWeapon);
            ownedWeaponsMask = save.ownedWeaponsMask == 0 ? 1 : save.ownedWeaponsMask;
            ownedWeaponsMask = WeaponCatalog.WithOwned(ownedWeaponsMask, WeaponId.IronSword);
            ownedSkillsMask = save.ownedSkillsMask == 0
                ? SkillCatalog.DefaultOwnedMask
                : save.ownedSkillsMask;
            ownedSkillsMask = SkillCatalog.ApplyMetaUnlocks(ownedSkillsMask, save.dungeonClears, level);
            equippedSkillId = save.equippedSkillId;
            skillSourceWeaponId = save.skillSourceWeaponId;
            if (skillSourceWeaponId >= WeaponCatalog.WeaponCount)
            {
                skillSourceWeaponId = -1;
            }

            var flooredMaxHp = Mathf.Max(save.maxHp, MinMaxHpForLevel(level));
            // 0 HP 세이브로 마을에 들어오면 조작만 되고 회복이 안 되던 문제 방지
            var hp = save.currentHp <= 0 ? flooredMaxHp : Mathf.Min(save.currentHp, flooredMaxHp);
            if (flooredMaxHp > save.maxHp)
            {
                hp = flooredMaxHp; // 보정으로 오른 최대 체력은 풀피로
            }

            health.Initialize(flooredMaxHp, hp);
            if (save.currentHp <= 0 || flooredMaxHp > save.maxHp)
            {
                save.currentHp = hp;
            }

            LoadInventoryFromSave(save);
            ValidateSkillSource();
            if (!SkillCatalog.Owns(ownedSkillsMask, EquippedSkill))
            {
                equippedSkillId = (int)SkillCatalog.ForWeapon(SkillSourceWeapon);
            }

            _dying = false;
            _controller?.SetControlEnabled(true);
            ApplyWeaponVisual();
            _skill?.RefreshFromStats();
            SyncToSave();
            RaiseExpChanged();
            RaiseMeta();
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0 || health.IsDead)
            {
                return;
            }

            experience += amount;
            while (experience >= experienceToNext)
            {
                experience -= experienceToNext;
                LevelUp();
            }

            RaiseExpChanged();
            SyncToSave();
        }

        private void LevelUp()
        {
            level += 1;
            experienceToNext = Mathf.RoundToInt(experienceToNext * 1.32f) + 4;
            ApplyBaselineGrowth();
            RefreshBagCapacityForLevel();
            TryUnlockSkillsByLevel();
            OnLevelUp?.Invoke(level);

            var levelUi = FindFirstObjectByType<LevelUpUI>();
            if (levelUi != null)
            {
                levelUi.Show(this);
            }
            else
            {
                ApplyLevelBonus(LevelBonus.Attack);
            }
        }

        /// <summary>레벨업마다 무조건 오르는 기본 성장 (선택 보너스와 별개).</summary>
        private void ApplyBaselineGrowth()
        {
            attackPower += 3;
            moveSpeedBonus += 0.15f;
            critChanceBonus += 0.02f;
            health.SetMaxHp(health.MaxHp + 20, healToFull: true);
            if (level % 3 == 0)
            {
                TryAddInvItem(InvSlot.MakePotion(1), out _);
            }

            SyncToSave();
            RaiseMeta();
        }

        private void TryUnlockSkillsByLevel()
        {
            ownedSkillsMask = SkillCatalog.FullMask;

            var weaponName = WeaponCatalog.NewUnlockNameAtLevel(level);
            var forgeCap = WeaponCatalog.MaxUpgradeAllowed(EquippedWeapon, level);

            if (weaponName != null)
            {
                FindFirstObjectByType<HudUI>()?.SetHint(
                    $"무기 해금! {weaponName} — 상점에서 구매 가능");
                CombatAudio.LevelUp();
            }
            else if (level % 2 == 1)
            {
                FindFirstObjectByType<HudUI>()?.SetHint(
                    $"무기 강화 상한 +{forgeCap}  (캐릭터 Lv {level})");
            }

            SyncToSave();
            RaiseMeta();
            _skill?.RefreshFromStats();
        }

        public static int MinAttackForLevel(int lv) => 10 + Mathf.Max(0, lv - 1) * 3;

        public static int MinMaxHpForLevel(int lv) => 100 + Mathf.Max(0, lv - 1) * 20;

        public static float MinMoveSpeedForLevel(int lv) => Mathf.Max(0, lv - 1) * 0.15f;

        public static float MinCritForLevel(int lv) => Mathf.Max(0, lv - 1) * 0.02f;

        private static void EnsureLevelFloor(ref int atk, ref float move, ref float crit, int lv)
        {
            atk = Mathf.Max(atk, MinAttackForLevel(lv));
            move = Mathf.Max(move, MinMoveSpeedForLevel(lv));
            crit = Mathf.Max(crit, MinCritForLevel(lv));
        }

        public enum LevelBonus
        {
            MaxHp,
            Attack,
            Speed,
            Crit,
            WeaponEnhance
        }

        public void ApplyLevelBonus(LevelBonus bonus)
        {
            switch (bonus)
            {
                case LevelBonus.MaxHp:
                    health.SetMaxHp(health.MaxHp + 36, healToFull: true);
                    attackPower += 2;
                    break;
                case LevelBonus.Attack:
                    attackPower += 7;
                    health.SetMaxHp(health.MaxHp + 12, healToFull: true);
                    break;
                case LevelBonus.Speed:
                    moveSpeedBonus += 0.9f;
                    attackPower += 2;
                    health.SetMaxHp(health.MaxHp + 12, healToFull: true);
                    break;
                case LevelBonus.Crit:
                    critChanceBonus += 0.12f;
                    attackPower += 3;
                    health.SetMaxHp(health.MaxHp + 12, healToFull: true);
                    break;
                case LevelBonus.WeaponEnhance:
                    if (!TryFreeWeaponEnhance(out _))
                    {
                        // 이미 극한이면 대체 보상
                        attackPower += 5;
                        health.SetMaxHp(health.MaxHp + 16, healToFull: true);
                    }

                    break;
            }

            SyncToSave();
            RaiseMeta();
        }

        /// <summary>레벨업 보상 — 골드/상한 무시하고 장착 무기 +1 (또는 템퍼).</summary>
        public bool TryFreeWeaponEnhance(out string message)
        {
            EnsureWeaponArrays();
            var def = WeaponCatalog.Get(EquippedWeapon);
            var current = WeaponUpgradeLevel;
            var max = WeaponCatalog.MaxUpgradeAllowed(EquippedWeapon, level);
            if (current < max)
            {
                SetUpgrade(EquippedWeapon, current + 1);
                WriteEquippedSlotFromArrays();
                ApplyWeaponVisual();
                SyncToSave();
                RaiseMeta();
                message =
                    $"레벨업 연마!  {WeaponLabel}  ·  {WeaponCatalog.Stars(WeaponUpgradeLevel, max)}\n" +
                    $"무기 +{WeaponBonus}  ATK {AttackPower}";
                FindFirstObjectByType<ClearBannerUI>()?.Show(
                    $"연마  {def.Name}+{WeaponUpgradeLevel}", ToastKind.Success, 1.6f);
                return true;
            }

            if (WeaponTemperLevel < WeaponCatalog.MaxTemper)
            {
                SetTemper(EquippedWeapon, WeaponTemperLevel + 1);
                WriteEquippedSlotFromArrays();
                ApplyWeaponVisual();
                SyncToSave();
                RaiseMeta();
                message = $"레벨업 템퍼!  {WeaponLabel}  ATK {AttackPower}";
                FindFirstObjectByType<ClearBannerUI>()?.Show(
                    $"템퍼 ⋆{WeaponTemperLevel}  {def.Name}", ToastKind.Success, 1.6f);
                return true;
            }

            message = $"{def.Name}은(는) 현재 레벨 상한(+{max})·템퍼 상태다.";
            return false;
        }

        public string DescribeLevelWeaponBonus()
        {
            var def = WeaponCatalog.Get(EquippedWeapon);
            var max = WeaponCatalog.MaxUpgradeAllowed(EquippedWeapon, level);
            if (WeaponUpgradeLevel < max)
            {
                var next = WeaponUpgradeLevel + 1;
                var bonusNext = WeaponCatalog.GetWeaponBonus(EquippedWeapon, next, WeaponTemperLevel)
                                - WeaponBonus;
                return $"◆ 무기 연마\n{def.Name}+{WeaponUpgradeLevel}→+{next}  ·  ATK +{bonusNext}\n(상한 +{max} = 캐릭터 Lv)";
            }

            if (WeaponTemperLevel < WeaponCatalog.MaxTemper)
            {
                return $"◆ 무기 템퍼\n{def.Name} ⋆{WeaponTemperLevel}→⋆{WeaponTemperLevel + 1}  ·  ATK +1\n(무료)";
            }

            return $"◆ 무기 상한\n현재 Lv 상한 +{max} 도달\n대체: ATK +5 · HP +16";
        }

        public bool TryUsePotion()
        {
            if (potions <= 0 || health.IsDead || health.CurrentHp >= health.MaxHp)
            {
                return false;
            }

            if (!TryConsumeKind(InvItemKind.Potion, 1))
            {
                return false;
            }

            var heal = Mathf.RoundToInt(health.MaxHp * 0.4f);
            health.Initialize(health.MaxHp, Mathf.Min(health.MaxHp, health.CurrentHp + heal));
            CombatAudio.Heal();
            FloatingText.Heal(transform.position + Vector3.up * 1.4f, heal);
            SessionStats.NotePotion();
            SyncToSave();
            RaiseMeta();
            return true;
        }

        public void AddPotions(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            TryAddInvItem(InvSlot.MakePotion(amount), out _);
            SyncToSave();
            RaiseMeta();
        }

        private bool TryConsumeKind(InvItemKind kind, int amount)
        {
            var left = amount;
            for (var i = 0; i < _inventory.Length && left > 0; i++)
            {
                var s = _inventory[i];
                if (s == null || s.Kind != kind || s.count <= 0)
                {
                    continue;
                }

                var take = Mathf.Min(s.count, left);
                s.count -= take;
                left -= take;
                if (s.count <= 0)
                {
                    _inventory[i] = InvSlot.Empty();
                }
            }

            if (left > 0)
            {
                return false;
            }

            SyncConsumableScalars();
            return true;
        }

        public void AddAttack(int amount)
        {
            attackPower += amount;
            SyncToSave();
            RaiseMeta();
        }

        public void AddCritBonus(float amount)
        {
            critChanceBonus += amount;
            SyncToSave();
            RaiseMeta();
        }

        public void SetEquippedSkill(SkillId id)
        {
            // 계열만 지정된 경우 — 해당 계열의 보유 무기 스킬로 맞춤
            for (var i = 0; i < BagCapacity; i++)
            {
                if (!IsWeaponSlot(i))
                {
                    continue;
                }

                var w = GetInvSlot(i).Weapon;
                if (SkillCatalog.ForWeapon(w) == id)
                {
                    SetSkillSourceWeapon(w);
                    return;
                }
            }

            SetSkillFollowWeapon();
        }

        /// <summary>가방에서 고른 무기의 스킬을 R로 장착.</summary>
        public bool SetSkillSourceWeapon(WeaponId id, out string message)
        {
            if (!IsWeaponInInventory(id))
            {
                message = "가방에 없는 무기의 스킬이다.";
                return false;
            }

            skillSourceWeaponId = (int)id;
            equippedSkillId = (int)SkillCatalog.ForWeapon(id);
            SyncToSave();
            RaiseMeta();
            _skill?.RefreshFromStats();
            message =
                $"스킬 장착  ·  {SkillCatalog.WeaponSkillName(id)}  ({WeaponCatalog.Get(id).Name})";
            return true;
        }

        public void SetSkillSourceWeapon(WeaponId id) => SetSkillSourceWeapon(id, out _);

        /// <summary>R 스킬을 현재 장착 무기에 맞춤.</summary>
        public void SetSkillFollowWeapon()
        {
            skillSourceWeaponId = -1;
            equippedSkillId = (int)SkillCatalog.ForWeapon(EquippedWeapon);
            SyncToSave();
            RaiseMeta();
            _skill?.RefreshFromStats();
        }

        public void ValidateSkillSource()
        {
            if (skillSourceWeaponId < 0)
            {
                return;
            }

            if (skillSourceWeaponId >= WeaponCatalog.WeaponCount ||
                !IsWeaponInInventory((WeaponId)skillSourceWeaponId))
            {
                skillSourceWeaponId = -1;
                equippedSkillId = (int)SkillCatalog.ForWeapon(EquippedWeapon);
            }
        }

        public bool TryBuySkill(out string message)
        {
            message = "스킬은 가방에서 보유 무기 스킬 중 골라 장착한다.";
            return false;
        }

        public bool TryUpgradeWeapon(out string message) =>
            TryForgeWeapon(EquippedWeapon, precision: false, out message);

        public bool TryPrecisionForge(out string message) =>
            TryForgeWeapon(EquippedWeapon, precision: true, out message);

        public bool TryForgeWeapon(WeaponId id, bool precision, out string message) =>
            TryForgeWeapon(id, precision, out message, out _, out _);

        public bool TryForgeWeapon(WeaponId id, bool precision, out string message,
            out ForgeResult result, out int stepsGained)
        {
            result = ForgeResult.Fail;
            stepsGained = 0;
            EnsureWeaponArrays();
            if (!IsWeaponInInventory(id))
            {
                message = "보유하지 않은 무기다.";
                return false;
            }

            var def = WeaponCatalog.Get(id);
            var current = GetUpgrade(id);
            var allowed = WeaponCatalog.MaxUpgradeAllowed(id, level);
            if (current >= allowed)
            {
                message = HasSigilOf(id)
                    ? $"{def.Name}은(는) 이미 레벨 상한(+{allowed})·각인 상태다."
                    : $"{def.Name}은(는) 현재 레벨 상한(+{allowed}). 레벨업 후 더 강화할 수 있다.";
                return false;
            }

            var cost = precision
                ? WeaponCatalog.PrecisionCost(current, level)
                : WeaponCatalog.UpgradeCost(current, level);
            if (GameManager.Instance == null || !GameManager.Instance.TrySpendGold(cost))
            {
                message = $"골드가 부족하다. ({(precision ? "정밀 " : "")}연마 {cost}G)";
                return false;
            }

            result = WeaponCatalog.RollForge(current, precision);
            var steps = WeaponCatalog.ForgeSteps(result);
            var room = allowed - current;
            stepsGained = Mathf.Min(steps, room);
            var beforeBonus = WeaponCatalog.GetWeaponBonus(id, current, GetTemper(id));
            SetUpgrade(id, current + stepsGained);

            if (precision)
            {
                SetTemper(id, GetTemper(id) + 1);
            }

            var afterBonus = WeaponCatalog.GetWeaponBonus(id, GetUpgrade(id), GetTemper(id));
            if (id == EquippedWeapon)
            {
                ApplyWeaponVisual();
                WriteEquippedSlotFromArrays();
            }

            SyncToSave();
            RaiseMeta();

            if (WeaponCatalog.IsFullyUpgraded(GetUpgrade(id), level))
            {
                AchievementCatalog.TryUnlock(AchievementId.FullForge, "최대 강화");
            }

            var tag = result switch
            {
                ForgeResult.Perfect => "✦ 완벽 연마!",
                ForgeResult.Great => "◆ 대성공!",
                _ => precision ? "정밀 연마" : "연마 성공"
            };
            var label = WeaponCatalog.Label(id, GetUpgrade(id), GetTemper(id), HasSigilOf(id));
            message =
                $"{tag}  {label}  ·  +{current}→+{GetUpgrade(id)} (+{stepsGained})  ·  " +
                $"보너스 +{beforeBonus}→+{afterBonus}  ·  ATK {AttackPower}" +
                (GetTemper(id) > 0 ? $"  ·  템퍼 ⋆{GetTemper(id)}" : "");

            FindFirstObjectByType<ClearBannerUI>()?.Show(
                $"{tag}  {def.Name}+{GetUpgrade(id)}",
                result == ForgeResult.Perfect || result == ForgeResult.Great
                    ? ToastKind.Style
                    : ToastKind.Success,
                1.8f);
            return true;
        }

        public bool TryInscribeSigil(out string message)
        {
            EnsureWeaponArrays();
            var def = WeaponCatalog.Get(EquippedWeapon);
            var allowed = WeaponCatalog.MaxUpgradeAllowed(EquippedWeapon, level);
            if (WeaponUpgradeLevel < allowed)
            {
                message = $"각인은 현재 레벨 상한(+{allowed})까지 강화한 뒤 가능하다.";
                return false;
            }

            if (HasWeaponSigil)
            {
                message = $"{def.Name}에는 이미 『{def.SigilName}』이 새겨져 있다.";
                return false;
            }

            var cost = WeaponCatalog.SigilCost(EquippedWeapon, level);
            if (GameManager.Instance == null || !GameManager.Instance.TrySpendGold(cost))
            {
                message = $"골드가 부족하다. (각인 {cost}G)";
                return false;
            }

            weaponSigilMask = WeaponCatalog.WithSigil(weaponSigilMask, EquippedWeapon);
            WriteEquippedSlotFromArrays();
            ApplyWeaponVisual();
            SyncToSave();
            RaiseMeta();
            message = $"『{def.SigilName}』 각인!  {def.SigilDesc}";
            FindFirstObjectByType<ClearBannerUI>()
                ?.Show($"각인  ·  {def.SigilName}", ToastKind.Achievement, 2.2f);
            CombatAudio.LevelUp();
            return true;
        }

        /// <summary>보유 무기만 순환 (상점 없이 즉시 교체). direction: +1 다음, -1 이전.</summary>
        public bool TryCycleWeapon(out string message) => TryCycleWeapon(1, out message);

        public bool TryCycleWeapon(int direction, out string message)
        {
            if (CountWeaponSlots() <= 1)
            {
                message = "교체할 다른 무기가 없다.";
                return false;
            }

            var next = NextWeaponInvSlot(_equippedInvSlot, direction);
            if (next == _equippedInvSlot)
            {
                message = "교체할 다른 무기가 없다.";
                return false;
            }

            return TryEquipInvSlot(next, out message);
        }

        public bool TryEquipWeapon(WeaponId id, out string message)
        {
            for (var i = 0; i < _inventory.Length; i++)
            {
                if (IsWeaponSlot(i) && _inventory[i].Weapon == id)
                {
                    return TryEquipInvSlot(i, out message);
                }
            }

            message = "보유하지 않은 무기다.";
            return false;
        }

        /// <summary>보유 순번(1~)으로 즉시 장착.</summary>
        public bool TryEquipOwnedSlot(int oneBasedSlot, out string message)
        {
            var n = 0;
            for (var i = 0; i < _inventory.Length; i++)
            {
                if (!IsWeaponSlot(i))
                {
                    continue;
                }

                n++;
                if (n == oneBasedSlot)
                {
                    return TryEquipInvSlot(i, out message);
                }
            }

            message = $"슬롯 {oneBasedSlot}에 무기가 없다.";
            return false;
        }

        /// <summary>다음에 교체될 무기 이름 (HUD 미리보기).</summary>
        public bool TryPeekNextWeapon(out string label)
        {
            label = "";
            if (CountWeaponSlots() <= 1)
            {
                return false;
            }

            var next = NextWeaponInvSlot(_equippedInvSlot, 1);
            if (next == _equippedInvSlot || !IsWeaponSlot(next))
            {
                return false;
            }

            var s = _inventory[next];
            label = WeaponCatalog.Label(s.Weapon, s.upgrade, s.temper, s.HasSigil);
            return true;
        }

        public bool TryBuyWeapon(WeaponId id, out string message)
        {
            EnsureWeaponArrays();
            var def = WeaponCatalog.Get(id);
            if (!WeaponCatalog.IsUnlocked(id, level))
            {
                message = $"{def.Name}은(는) Lv {def.UnlockLevel}에 해금된다.";
                return false;
            }

            if (!HasBagSpaceForNewItem(InvItemKind.Weapon))
            {
                message = $"가방이 가득 찼다. ({UsedBagSlots}/{_bagCapacity})";
                return false;
            }

            var cost = WeaponCatalog.ShopPrice(id);
            if (GameManager.Instance == null || !GameManager.Instance.TrySpendGold(cost))
            {
                message = $"골드가 부족하다. ({def.Name} {cost}G)";
                return false;
            }

            if (!TryAddInvItem(InvSlot.MakeWeapon(id), out message))
            {
                GameManager.Instance.AddGold(cost); // 롤백
                return false;
            }

            // 방금 넣은 슬롯 찾아 장착
            for (var i = _inventory.Length - 1; i >= 0; i--)
            {
                if (IsWeaponSlot(i) && _inventory[i].Weapon == id)
                {
                    TryEquipInvSlot(i, out _);
                    break;
                }
            }

            ApplyWeaponVisual();
            SyncToSave();
            RaiseMeta();
            var rarity = WeaponCatalog.RarityName(WeaponCatalog.GetRarity(id));
            message = $"[{rarity}] {def.Name} 구매! 가방에 저장 · 장착  ATK {AttackPower}";
            return true;
        }

        /// <summary>같은 등급 무기 2개 합성 → 한 단계 높은 등급 무기 획득.</summary>
        public bool TrySynthesizeWeapons(WeaponId a, WeaponId b, out string message, out WeaponId? gained)
        {
            gained = null;
            var slotA = -1;
            var slotB = -1;
            for (var i = 0; i < _inventory.Length; i++)
            {
                if (!IsWeaponSlot(i))
                {
                    continue;
                }

                if (slotA < 0 && _inventory[i].Weapon == a)
                {
                    slotA = i;
                    continue;
                }

                if (slotB < 0 && _inventory[i].Weapon == b && i != slotA)
                {
                    slotB = i;
                }
            }

            if (slotA < 0 || slotB < 0)
            {
                message = "보유하지 않은 무기다.";
                return false;
            }

            return TrySynthesizeInvSlots(slotA, slotB, out message, out gained);
        }

        public bool TryBuyOrCycleWeapon(out string message)
        {
            // 가방에 공간이 있으면 미보유 무기 구매 시도, 아니면 순환
            var next = WeaponCatalog.NextUnowned(ownedWeaponsMask, level);
            if (next.HasValue && HasBagSpaceForNewItem(InvItemKind.Weapon))
            {
                return TryBuyWeapon(next.Value, out message);
            }

            var locked = WeaponCatalog.NextLocked(ownedWeaponsMask, level);
            if (locked.HasValue && (!next.HasValue || !HasBagSpaceForNewItem(InvItemKind.Weapon)))
            {
                // fall through to cycle when can't buy
            }

            return TryCycleWeapon(1, out message);
        }

        public int UpgradeOf(WeaponId id) => GetUpgrade(id);
        public int TemperOf(WeaponId id) => GetTemper(id);
        public bool HasSigilOf(WeaponId id) => WeaponCatalog.HasSigil(weaponSigilMask, id);

        public void ApplyWeaponVisual()
        {
            EquipmentBuilder.ApplyWeaponVisual(gameObject, EquippedWeapon,
                WeaponUpgradeLevel + WeaponTemperLevel + (HasWeaponSigil ? 1 : 0));
        }

        public void FullHeal()
        {
            _dying = false;
            _controller?.SetControlEnabled(true);
            health.HealFull();
            SyncToSave();
            RaiseMeta();
        }

        public void NotifyMetaChanged() => RaiseMeta();

        public void SyncToSave()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.ApplyPlayerStats(
                level,
                experience,
                experienceToNext,
                health.MaxHp,
                health.CurrentHp,
                attackPower);
            var save = GameManager.Instance.CurrentSave;
            save.potions = potions;
            save.moveSpeedBonus = moveSpeedBonus;
            save.critChanceBonus = critChanceBonus;
            save.equippedWeaponId = equippedWeaponId;
            EnsureWeaponArrays();
            save.weaponUpgrades = (int[])_weaponUpgrades.Clone();
            save.weaponTempers = (int[])_weaponTempers.Clone();
            save.weaponUpgradeLevel = WeaponUpgradeLevel;
            save.weaponSigilMask = weaponSigilMask;
            SaveInventoryToSave(save);
            save.equippedSkillId = equippedSkillId;
            save.skillSourceWeaponId = skillSourceWeaponId;
            save.ownedSkillsMask = ownedSkillsMask;
        }

        private void RaiseExpChanged()
        {
            OnExperienceChanged?.Invoke(experience, experienceToNext, level);
        }

        private void RaiseMeta()
        {
            OnMetaChanged?.Invoke();
        }

        private void HandleDeath()
        {
            if (_dying)
            {
                return;
            }

            _dying = true;
            SyncToSave();
            CombatAudio.Defeat();
            _controller?.SetControlEnabled(false);
            StartCoroutine(DeathReturnRoutine());
        }

        private IEnumerator DeathReturnRoutine()
        {
            var lostPreview = GameManager.Instance != null
                ? Mathf.RoundToInt(GameManager.Instance.CurrentSave.gold * 0.2f)
                : 0;

            var goUi = FindFirstObjectByType<GameOverUI>();
            if (goUi != null)
            {
                goUi.Show(lostPreview);
                yield return new WaitForSecondsRealtime(1.8f);
            }
            else
            {
                yield return new WaitForSeconds(0.7f);
            }

            GameManager.Instance?.ReturnToTownFromDeath(0.2f);
        }
    }
}
