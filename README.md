# Suucha ADKU - AppsFlyer

[AppsFlyer](https://appsflyer.com)是一种移动应用分析工具，帮助开发者和营销团队追踪移动应用的用户行为和广告活动的效果。
## 接入Suucha ADKU AppsFlyer
修改Packages/manifest.json文件在dependencies中添加：
``` json
"dependencies": {
  "appsflyer-unity-plugin": "https://github.com/AppsFlyerSDK/appsflyer-unity-plugin.git#upm",
  "com.suucha.unity.appsflyer":"1.0.0"，

  //...
 }
```
## 开始
在实现了接口IAfterSuuchaInit的类的Execute方法中，用以下代码初始化AppsFlyer和启用AppsFlyer的事件上报器：
``` csharp
public class AfterSuuchaInit: IAfterSuuchaInit
{
    public void Execute()
    {
        //初始化AppsFlyer
        Suucha.App.InitAppsFlyer("your appsflyer dev key");
        //初始化AppsFlyer Log Event Reproter并启用
        //如果你不想上报埋点事件到AppsFlyer不用此代码
        Suucha.App.InitAppsFlyerLogEventReporter().Use();

        //其他逻辑
        // ...
    }
}
```
初始化AppsFlyer时可以设置某些感兴趣的回调，InitAppsFlyer方法的完整签名如下：
``` csharp
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
```
