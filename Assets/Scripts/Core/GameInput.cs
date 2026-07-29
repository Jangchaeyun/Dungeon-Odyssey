using UnityEngine;

namespace DungeonOdyssey.Core
{
    /// <summary>
    /// PC 키보드 + 게임패드 + 모바일 터치를 한곳에서 합칩니다.
    /// 패드: LS 이동 · A 공격 · X 상호작용 · Y 포션 · B 스킬 · LB/RB 무기 · Back 가방 · Start 일시정지
    /// </summary>
    public static class GameInput
    {
        private static Vector2 _mobileMove;
        private static int _attackFrame = -1;
        private static int _interactFrame = -1;
        private static int _potionFrame = -1;
        private static int _skillFrame = -1;
        private static int _skillCycleFrame = -1;
        private static int _weaponCycleFrame = -1;
        private static int _weaponCyclePrevFrame = -1;
        private static int _bagFrame = -1;
        private static int _pauseFrame = -1;
        private static readonly int[] MenuFrames = { -1, -1, -1, -1, -1, -1, -1 };

        // Xbox 레이아웃 기준 (대부분 패드 공통)
        private const KeyCode PadA = KeyCode.JoystickButton0;
        private const KeyCode PadB = KeyCode.JoystickButton1;
        private const KeyCode PadX = KeyCode.JoystickButton2;
        private const KeyCode PadY = KeyCode.JoystickButton3;
        private const KeyCode PadLb = KeyCode.JoystickButton4;
        private const KeyCode PadRb = KeyCode.JoystickButton5;
        private const KeyCode PadBack = KeyCode.JoystickButton6;
        private const KeyCode PadStart = KeyCode.JoystickButton7;

        public static bool IsMobileLayout
        {
            get
            {
#if UNITY_ANDROID || UNITY_IOS
                return true;
#else
                return Application.isMobilePlatform
                       || Input.touchSupported && SystemInfo.deviceType == DeviceType.Handheld;
#endif
            }
        }

        public static void SetMobileMove(Vector2 axis) =>
            _mobileMove = Vector2.ClampMagnitude(axis, 1f);

        public static void ClearMobileMove() => _mobileMove = Vector2.zero;

        public static void PressAttack() => _attackFrame = Time.frameCount;
        public static void PressInteract() => _interactFrame = Time.frameCount;
        public static void PressPotion() => _potionFrame = Time.frameCount;
        public static void PressSkill() => _skillFrame = Time.frameCount;
        public static void PressSkillCycle() => _skillCycleFrame = Time.frameCount;
        public static void PressWeaponCycle() => _weaponCycleFrame = Time.frameCount;
        public static void PressWeaponCyclePrev() => _weaponCyclePrevFrame = Time.frameCount;
        public static void PressBag() => _bagFrame = Time.frameCount;
        public static void PressPause() => _pauseFrame = Time.frameCount;

        public static void PressMenu(int index)
        {
            if (index >= 1 && index <= 6)
            {
                MenuFrames[index] = Time.frameCount;
            }
        }

        public static Vector2 MoveAxis
        {
            get
            {
                // 키보드는 항상 우선 (스틱 드리프트가 WASD를 가로채지 않게)
                var x = 0f;
                var y = 0f;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
                var key = new Vector2(x, y);
                if (key.sqrMagnitude > 1f)
                {
                    key.Normalize();
                }

                if (key.sqrMagnitude < 0.01f)
                {
                    // 키 입력이 없을 때만 패드 스틱
                    var jx = Input.GetAxisRaw("Horizontal");
                    var jy = Input.GetAxisRaw("Vertical");
                    if (Mathf.Abs(jx) < 0.25f)
                    {
                        jx = 0f;
                    }

                    if (Mathf.Abs(jy) < 0.25f)
                    {
                        jy = 0f;
                    }

                    key = new Vector2(jx, jy);
                    if (key.sqrMagnitude > 1f)
                    {
                        key.Normalize();
                    }
                }

                if (_mobileMove.sqrMagnitude > 0.01f)
                {
                    return Vector2.ClampMagnitude(key + _mobileMove, 1f);
                }

                return key;
            }
        }

        public static bool AttackDown =>
            Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.J)
            || Input.GetKeyDown(PadA) || _attackFrame == Time.frameCount;

        public static bool InteractDown =>
            Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(PadX)
            || _interactFrame == Time.frameCount;

        public static bool PotionDown =>
            Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(PadY)
            || _potionFrame == Time.frameCount;

        public static bool SkillDown =>
            Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(PadB)
            || _skillFrame == Time.frameCount;

        public static bool SkillCycleDown =>
            Input.GetKeyDown(KeyCode.Tab) || _skillCycleFrame == Time.frameCount;

        /// <summary>보유 무기 순환 — V/C · [ ] · 마우스 휠 · 휠클릭 · RB.</summary>
        public static bool WeaponCycleDown =>
            Input.GetKeyDown(KeyCode.V)
            || Input.GetKeyDown(KeyCode.C)
            || Input.GetKeyDown(KeyCode.RightBracket)
            || Input.GetKeyDown(KeyCode.Period)
            || Input.GetMouseButtonDown(2)
            || Input.GetKeyDown(PadRb)
            || _weaponCycleFrame == Time.frameCount;

        public static bool WeaponCyclePrevDown =>
            Input.GetKeyDown(KeyCode.Z)
            || Input.GetKeyDown(KeyCode.LeftBracket)
            || Input.GetKeyDown(KeyCode.Comma)
            || Input.GetKeyDown(PadLb)
            || _weaponCyclePrevFrame == Time.frameCount;

        /// <summary>마우스 휠: +1 다음, -1 이전, 0 없음.</summary>
        public static int WeaponScrollDelta
        {
            get
            {
                var y = Input.mouseScrollDelta.y;
                if (y > 0.05f)
                {
                    return -1;
                }

                if (y < -0.05f)
                {
                    return 1;
                }

                return 0;
            }
        }

        /// <summary>Shift+1~9 로 보유 무기 슬롯 직접 선택 (스킬 숫자키와 분리).</summary>
        public static int WeaponSlotKeyDown
        {
            get
            {
                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                {
                    return 0;
                }

                for (var i = 1; i <= 9; i++)
                {
                    var alpha = KeyCode.Alpha0 + i;
                    var pad = KeyCode.Keypad0 + i;
                    if (Input.GetKeyDown(alpha) || Input.GetKeyDown(pad))
                    {
                        return i;
                    }
                }

                return 0;
            }
        }

        public static bool PauseDown =>
            Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(PadStart)
            || _pauseFrame == Time.frameCount;

        public static bool BagDown =>
            Input.GetKeyDown(KeyCode.B)
            || Input.GetKeyDown(KeyCode.I)
            || Input.GetKeyDown(PadBack)
            || _bagFrame == Time.frameCount;

        public static bool MenuDown(int index)
        {
            if (index < 1 || index > 6)
            {
                return false;
            }

            if (MenuFrames[index] == Time.frameCount)
            {
                return true;
            }

            return index switch
            {
                1 => Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1),
                2 => Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2),
                3 => Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3),
                4 => Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4),
                5 => Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5),
                6 => Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6),
                _ => false
            };
        }

        public static bool MoveHeld(DoorHint dir)
        {
            var m = MoveAxis;
            return dir switch
            {
                DoorHint.North => m.y > 0.45f,
                DoorHint.South => m.y < -0.45f,
                DoorHint.East => m.x > 0.45f,
                DoorHint.West => m.x < -0.45f,
                _ => false
            };
        }
    }

    public enum DoorHint
    {
        North,
        South,
        East,
        West
    }
}
