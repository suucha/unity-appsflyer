using AppsFlyerSDK;
using SuuchaStudio.Unity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuuchaStudio.Unity.AppsFlyers
{
    public delegate void ConversionDataSuccessHandler(Dictionary<string, object> conversionData);
    public delegate void ConversionDataFailHandler(string error);
    public delegate void AppOpenAttributionHandler(string data);
    public delegate void AppOpenAttributionFailureHandler(string error);
    public class AppsFlyerConversionData : SuuchaMonoBehaviourBase, IAppsFlyerConversionData
    {
        private event ConversionDataSuccessHandler ConversionDataSuccess;
        public event ConversionDataSuccessHandler OnConversionDataSuccess
        {
            add
            {
                ConversionDataSuccess += value;
            }
            remove
            {
                ConversionDataSuccess -= value;
            }
        }

        private event ConversionDataFailHandler ConversionDataFail;
        public event ConversionDataFailHandler OnConversionDataFail
        {
            add { ConversionDataFail += value; }
            remove { ConversionDataFail -= value; }
        }

        private event AppOpenAttributionHandler AppOpenAttribution;
        public event AppOpenAttributionHandler OnAppOpenAttribution
        {
            add => AppOpenAttribution += value;
            remove => AppOpenAttribution -= value;
        }

        private event AppOpenAttributionFailureHandler AppOpenAttributionFailure;
        public event AppOpenAttributionFailureHandler OnAppOpenAttributionFailure
        {
            add => AppOpenAttributionFailure += value;
            remove => OnAppOpenAttributionFailure -= value;
        }

        public void onAppOpenAttribution(string attributionData)
        {
            Logger.LogDebug("On app open attribution:" + attributionData);
            AppOpenAttribution?.Invoke(attributionData);
        }

        public void onAppOpenAttributionFailure(string error)
        {
            Logger.LogError("On app open attribution failure:" + error);
            AppOpenAttributionFailure?.Invoke(error);
        }

        public void onConversionDataFail(string error)
        {
            Logger.LogError("On conversion data fail:" + error);
            ConversionDataFail?.Invoke(error);
        }

        public void onConversionDataSuccess(string conversionData)
        {
            Logger.LogDebug($"On Conversion data success:{conversionData}");
            try
            {
                Dictionary<string, object> dictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
                dictionary.Add("Original", conversionData);
                ConversionDataSuccess?.Invoke(dictionary);
            }
            catch (Exception ex)
            {
                Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
                dictionary2.Add("Original", conversionData);
                ConversionDataSuccess?.Invoke(dictionary2);
                Logger.LogError("On conversion data error:" + ex.Message + ", conversion data:" + conversionData);
            }
        }
    }

}
