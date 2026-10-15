using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace CoalcavaDropzone
{
    public static class AuthService
    {
        private const string ClientId = "ce2d41ed-73df-43da-ae41-fbfa9914df9e";
        private const string TenantId = "47d4e187-f644-4ecf-8b7d-e4456388865f";

        private static readonly string[] Scopes = new[] { "User.Read" };
        private static readonly IPublicClientApplication _app;

        // Local cache path in AppData so the session persists across application restarts
        private static readonly string CacheFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CoalcavaDropzone",
            "msal_cache.bin"
        );

        public static string CurrentUserEmail { get; private set; } = string.Empty;
        public static string CurrentDisplayName { get; private set; } = string.Empty;
        public static bool IsSignedIn => !string.IsNullOrWhiteSpace(CurrentUserEmail);

        static AuthService()
        {
            _app = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, TenantId)
                .WithDefaultRedirectUri()
                .Build();

            BindTokenCache();
        }

        private static void BindTokenCache()
        {
            _app.UserTokenCache.SetBeforeAccess(args =>
            {
                lock (CacheFilePath)
                {
                    if (File.Exists(CacheFilePath))
                    {
                        byte[] data = File.ReadAllBytes(CacheFilePath);
                        args.TokenCache.DeserializeMsalV3(data);
                    }
                }
            });

            _app.UserTokenCache.SetAfterAccess(args =>
            {
                if (args.HasStateChanged)
                {
                    lock (CacheFilePath)
                    {
                        string dir = Path.GetDirectoryName(CacheFilePath);
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                        File.WriteAllBytes(CacheFilePath, args.TokenCache.SerializeMsalV3());
                    }
                }
            });
        }

        public static async Task<bool> TrySilentSignInAsync()
        {
            try
            {
                var accounts = await _app.GetAccountsAsync();
                var firstAccount = accounts.FirstOrDefault();
                if (firstAccount == null) return false;

                var result = await _app.AcquireTokenSilent(Scopes, firstAccount).ExecuteAsync();
                SetSessionData(result);
                return true;
            }
            catch
            {
                CurrentUserEmail = string.Empty;
                CurrentDisplayName = string.Empty;
                return false;
            }
        }

        public static async Task<bool> SignInInteractiveAsync(IntPtr windowHandle)
        {
            try
            {
                var accounts = await _app.GetAccountsAsync();
                var firstAccount = accounts.FirstOrDefault();

                AuthenticationResult result;
                try
                {
                    result = await _app.AcquireTokenSilent(Scopes, firstAccount).ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    result = await _app.AcquireTokenInteractive(Scopes)
                        .WithParentActivityOrWindow(windowHandle)
                        .WithPrompt(Prompt.SelectAccount)
                        .ExecuteAsync();
                }

                SetSessionData(result);
                return true;
            }
            catch (Exception ex)
            {
                CurrentUserEmail = string.Empty;
                CurrentDisplayName = string.Empty;
                throw new Exception($"Error de autenticación: {ex.Message}");
            }
        }

        public static async Task SignOutAsync()
        {
            var accounts = await _app.GetAccountsAsync();
            foreach (var account in accounts)
            {
                await _app.RemoveAsync(account);
            }

            CurrentUserEmail = string.Empty;
            CurrentDisplayName = string.Empty;

            try
            {
                if (File.Exists(CacheFilePath))
                {
                    File.Delete(CacheFilePath);
                }
            }
            catch { }
        }

        private static void SetSessionData(AuthenticationResult result)
        {
            CurrentUserEmail = result.Account.Username ?? string.Empty;

            string rawName = result.ClaimsPrincipal?.FindFirst("name")?.Value ?? result.Account.Username ?? string.Empty;

            // Normalize consecutive spaces/tabs to a single space
            CurrentDisplayName = System.Text.RegularExpressions.Regex.Replace(rawName, @"\s+", " ").Trim();
        }
    }
}