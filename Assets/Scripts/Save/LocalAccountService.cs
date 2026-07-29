using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace DungeonOdyssey.Save
{
    /// <summary>로컬 회원가입·로그인 (persistentDataPath JSON).</summary>
    public static class LocalAccountService
    {
        private const string FileName = "accounts.json";

        public static string CurrentUsername { get; private set; } = "";
        public static string CurrentDisplayName { get; private set; } = "";
        public static bool IsGuest { get; private set; }
        public static bool IsLoggedIn => !string.IsNullOrEmpty(CurrentUsername);

        [Serializable]
        private class AccountRecord
        {
            public string username;
            public string passwordHash;
            public string displayName;
            public string createdUtc;
        }

        [Serializable]
        private class AccountDb
        {
            public AccountRecord[] accounts = Array.Empty<AccountRecord>();
        }

        public static bool TrySignUp(string username, string password, string displayName, out string error)
        {
            error = null;
            username = SanitizeUser(username);
            displayName = string.IsNullOrWhiteSpace(displayName) ? username : displayName.Trim();
            if (username.Length < 3)
            {
                error = "아이디는 3자 이상이어야 합니다.";
                return false;
            }

            if (string.IsNullOrEmpty(password) || password.Length < 4)
            {
                error = "비밀번호는 4자 이상이어야 합니다.";
                return false;
            }

            var db = LoadDb();
            var list = new List<AccountRecord>(db.accounts ?? Array.Empty<AccountRecord>());
            if (FindIn(list, username) != null)
            {
                error = "이미 존재하는 아이디입니다.";
                return false;
            }

            list.Add(new AccountRecord
            {
                username = username,
                passwordHash = Hash(password),
                displayName = displayName,
                createdUtc = DateTime.UtcNow.ToString("o")
            });
            db.accounts = list.ToArray();
            SaveDb(db);
            CurrentUsername = username;
            CurrentDisplayName = displayName;
            IsGuest = false;
            return true;
        }

        public static bool TryLogin(string username, string password, out string error)
        {
            error = null;
            username = SanitizeUser(username);
            if (username.Length < 1)
            {
                error = "아이디를 입력하세요.";
                return false;
            }

            var db = LoadDb();
            var list = new List<AccountRecord>(db.accounts ?? Array.Empty<AccountRecord>());
            var rec = FindIn(list, username);
            if (rec == null)
            {
                error = "계정을 찾을 수 없습니다. 먼저 가입하세요.";
                return false;
            }

            if (!string.Equals(rec.passwordHash, Hash(password ?? ""), StringComparison.Ordinal))
            {
                error = "비밀번호가 올바르지 않습니다.";
                return false;
            }

            CurrentUsername = rec.username;
            CurrentDisplayName = string.IsNullOrEmpty(rec.displayName) ? rec.username : rec.displayName;
            IsGuest = false;
            return true;
        }

        public static void LoginAsGuest()
        {
            var tag = UnityEngine.Random.Range(1000, 9999);
            CurrentUsername = $"guest_{tag}";
            CurrentDisplayName = $"게스트{tag}";
            IsGuest = true;
        }

        public static void Logout()
        {
            CurrentUsername = "";
            CurrentDisplayName = "";
            IsGuest = false;
        }

        private static string SanitizeUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return "";
            }

            return username.Trim().ToLowerInvariant();
        }

        private static AccountRecord FindIn(List<AccountRecord> list, string username)
        {
            if (list == null)
            {
                return null;
            }

            foreach (var a in list)
            {
                if (a != null && string.Equals(a.username, username, StringComparison.OrdinalIgnoreCase))
                {
                    return a;
                }
            }

            return null;
        }

        private static string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes("dungeon-odyssey|" + password));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        private static string Path => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        private static AccountDb LoadDb()
        {
            try
            {
                if (!System.IO.File.Exists(Path))
                {
                    return new AccountDb();
                }

                var json = System.IO.File.ReadAllText(Path);
                var db = JsonUtility.FromJson<AccountDb>(json);
                return db ?? new AccountDb();
            }
            catch
            {
                return new AccountDb();
            }
        }

        private static void SaveDb(AccountDb db)
        {
            try
            {
                var json = JsonUtility.ToJson(db, true);
                System.IO.File.WriteAllText(Path, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"계정 저장 실패: {e.Message}");
            }
        }
    }
}
