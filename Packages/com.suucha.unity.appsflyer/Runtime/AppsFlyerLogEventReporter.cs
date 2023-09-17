using System.Collections.Generic;
using AppsFlyerSDK;
using SuuchaStudio.Unity.Core.LogEvents;
using SuuchaStudio.Unity.Core;
using Cysharp.Threading.Tasks;

namespace SuuchaStudio.Unity.LogEvents.AppsFlyers
{
    public class AppsFlyerLogEventReporter : LogEventReporterAbstract
    {
        private readonly bool onlyAfPurchaseWithRevenue = true;
        public AppsFlyerLogEventReporter() : this(true)
        {

        }
        public AppsFlyerLogEventReporter(bool onlyAfPurchaseWithRevenue)
        {
            this.onlyAfPurchaseWithRevenue = onlyAfPurchaseWithRevenue;
        }
        public AppsFlyerLogEventReporter(List<string> allowedEventNames,
            List<string> excludedEventNames,
            Dictionary<string, string> eventNameMap,
            Dictionary<string, string> eventParameterNameMap)
            : base(allowedEventNames, excludedEventNames, eventNameMap, eventParameterNameMap)
        {
            onlyAfPurchaseWithRevenue = true;
        }
        public override string Name => "AppsFlyer";

        protected override UniTask LogEventInternal(string name, Dictionary<string, string> eventParameters)
        {
            if (onlyAfPurchaseWithRevenue && name != "af_purchase")
            {
                if (eventParameters.ContainsKey("af_revenue"))
                {
                    eventParameters.Remove("af_revenue");
                }
            }
            Suucha.App.QueueOnMainThread(() =>
            {
                AppsFlyer.sendEvent(name, eventParameters);
            });
            return UniTask.CompletedTask;
        }

    }
}
