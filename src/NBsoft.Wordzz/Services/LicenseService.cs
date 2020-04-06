using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly WordzzSettings _setting;
        private readonly ILogger _log;
        private readonly Timer _licenseCheckTimer;
        private int validationAttempts;
        private int validationInterval;

        public LicenseService(WordzzSettings setting, ILogger log)
        {
            _setting = setting;
            _log = log;
            _licenseCheckTimer = new Timer(new TimerCallback(LicenseCheckCallBack));
            _licenseCheckTimer.Change(10 * 1000, -1);

            validationAttempts = 0;
            IsLicensed = true;
        }

        public bool IsLicensed { get; private set; }

        private async void LicenseCheckCallBack(object status)
        {
            _licenseCheckTimer.Change(-1, -1);
            try { await CheckLicense(); }
            finally
            {
                _licenseCheckTimer.Change(validationInterval, -1);
            }
        }

        private async Task CheckLicense()
        {
            string licenseAddress = "http://www.nbsoft.pt/V1/Licenses/CheckLicenseV2";

            // lic2.2
            var CheckLicensePars = new Dictionary<string, string>
            {
                {"serverId", string.Format("{0}", _setting.ServerId) },
                {"validationKey", string.Format("{0}", _setting.ApiKey) }
            };
            try
            {
                var response = await SendHttpPostAsync(licenseAddress, CheckLicensePars);
                if (response.StartsWith("Company:"))
                {
                    IsLicensed = true;
                    validationInterval = 24 * 3600 * 1000;
                    _log.WriteInfo(nameof(LicenseService), nameof(CheckLicense), licenseAddress, $"License OK for Server Id: {_setting.ServerId} - {response}");
                }
                else
                {
                    IsLicensed = false;
                    validationInterval = 60 * 1000;
                    _log.WriteWarning(nameof(LicenseService), nameof(CheckLicense), licenseAddress, $"No License for Server Id: {_setting.ServerId}");
                }
                validationAttempts = 0;
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(LicenseService), nameof(CheckLicense), licenseAddress, $"Check License Failed for Server Id: {_setting.ServerId}", ex);
                validationAttempts++;
                validationInterval = 60 * 1000;
                if (validationAttempts > 120)
                    IsLicensed = false;
            }
        }
        private async Task<string> SendHttpPostAsync(string url, Dictionary<string, string> postParameters)
        {
            var content = new System.Net.Http.FormUrlEncodedContent(postParameters);
            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
            {
                var response = await client.PostAsync(url, content);
                var res = await response.Content.ReadAsStringAsync();
                return res;
            }
        }
    }
}
