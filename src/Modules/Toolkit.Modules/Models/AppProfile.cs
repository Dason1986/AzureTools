using AzureTools;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Toolkit.Modules.Models
{
    public class AppProfile
    {
        #region Constructors

        public AppProfile() : this(new Dictionary<string, string>())
        {

        }
        public AppProfile(IDictionary<string, string> valuePairs)
        {
            this.valuePairs = valuePairs;
        }

        #endregion

        #region Fields & Properties

        private readonly IDictionary<string, string> valuePairs;

        public string Name { get => GetString(nameof(Name)); set => SetString(nameof(Name), value); }

        public TenantMode TenantMode
        {
            get
            {
                string val = GetString(nameof(TenantMode));
                if (!string.IsNullOrEmpty(val) && Enum.TryParse<TenantMode>(val, true, out TenantMode tenantMode)) return tenantMode;
                return default;
            }

            set
            {
                SetString(nameof(TenantMode), value.ToString());
            }
        }
        public bool LoginUsePassword
        {
            get
            {
                string val = GetString(nameof(LoginUsePassword));
                if (!string.IsNullOrEmpty(val) && bool.TryParse(val, out bool flag)) return flag;
                return default;
            }

            set
            {
                SetString(nameof(LoginUsePassword), value.ToString());
            }
        }

        public ThemeMode ThemeMode
        {
            get
            {
                string val = GetString(nameof(ThemeMode));
                if (!string.IsNullOrEmpty(val) && Enum.TryParse<ThemeMode>(val, true, out ThemeMode mode)) return mode;
                return ThemeMode.System;
            }

            set
            {
                SetString(nameof(ThemeMode), value.ToString());
            }
        }

        #endregion

        protected virtual void SetString(string key, string val)
        {
            _ = valuePairs.Remove(key, out string _);
            valuePairs.TryAdd(key, val);
        }
        protected virtual string GetString(string key)
        {
            return valuePairs.TryGetValue(key, out string str) ? str.Trim() : string.Empty;

        }
        public void SetPassword(string password)
        {
            string loginPassword = HashPassword(password);
            SetString("LoginPassword", loginPassword);
            LoginUsePassword = true;
        }

        public bool IsAuthenticated(string password)
        {
            if (!LoginUsePassword) return true;
            if (string.IsNullOrEmpty(password)) return false;
            string LoginPassword = GetString("LoginPassword");
            return LoginPassword == HashPassword(password);
        }

        public void ClearPassword()
        {
            SetString("LoginPassword", string.Empty);
            LoginUsePassword = false;
        }

        private static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return null;
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        public IDictionary<string, string> GetValues() => valuePairs.AsReadOnly();
    }
}
