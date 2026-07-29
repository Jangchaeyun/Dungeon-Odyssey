using DungeonOdyssey.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonOdyssey.View3D
{
    /// <summary>
    /// 어깨·팔뚝·손 체인 + 검/방패 애니.
    /// 마을: 칼 늘어뜨림 / 던전: 전투 대기(공격 준비) 자세.
    /// </summary>
    public class HumanoidAnimator : MonoBehaviour
    {
        public Transform Hip;
        public Transform Chest;
        public Transform Head;
        public Transform LegL;
        public Transform LegR;
        public Transform ArmL;
        public Transform ArmR;
        public Transform ForeArmL;
        public Transform ForeArmR;
        public Transform HandL;
        public Transform HandR;
        public Transform Sword;
        public Transform Shield;

        /// <summary>Combat·애니 공유 타이밍.</summary>
        public static float WindupDuration(int combo) => combo == 2 ? 0.1f : 0.075f;
        public static float StrikeDuration(int combo) => combo == 2 ? 0.13f : 0.11f;
        public static float RecoverDuration(int combo) => combo == 2 ? 0.16f : 0.13f;

        private Rigidbody _rb;
        private Vector3 _hip0;
        private Vector3 _chest0;
        private Quaternion _armL0, _armR0, _foreL0, _foreR0, _handL0, _handR0;
        private Quaternion _legL0, _legR0, _sword0, _shield0, _chest0Rot, _head0Rot;
        private bool _restCaptured;

        private float _attackT = -1f;
        private float _phase;
        private int _combo;
        private WeaponTrail _trail;

        private Quaternion _sArmR, _sArmL, _sForeR, _sForeL, _sHandR, _sSword, _sChest;

        public bool IsAttacking => _attackT >= 0f;

        /// <summary>true면 던전 전투 대기 자세, false면 마을용 칼 늘어뜨림.</summary>
        public bool CombatStance { get; private set; }

        // 던전 — 검증된 기본 전투 대기 (옆구리 가드, 검 추가 회전 없음)
        private static Quaternion CombatArmR => Quaternion.Euler(6f, 38f, -52f);
        private static Quaternion CombatForeR => Quaternion.Euler(-42f, 10f, 8f);
        private static Quaternion CombatHandR => Quaternion.Euler(12f, -6f, -28f);
        private static Quaternion CombatArmL => Quaternion.Euler(28f, -48f, 62f);
        private static Quaternion CombatForeL => Quaternion.Euler(-78f, 12f, -22f);
        private static Quaternion CombatHandL => Quaternion.Euler(8f, 20f, -30f);

        // 마을 — 같은 손 위치, 팔꿈치만 조금 펴서 칼끝이 더 아래로
        private static Quaternion TownArmR => CombatArmR;
        private static Quaternion TownForeR => Quaternion.Euler(-22f, 8f, 6f);
        private static Quaternion TownHandR => Quaternion.Euler(20f, -4f, -24f);
        private static Quaternion TownArmL => Quaternion.Euler(24f, -44f, 56f);
        private static Quaternion TownForeL => Quaternion.Euler(-62f, 10f, -18f);
        private static Quaternion TownHandL => CombatHandL;

        private Quaternion IdleArmR => CombatStance ? CombatArmR : TownArmR;
        private Quaternion IdleForeR => CombatStance ? CombatForeR : TownForeR;
        private Quaternion IdleHandR => CombatStance ? CombatHandR : TownHandR;
        private Quaternion IdleArmL => CombatStance ? CombatArmL : TownArmL;
        private Quaternion IdleForeL => CombatStance ? CombatForeL : TownForeL;
        private Quaternion IdleHandL => CombatStance ? CombatHandL : TownHandL;
        private Quaternion SwordIdle => _sword0;

        public Transform GetBladeTipTransform()
        {
            if (Sword == null)
            {
                return null;
            }

            // TrailTip → Tip 메시 → 검 루트 순
            var tip = Sword.Find("TrailTip");
            if (tip == null)
            {
                tip = Sword.Find("Tip");
            }

            return tip != null ? tip : Sword;
        }

        /// <summary>칼끝(트레일 팁) 월드 좌표 — 슬래시/스킬 FX 원점으로 사용.</summary>
        public Vector3 GetBladeTipWorld()
        {
            var tip = GetBladeTipTransform();
            if (tip != null && tip != Sword)
            {
                return tip.position;
            }

            // TrailTip이 없으면 검 로컬 -Y(칼끝 방향)로 추정
            if (Sword != null)
            {
                return Sword.TransformPoint(new Vector3(0f, -1.15f, 0f));
            }

            return transform.position + Vector3.up * 1.1f + transform.forward * 0.75f;
        }

        public Vector3 GetBladeRootWorld()
        {
            if (Sword != null)
            {
                var root = Sword.Find("TrailRoot");
                if (root != null)
                {
                    return root.position;
                }

                return Sword.position;
            }

            return transform.position + Vector3.up * 1f + transform.forward * 0.35f;
        }

        private void Awake() => _rb = GetComponent<Rigidbody>();

        private void Start()
        {
            SyncStanceFromScene();
        }

        /// <summary>던전=공격 준비 자세, 마을=칼 늘어뜨림.</summary>
        public void SetCombatStance(bool combat)
        {
            CombatStance = combat;
            if (_attackT < 0f)
            {
                ApplyIdleImmediate();
            }
        }

        public void SyncStanceFromScene()
        {
            SetCombatStance(SceneManager.GetActiveScene().name == SceneNames.Dungeon);
        }

        public void Bind()
        {
            if (!_restCaptured)
            {
                _hip0 = Hip ? Hip.localPosition : Vector3.zero;
                _chest0 = Chest ? Chest.localPosition : Vector3.zero;
                _armL0 = ArmL ? ArmL.localRotation : Quaternion.identity;
                _armR0 = ArmR ? ArmR.localRotation : Quaternion.identity;
                _foreL0 = ForeArmL ? ForeArmL.localRotation : Quaternion.identity;
                _foreR0 = ForeArmR ? ForeArmR.localRotation : Quaternion.identity;
                _handL0 = HandL ? HandL.localRotation : Quaternion.identity;
                _handR0 = HandR ? HandR.localRotation : Quaternion.identity;
                _legL0 = LegL ? LegL.localRotation : Quaternion.identity;
                _legR0 = LegR ? LegR.localRotation : Quaternion.identity;
                _chest0Rot = Chest ? Chest.localRotation : Quaternion.identity;
                _head0Rot = Head ? Head.localRotation : Quaternion.identity;
                _restCaptured = true;
            }

            _sword0 = Sword ? Sword.localRotation : Quaternion.identity;
            _shield0 = Shield ? Shield.localRotation : Quaternion.identity;
            CombatStance = SceneManager.GetActiveScene().name == SceneNames.Dungeon;
            ApplyIdleImmediate();
        }

        /// <summary>무기 비주얼 교체 시 — Bind를 다시 돌리지 않고 검 기준만 갱신.</summary>
        public void RefreshSword(Transform sword)
        {
            Sword = sword;
            _sword0 = sword != null ? sword.localRotation : Quaternion.identity;
            if (_trail != null)
            {
                _trail = null;
            }
        }

        private void ApplyIdleImmediate()
        {
            if (ArmR != null)
            {
                ArmR.localRotation = _armR0 * IdleArmR;
            }

            if (ForeArmR != null)
            {
                ForeArmR.localRotation = _foreR0 * IdleForeR;
            }

            if (HandR != null)
            {
                HandR.localRotation = _handR0 * IdleHandR;
            }

            if (ArmL != null)
            {
                ArmL.localRotation = _armL0 * IdleArmL;
            }

            if (ForeArmL != null)
            {
                ForeArmL.localRotation = _foreL0 * IdleForeL;
            }

            if (HandL != null)
            {
                HandL.localRotation = _handL0 * IdleHandL;
            }

            if (Sword != null)
            {
                Sword.localRotation = SwordIdle;
            }
        }

        public void PlayAttack(int combo = 0)
        {
            _attackT = 0f;
            _combo = combo;
            _sArmR = ArmR ? ArmR.localRotation : _armR0;
            _sArmL = ArmL ? ArmL.localRotation : _armL0;
            _sForeR = ForeArmR ? ForeArmR.localRotation : _foreR0;
            _sForeL = ForeArmL ? ForeArmL.localRotation : _foreL0;
            _sHandR = HandR ? HandR.localRotation : _handR0;
            _sSword = Sword ? Sword.localRotation : _sword0;
            _sChest = Chest ? Chest.localRotation : _chest0Rot;

            if (_trail == null && Sword != null)
            {
                _trail = Sword.GetComponentInChildren<WeaponTrail>();
            }
        }

        private void LateUpdate()
        {
            var speed = 0f;
            if (_rb != null)
            {
                var v = _rb.linearVelocity;
                v.y = 0f;
                speed = v.magnitude;
            }

            if (_attackT >= 0f)
            {
                AnimateAttack();
                return;
            }

            var moving = speed > 0.25f;
            var gait = Mathf.Lerp(1.4f, 6.8f + speed * 0.55f, Mathf.Clamp01(speed / 4.5f));
            _phase += Time.deltaTime * gait;

            var breathe = Mathf.Sin(Time.time * 1.6f) * 0.01f;
            if (Chest != null)
            {
                Chest.localPosition = Vector3.Lerp(Chest.localPosition, _chest0 + Vector3.up * breathe,
                    10f * Time.deltaTime);
            }

            if (Head != null)
            {
                Soft(Head, _head0Rot * Quaternion.Euler(breathe * 18f, 0f, 0f), 7f);
            }

            if (moving)
            {
                var swing = Mathf.Sin(_phase) * Mathf.Lerp(12f, 28f, Mathf.Clamp01(speed / 5f));
                var bob = Mathf.Abs(Mathf.Sin(_phase)) * 0.028f;
                if (Hip != null)
                {
                    Hip.localPosition = Vector3.Lerp(Hip.localPosition, _hip0 + Vector3.up * bob, 14f * Time.deltaTime);
                }

                Soft(LegL, _legL0 * Quaternion.Euler(swing, 0f, 0f), 16f);
                Soft(LegR, _legR0 * Quaternion.Euler(-swing, 0f, 0f), 16f);

                // 오른팔은 검 때문에 조금만 흔들림 (크게 흔들면 칼이 뒤로 감)
                Soft(ArmR, _armR0 * IdleArmR * Quaternion.Euler(swing * 0.05f, 0f, 0f), 12f);
                Soft(ForeArmR, _foreR0 * IdleForeR, 12f);
                Soft(HandR, _handR0 * IdleHandR, 12f);
                Soft(Sword, SwordIdle, 14f);

                Soft(ArmL, _armL0 * IdleArmL * Quaternion.Euler(-swing * 0.25f, 0f, 0f), 12f);
                Soft(ForeArmL, _foreL0 * IdleForeL, 12f);
                Soft(HandL, _handL0 * IdleHandL, 10f);
                Soft(Shield, _shield0, 10f);
                Soft(Chest, _chest0Rot * Quaternion.Euler(0f, swing * 0.08f, 0f), 10f);
            }
            else
            {
                if (Hip != null)
                {
                    Hip.localPosition = Vector3.Lerp(Hip.localPosition, _hip0, 9f * Time.deltaTime);
                }

                Soft(LegL, _legL0, 9f);
                Soft(LegR, _legR0, 9f);
                Soft(ArmR, _armR0 * IdleArmR, 8f);
                Soft(ArmL, _armL0 * IdleArmL, 8f);
                Soft(ForeArmR, _foreR0 * IdleForeR, 8f);
                Soft(ForeArmL, _foreL0 * IdleForeL, 8f);
                Soft(HandR, _handR0 * IdleHandR, 8f);
                Soft(HandL, _handL0 * IdleHandL, 8f);
                Soft(Sword, SwordIdle, 9f);
                Soft(Shield, _shield0, 8f);
                Soft(Chest, _chest0Rot, 8f);
            }
        }

        private void AnimateAttack()
        {
            _attackT += Time.deltaTime;
            var t = _attackT;
            var windup = WindupDuration(_combo);
            var strike = StrikeDuration(_combo);
            var recover = RecoverDuration(_combo);
            var strikeEnd = windup + strike;
            var total = strikeEnd + recover;

            GetPoses(out var wArmR, out var sArmR, out var wForeR, out var sForeR,
                out var wArmL, out var sArmL, out var wForeL, out var sForeL,
                out var wHand, out var sHand, out var wChest, out var sChest);

            if (t < windup)
            {
                var p = EaseOut(t / windup);
                Blend(p, _sArmR, wArmR, _sArmL, wArmL, _sForeR, wForeR, _sForeL, wForeL,
                    _sHandR, wHand, _sChest, wChest, 0f, swordTwist: false, windupSword: true);
            }
            else if (t < strikeEnd)
            {
                if (t - windup < Time.deltaTime * 1.5f)
                {
                    _trail?.Begin(strike + 0.06f, _combo);
                }

                var p = EaseOutQuart((t - windup) / strike);
                // 초반에 빠르게 치고 끝에서 살짝 감속
                var snap = p < 0.55f
                    ? EaseOutQuart(p / 0.55f) * 0.72f
                    : 0.72f + (p - 0.55f) / 0.45f * 0.28f;
                var lunge = Mathf.Sin(Mathf.Clamp01(p) * Mathf.PI) * (_combo == 2 ? 0.09f : 0.055f);
                Blend(snap, wArmR, sArmR, wArmL, sArmL, wForeR, sForeR, wForeL, sForeL,
                    wHand, sHand, wChest, sChest, lunge, swordTwist: true);
                Soft(Head, _head0Rot * Quaternion.Euler(10f, 0f, _combo == 1 ? 8f : -6f), 14f);

                var stance = _combo == 2 ? 18f : 12f;
                Soft(LegL, _legL0 * Quaternion.Euler(stance, 0f, 0f), 20f);
                Soft(LegR, _legR0 * Quaternion.Euler(-stance * 0.6f, 0f, 0f), 20f);
            }
            else if (t < total)
            {
                var p = EaseOut((t - strikeEnd) / recover);
                var gArmR = _armR0 * IdleArmR;
                var gForeR = _foreR0 * IdleForeR;
                var gArmL = _armL0 * IdleArmL;
                var gForeL = _foreL0 * IdleForeL;
                var gHand = _handR0 * IdleHandR;
                Blend(p, sArmR, gArmR, sArmL, gArmL, sForeR, gForeR, sForeL, gForeL,
                    sHand, gHand, sChest, _chest0Rot, 0f);
                Soft(Head, _head0Rot, 10f);
                Soft(LegL, _legL0, 10f);
                Soft(LegR, _legR0, 10f);
                if (Hip != null)
                {
                    Hip.localPosition = Vector3.Lerp(Hip.localPosition, _hip0, 10f * Time.deltaTime);
                }
            }
            else
            {
                _attackT = -1f;
            }
        }

        private void Blend(float p,
            Quaternion aR, Quaternion bR, Quaternion aL, Quaternion bL,
            Quaternion fRa, Quaternion fRb, Quaternion fLa, Quaternion fLb,
            Quaternion hA, Quaternion hB, Quaternion cA, Quaternion cB, float hipZ,
            bool swordTwist = false, bool windupSword = false)
        {
            Set(ArmR, Quaternion.Slerp(aR, bR, p));
            Set(ArmL, Quaternion.Slerp(aL, bL, p));
            Set(ForeArmR, Quaternion.Slerp(fRa, fRb, p));
            Set(ForeArmL, Quaternion.Slerp(fLa, fLb, p));
            Set(HandR, Quaternion.Slerp(hA, hB, p));
            Set(Chest, Quaternion.Slerp(cA, cB, p));

            if (Sword != null)
            {
                if (swordTwist)
                {
                    // 스트라이크 중 검이 팔과  lagged Soft 없이 궤적을 따라감
                    var twist = _sword0 * Quaternion.Euler(
                        Mathf.Sin(p * Mathf.PI) * (_combo == 2 ? 28f : 14f),
                        Mathf.Lerp(22f, -28f, p),
                        Mathf.Sin(p * Mathf.PI) * (_combo == 1 ? -18f : 14f));
                    Sword.localRotation = twist;
                }
                else if (windupSword)
                {
                    // 윈드업: 검을 뒤로 젖혀 예비 동작
                    var raise = _sword0 * Quaternion.Euler(
                        Mathf.Lerp(0f, _combo == 2 ? -18f : -8f, p),
                        Mathf.Lerp(0f, _combo == 1 ? -16f : 14f, p),
                        Mathf.Lerp(0f, 8f, p));
                    Sword.localRotation = Quaternion.Slerp(_sSword, raise, p);
                }
                else
                {
                    Soft(Sword, SwordIdle, 16f);
                }
            }

            Soft(Shield, _shield0, 12f);
            Soft(HandL, _handL0 * IdleHandL, 10f);
            if (Hip != null)
            {
                Hip.localPosition = _hip0 + Vector3.forward * hipZ
                    + Vector3.up * (swordTwist ? Mathf.Sin(p * Mathf.PI) * 0.04f : 0f);
            }
        }

        private void GetPoses(
            out Quaternion wArmR, out Quaternion sArmR, out Quaternion wForeR, out Quaternion sForeR,
            out Quaternion wArmL, out Quaternion sArmL, out Quaternion wForeL, out Quaternion sForeL,
            out Quaternion wHand, out Quaternion sHand, out Quaternion wChest, out Quaternion sChest)
        {
            switch (_combo)
            {
                case 1: // 역베기 — 더 넓은 호
                    wArmR = _armR0 * Quaternion.Euler(-85f, -78f, 28f);
                    sArmR = _armR0 * Quaternion.Euler(38f, 88f, -42f);
                    wForeR = _foreR0 * Quaternion.Euler(-4f, 12f, 12f);
                    sForeR = _foreR0 * Quaternion.Euler(-82f, -12f, 22f);
                    wArmL = _armL0 * Quaternion.Euler(38f, 34f, -22f);
                    sArmL = _armL0 * IdleArmL;
                    wForeL = _foreL0 * Quaternion.Euler(-8f, 0f, 0f);
                    sForeL = _foreL0 * IdleForeL;
                    wHand = _handR0 * Quaternion.Euler(12f, 18f, 36f);
                    sHand = _handR0 * Quaternion.Euler(-12f, -22f, -52f);
                    wChest = _chest0Rot * Quaternion.Euler(-4f, -28f, 6f);
                    sChest = _chest0Rot * Quaternion.Euler(10f, 28f, -6f);
                    break;
                case 2: // 내려찍기 — 높이 들었다가 강하게
                    wArmR = _armR0 * Quaternion.Euler(-128f, 18f, -18f);
                    sArmR = _armR0 * Quaternion.Euler(52f, -18f, -28f);
                    wForeR = _foreR0 * Quaternion.Euler(8f, 0f, 0f);
                    sForeR = _foreR0 * Quaternion.Euler(-92f, 0f, 6f);
                    wArmL = _armL0 * Quaternion.Euler(42f, 8f, -18f);
                    sArmL = _armL0 * IdleArmL;
                    wForeL = _foreL0 * Quaternion.Euler(-6f, 0f, 0f);
                    sForeL = _foreL0 * IdleForeL;
                    wHand = _handR0 * Quaternion.Euler(-28f, 0f, 12f);
                    sHand = _handR0 * Quaternion.Euler(36f, 0f, -16f);
                    wChest = _chest0Rot * Quaternion.Euler(-16f, 10f, 0f);
                    sChest = _chest0Rot * Quaternion.Euler(22f, -12f, 0f);
                    break;
                default: // 가로베기
                    wArmR = _armR0 * Quaternion.Euler(-68f, 82f, -48f);
                    sArmR = _armR0 * Quaternion.Euler(28f, -88f, -16f);
                    wForeR = _foreR0 * Quaternion.Euler(-4f, 8f, -6f);
                    sForeR = _foreR0 * Quaternion.Euler(-78f, -8f, -24f);
                    wArmL = _armL0 * Quaternion.Euler(32f, -34f, 22f);
                    sArmL = _armL0 * IdleArmL;
                    wForeL = _foreL0 * Quaternion.Euler(-6f, 0f, 0f);
                    sForeL = _foreL0 * IdleForeL;
                    wHand = _handR0 * Quaternion.Euler(6f, 28f, 34f);
                    sHand = _handR0 * Quaternion.Euler(-8f, -34f, -48f);
                    wChest = _chest0Rot * Quaternion.Euler(-3f, 24f, -5f);
                    sChest = _chest0Rot * Quaternion.Euler(8f, -26f, 5f);
                    break;
            }
        }

        private static void Set(Transform t, Quaternion r)
        {
            if (t != null)
            {
                t.localRotation = r;
            }
        }

        private static void Soft(Transform t, Quaternion target, float speed)
        {
            if (t != null)
            {
                t.localRotation = Quaternion.Slerp(t.localRotation, target, speed * Time.deltaTime);
            }
        }

        private static float EaseOut(float x)
        {
            var u = 1f - x;
            return 1f - u * u * u;
        }

        private static float EaseOutQuart(float x)
        {
            var u = 1f - x;
            return 1f - u * u * u * u;
        }
    }
}
