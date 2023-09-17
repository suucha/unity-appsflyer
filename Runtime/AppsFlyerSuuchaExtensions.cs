using AppsFlyerSDK;
using SuuchaStudio.Unity.Core;
using SuuchaStudio.Unity.LogEvents.AppsFlyers;
using System;
using System.Collections.Generic;

namespace SuuchaStudio.Unity.AppsFlyers
{
    public static class AppsFlyerSuuchaExtensions
    {

        private static bool isInited = false;
        private static AppsFlyerConversionData callback;
        private static Action<Dictionary<string, object>> onConversionDataSuccess;
        private static Action<string> onConversionDataFail;
        private static Action<string> onAppOpenAttribution;
        private static Action<string> onAppOpenAttributionFailure;
        private static bool conversionDataProcessed = false;
        private static Suucha suucha;
        private static Dictionary<string, string> mediaSourceAdCreativeKeyNameMap;
        /// <summary>
        /// 初始化AppsFlyer
        /// </summary>
        /// <param name="suucha"></param>
        /// <param name="devKey">AppsFlyer devKey</param>
        /// <param name="bundleId">应用包名，可以为空</param>
        /// <param name="onConversionDataSuccess">初始化成功回调</param>
        /// <param name="onConversionDataFail">初始化失败回调</param>
        /// <param name="onAppOpenAttribution">应用打开，归因成功回调</param>
        /// <param name="onAppOpenAttributionFailure">应用打开，归因失败回调</param>
        /// <param name="waitForCustomerUserId">是否等待CustomerUserId设置后再上报事件</param>
        /// <param name="mediaSourceAdCreativeKeyNameMap"></param>
        /// <returns></returns>
        public static Suucha InitAppsFlyer(this Suucha suucha, string devKey, string bundleId = null,
           Action<Dictionary<string, object>> onConversionDataSuccess = null,
           Action<string> onConversionDataFail = null,
           Action<string> onAppOpenAttribution = null,
           Action<string> onAppOpenAttributionFailure = null,
           bool waitForCustomerUserId = false,
           Dictionary<string, string> mediaSourceAdCreativeKeyNameMap = null)
        {
            if (isInited)
            {
                return suucha;
            }
            AppsFlyerSuuchaExtensions.suucha = suucha;
            if (string.IsNullOrEmpty(Suucha.App.CustomerUserId))
            {
                Suucha.App.OnCustomerUserIdChanged += OnCustomerUserIdChanged;
            }
            else
            {
                AppsFlyer.setCustomerUserId(Suucha.App.CustomerUserId);
            }
            AppsFlyer.waitForCustomerUserId(waitForCustomerUserId);
            callback = suucha.AddComponent<AppsFlyerConversionData>();
            callback.OnConversionDataSuccess += OnConversionDataSuccessInternal;
            callback.OnConversionDataFail += OnConversionDataFailInternal;
            callback.OnAppOpenAttribution += OnAppOpenAttributionInternal;
            callback.OnAppOpenAttributionFailure += OnAppOpenAttributionFailureInternal;
            if (onConversionDataSuccess != null)
            {
                AppsFlyerSuuchaExtensions.onConversionDataSuccess = onConversionDataSuccess;
            }
            if (onConversionDataFail != null)
            {
                AppsFlyerSuuchaExtensions.onConversionDataFail = onConversionDataFail;
            }
            if (onAppOpenAttribution != null)
            {
                AppsFlyerSuuchaExtensions.onAppOpenAttribution = onAppOpenAttribution;
            }
            if (onAppOpenAttributionFailure != null)
            {
                AppsFlyerSuuchaExtensions.onAppOpenAttributionFailure = onAppOpenAttributionFailure;
            }
            AppsFlyerSuuchaExtensions.mediaSourceAdCreativeKeyNameMap = mediaSourceAdCreativeKeyNameMap;
            if (AppsFlyerSuuchaExtensions.mediaSourceAdCreativeKeyNameMap == null)
            {
                AppsFlyerSuuchaExtensions.mediaSourceAdCreativeKeyNameMap = new Dictionary<string, string>();
            }
            suucha.BeginGetAttribution();
            if (string.IsNullOrEmpty(bundleId))
            {
                bundleId = UnityEngine.Application.identifier;
            }
            AppsFlyer.initSDK(devKey, bundleId, callback);
            AppsFlyer.startSDK();
            isInited = true;
            return suucha;
        }

        private static void OnAppOpenAttributionFailureInternal(string error)
        {
            onAppOpenAttributionFailure?.Invoke(error);
        }

        private static void OnAppOpenAttributionInternal(string data)
        {
            onAppOpenAttribution?.Invoke(data);
        }

        private static void OnConversionDataFailInternal(string error)
        {
            onConversionDataFail?.Invoke(error);
        }

        private static async void OnConversionDataSuccessInternal(Dictionary<string, object> conversionData)
        {
            if (conversionDataProcessed)
            {
                return;
            }
            var status = "";
            var mediaSource = "";
            if (conversionData.ContainsKey("af_status"))
            {
                status = conversionData["af_status"] as string;
            }
            bool isOrganic;
            if (!string.IsNullOrEmpty(status) && status.ToLower() == "non-organic")
            {
                isOrganic = false;
            }
            else
            {
                isOrganic = true;
            }
            if (conversionData.ContainsKey("media_source"))
            {
                if (conversionData["media_source"] != null)
                {
                    mediaSource = conversionData["media_source"].ToString();
                }
            }
            var adSet = GetAdSet(conversionData, mediaSource);
            var adCampaign = "";
            if (conversionData.ContainsKey("campaign"))
            {
                if (conversionData["campaign"] != null)
                {
                    adCampaign = conversionData["campaign"].ToString();
                }
            }
            var adCampaignId = "";
            if (conversionData.ContainsKey("campaign_id"))
            {
                if (conversionData["campaign_id"] != null)
                {
                    adCampaignId = conversionData["campaign_id"].ToString();
                }
            }
            var adSiteId = "";
            if (conversionData.ContainsKey("af_siteid"))
            {
                if (conversionData["af_siteid"] != null)
                {
                    adSiteId = conversionData["af_siteid"].ToString();
                }
            }
            var cpi = "0";
            if (conversionData.ContainsKey("cost_cents_USD"))
            {
                if (conversionData["cost_cents_USD"] != null)
                {
                    cpi = conversionData["cost_cents_USD"].ToString();
                }
            }
            await suucha.SetAttribution(isOrganic, mediaSource, adCampaign, adSet, adSiteId, adCampaignId, cpi);
            conversionDataProcessed = true;
            onConversionDataSuccess?.Invoke(conversionData);
        }
        private static string GetAdSet(Dictionary<string, object> conversionData, string mediaSource)
        {
            var adSet = "";
            var adCreativeKeyName = "af_ad";
            if (mediaSourceAdCreativeKeyNameMap.ContainsKey(mediaSource))
            {
                adCreativeKeyName = mediaSourceAdCreativeKeyNameMap[mediaSource];
            }
            if (conversionData.ContainsKey(adCreativeKeyName))
            {
                adSet = conversionData[adCreativeKeyName].ToString();
            }
            return adSet;
        }

        private static void OnCustomerUserIdChanged(string oldId, string newId)
        {
            AppsFlyer.setCustomerUserId(newId);
        }
        private static bool appsFlyerLogEventReporterAdded = false;
        private static AppsFlyerLogEventReporter reporter;
        public static AppsFlyerLogEventReporter InitAppsFlyerLogEventReporter(this Suucha suucha,
            List<string> allowedEventNames = null,
            List<string> excludedEventNames = null,
            Dictionary<string, string> eventNameMap = null,
            Dictionary<string, string> eventParameterNameMap = null)
        {
            if (!appsFlyerLogEventReporterAdded)
            {
                reporter = new AppsFlyerLogEventReporter(allowedEventNames, excludedEventNames, eventNameMap, eventParameterNameMap);
                appsFlyerLogEventReporterAdded = true;
            }
            return reporter;
        }
    }
}
