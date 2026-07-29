using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Player;
using DungeonOdyssey.Save;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonOdyssey.UI
{
    public class TitleUI : MonoBehaviour
    {
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Text statusText;
        [SerializeField] private Button[] slotButtons = new Button[3];

        private int _slot;
        private Image[] _slotAccents = new Image[3];

        private GameObject _authRoot;
        private GameObject _playRoot;
        private InputField _userField;
        private InputField _passwordField;
        private InputField _displayField;
        private Text _authStatus;
        private Text _accountLabel;
        private Button _submitBtn;
        private Button _modeLoginBtn;
        private Button _modeSignUpBtn;
        private Button _guestBtn;
        private bool _signUpMode;

        private void Start()
        {
            EnsureGameManager();
            WireButtons();
            ShowAuthOrPlay();
        }

        private void WireButtons()
        {
            if (newGameButton != null)
            {
                newGameButton.onClick.RemoveAllListeners();
                newGameButton.onClick.AddListener(OnNewGame);
            }

            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(OnContinue);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveAllListeners();
                quitButton.onClick.AddListener(OnQuit);
            }

            for (var i = 0; i < slotButtons.Length; i++)
            {
                var idx = i;
                if (slotButtons[i] == null)
                {
                    continue;
                }

                slotButtons[i].onClick.RemoveAllListeners();
                slotButtons[i].onClick.AddListener(() => SelectSlot(idx));
                var accent = slotButtons[i].transform.Find("Accent")?.GetComponent<Image>();
                _slotAccents[i] = accent;
            }
        }

        public void Bind(Button newGame, Button continueBtn, Button quit, Text status,
            Button slot1 = null, Button slot2 = null, Button slot3 = null)
        {
            newGameButton = newGame;
            continueButton = continueBtn;
            quitButton = quit;
            statusText = status;
            slotButtons = new[] { slot1, slot2, slot3 };
        }

        public void BindAuth(GameObject authRoot, GameObject playRoot, InputField user, InputField password,
            InputField display, Text authStatus, Text accountLabel,
            Button submitBtn, Button guestBtn, Button logoutBtn,
            Button modeLoginBtn, Button modeSignUpBtn)
        {
            _authRoot = authRoot;
            _playRoot = playRoot;
            _userField = user;
            _passwordField = password;
            _displayField = display;
            _authStatus = authStatus;
            _accountLabel = accountLabel;
            _submitBtn = submitBtn;
            _modeLoginBtn = modeLoginBtn;
            _modeSignUpBtn = modeSignUpBtn;
            _guestBtn = guestBtn;

            submitBtn?.onClick.AddListener(OnSubmit);
            guestBtn?.onClick.AddListener(OnGuest);
            logoutBtn?.onClick.AddListener(OnLogout);
            modeLoginBtn?.onClick.AddListener(() => SetSignUpMode(false));
            modeSignUpBtn?.onClick.AddListener(() => SetSignUpMode(true));
            SetSignUpMode(false);
        }

        private void ShowAuthOrPlay()
        {
            var loggedIn = LocalAccountService.IsLoggedIn;
            if (_authRoot != null)
            {
                _authRoot.SetActive(!loggedIn);
            }

            if (_playRoot != null)
            {
                _playRoot.SetActive(loggedIn);
            }

            if (loggedIn)
            {
                RefreshAccountLabel();
                RefreshContinueState();
                RefreshSlotVisuals();
            }
            else
            {
                SetAuthStatus(_signUpMode
                    ? "새 모험가 계정을 만드세요."
                    : "아이디와 비밀번호를 입력하세요.");
            }
        }

        private void SetSignUpMode(bool signUp)
        {
            _signUpMode = signUp;
            if (_displayField != null)
            {
                _displayField.gameObject.SetActive(signUp);
            }

            // 이름 칸이 열리면 아래 요소를 내려 간격 유지
            if (_submitBtn != null)
            {
                var rt = _submitBtn.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(0f, signUp ? -104f : -72f);
                }

                var label = _submitBtn.transform.Find("Label")?.GetComponent<Text>();
                if (label != null)
                {
                    label.text = signUp ? "가입하기" : "로그인";
                }
            }

            if (_guestBtn != null)
            {
                var gr = _guestBtn.GetComponent<RectTransform>();
                if (gr != null)
                {
                    gr.anchoredPosition = new Vector2(0f, signUp ? -146f : -118f);
                }
            }

            if (_authStatus != null)
            {
                var sr = _authStatus.rectTransform;
                sr.anchoredPosition = new Vector2(0f, signUp ? -172f : -148f);
            }

            StyleModeTab(_modeLoginBtn, !signUp);
            StyleModeTab(_modeSignUpBtn, signUp);
            SetAuthStatus(signUp
                ? "새 모험가 계정을 만드세요."
                : "아이디와 비밀번호를 입력하세요.");
        }

        private static void StyleModeTab(Button btn, bool active)
        {
            if (btn == null)
            {
                return;
            }

            var label = btn.transform.Find("Label")?.GetComponent<Text>();
            if (label != null)
            {
                label.color = active
                    ? new Color(0.55f, 0.92f, 0.88f, 1f)
                    : new Color(0.55f, 0.6f, 0.64f, 0.9f);
                label.fontStyle = active ? FontStyle.Bold : FontStyle.Normal;
            }
        }

        private void OnSubmit()
        {
            CombatAudio.UiClick();
            if (_signUpMode)
            {
                if (LocalAccountService.TrySignUp(
                        _userField != null ? _userField.text : "",
                        _passwordField != null ? _passwordField.text : "",
                        _displayField != null ? _displayField.text : "",
                        out var err))
                {
                    ShowAuthOrPlay();
                }
                else
                {
                    SetAuthStatus(err);
                }
            }
            else
            {
                if (LocalAccountService.TryLogin(_userField != null ? _userField.text : "",
                        _passwordField != null ? _passwordField.text : "", out var err))
                {
                    ShowAuthOrPlay();
                }
                else
                {
                    SetAuthStatus(err);
                }
            }
        }

        private void OnGuest()
        {
            CombatAudio.UiClick();
            LocalAccountService.LoginAsGuest();
            ShowAuthOrPlay();
        }

        private void OnLogout()
        {
            CombatAudio.UiClick();
            LocalAccountService.Logout();
            ShowAuthOrPlay();
        }

        private void RefreshAccountLabel()
        {
            if (_accountLabel == null)
            {
                return;
            }

            var guest = LocalAccountService.IsGuest ? " · 게스트" : "";
            _accountLabel.text = $"{LocalAccountService.CurrentDisplayName}{guest}";
        }

        private void SetAuthStatus(string msg)
        {
            if (_authStatus != null)
            {
                _authStatus.text = msg ?? "";
            }
        }

        private void SelectSlot(int index)
        {
            _slot = Mathf.Clamp(index, 0, 2);
            RefreshContinueState();
            RefreshSlotVisuals();
            CombatAudio.UiClick();
        }

        private void RefreshSlotVisuals()
        {
            EnsureGameManager();
            var save = GameManager.Instance?.SaveService;
            var user = LocalAccountService.CurrentUsername;
            for (var i = 0; i < 3; i++)
            {
                var btn = slotButtons != null && i < slotButtons.Length ? slotButtons[i] : null;
                if (btn == null)
                {
                    continue;
                }

                var label = btn.transform.Find("Label")?.GetComponent<Text>();
                var exists = save != null && save.Exists(i);
                var owned = true;
                if (exists && save != null)
                {
                    var data = save.Load(i);
                    if (data != null && !string.IsNullOrEmpty(data.accountUsername)
                        && !string.Equals(data.accountUsername, user, System.StringComparison.OrdinalIgnoreCase))
                    {
                        owned = false;
                    }
                }

                var selected = i == _slot;
                if (label != null)
                {
                    if (!exists)
                    {
                        label.text = $"슬롯 {i + 1}";
                    }
                    else if (!owned)
                    {
                        label.text = $"슬롯 {i + 1} 🔒";
                    }
                    else
                    {
                        label.text = $"슬롯 {i + 1} ●";
                    }

                    label.color = selected
                        ? new Color(0.9f, 0.96f, 0.95f, 1f)
                        : new Color(0.65f, 0.7f, 0.74f, 0.9f);
                }

                if (_slotAccents[i] != null)
                {
                    var c = _slotAccents[i].color;
                    _slotAccents[i].color = new Color(c.r, c.g, c.b, selected ? 0.95f : 0.2f);
                }
            }

            if (statusText != null && save != null)
            {
                statusText.text = DescribeSlot(_slot, save, user);
            }
        }

        private static string DescribeSlot(int slot, ISaveService save, string user)
        {
            if (!save.Exists(slot))
            {
                return $"슬롯 {slot + 1} · 비어 있음 — 새 게임으로 시작";
            }

            var data = save.Load(slot);
            if (data == null)
            {
                return $"슬롯 {slot + 1} · 비어 있음 — 새 게임으로 시작";
            }

            if (!string.IsNullOrEmpty(data.accountUsername)
                && !string.Equals(data.accountUsername, user, System.StringComparison.OrdinalIgnoreCase))
            {
                return $"슬롯 {slot + 1} · 다른 계정 세이브 — 이 계정으로는 열 수 없음";
            }

            var mins = Mathf.FloorToInt(data.playTimeSeconds / 60f);
            var weapon = WeaponCatalog.Label(
                (WeaponId)Mathf.Clamp(data.equippedWeaponId, 0, WeaponCatalog.WeaponCount - 1),
                data.weaponUpgradeLevel);
            var ach = AchievementCatalog.CountUnlocked(data);
            var depth = Mathf.Max(1, data.deepestDepth);
            var questDone = 0;
            for (var i = 0; i < 8; i++)
            {
                if ((data.completedQuestMask & (1 << i)) != 0)
                {
                    questDone++;
                }
            }

            var name = string.IsNullOrEmpty(data.playerName) ? "모험가" : data.playerName;
            return $"{name}  ·  슬롯 {slot + 1}  ·  Lv {data.level}  ·  클리어 {data.dungeonClears}  ·  깊이 {depth}\n" +
                   $"{weapon}  ·  축복 +{data.metaBlessing}  ·  업적 {ach}/6  ·  의뢰 {questDone}/8\n" +
                   $"{data.gold}G  ·  스트릭 {data.bestKillStreak}  ·  {mins}분";
        }

        private void RefreshContinueState()
        {
            EnsureGameManager();
            var save = GameManager.Instance?.SaveService;
            var has = false;
            if (save != null && save.Exists(_slot))
            {
                var data = save.Load(_slot);
                has = data != null && (string.IsNullOrEmpty(data.accountUsername)
                    || string.Equals(data.accountUsername, LocalAccountService.CurrentUsername,
                        System.StringComparison.OrdinalIgnoreCase));
            }

            if (continueButton != null)
            {
                continueButton.interactable = has;
                var label = continueButton.transform.Find("Label")?.GetComponent<Text>();
                if (label != null)
                {
                    label.color = has
                        ? new Color(0.78f, 0.74f, 0.68f, 1f)
                        : new Color(0.45f, 0.42f, 0.4f, 0.7f);
                }
            }
        }

        private static void EnsureGameManager()
        {
            if (GameManager.Instance != null)
            {
                return;
            }

            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }

        private void OnNewGame()
        {
            if (!LocalAccountService.IsLoggedIn)
            {
                SetStatus("먼저 로그인하세요.");
                return;
            }

            GameManager.Instance.StartNewGame(_slot);
        }

        private void OnContinue()
        {
            if (!LocalAccountService.IsLoggedIn)
            {
                SetStatus("먼저 로그인하세요.");
                return;
            }

            if (!GameManager.Instance.TryContinueGame(_slot))
            {
                SetStatus($"슬롯 {_slot + 1}을 이 계정으로 이어할 수 없습니다.");
                RefreshSlotVisuals();
            }
        }

        private void OnQuit()
        {
            GameManager.Instance.QuitGame();
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message ?? string.Empty;
            }
        }
    }
}
