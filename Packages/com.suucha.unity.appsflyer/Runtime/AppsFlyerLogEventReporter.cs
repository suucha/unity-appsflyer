using System;
using System.Collections.Generic;
using System.Linq;
using AppsFlyerSDK;
using System.Threading;
using UnityEngine;
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
            Loom.QueueOnMainThread(() =>
            {
                AppsFlyer.sendEvent(name, eventParameters);
            });
            return UniTask.CompletedTask;
        }

    }

    internal class Loom : SuuchaMonoBehaviourBase
    {
        public static int maxThreads = 8;
        static int numThreads;

        private static Loom _current;
        private int _count;
        public static Loom Current
        {
            get
            {
                Initialize();
                return _current;
            }
        }
        public Loom()
        {

        }

        void Awake()
        {
            _current = this;
            initialized = true;
        }

        static bool initialized;

        static void Initialize()
        {
            if (!initialized)
            {

                if (!Application.isPlaying)
                    return;
                initialized = true;
                var g = new GameObject("Loom");
                DontDestroyOnLoad(g);
                _current = g.AddComponent<Loom>();
            }

        }

        private readonly List<Action> _actions = new List<Action>();
        public struct DelayedQueueItem
        {
            public float time;
            public Action action;
        }
        private readonly List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();
        readonly List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();

        public static void QueueOnMainThread(Action action)
        {
            QueueOnMainThread(action, 0f);
        }
        public static void QueueOnMainThread(Action action, float time)
        {
            Current.Logger.LogDebug($"QueueOnMainThread entered.");
            if (time != 0)
            {
                lock (Current._delayed)
                {
                    Current._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action });
                }
            }
            else
            {
                lock (Current._actions)
                {
                    Current._actions.Add(action);
                }
            }
        }

        public static Thread RunAsync(Action a)
        {
            Initialize();
            while (numThreads >= maxThreads)
            {
                Thread.Sleep(1);
            }
            Interlocked.Increment(ref numThreads);
            ThreadPool.QueueUserWorkItem(RunAction, a);
            return null;
        }

        private static void RunAction(object action)
        {
            try
            {
                ((Action)action)();
            }
            catch
            {
            }
            finally
            {
                Interlocked.Decrement(ref numThreads);
            }

        }


        void OnDisable()
        {
            if (_current == this)
            {

                _current = null;
            }
        }



        // Use this for initialization
        void Start()
        {

        }

        readonly List<Action> _currentActions = new List<Action>();

        // Update is called once per frame
        void Update()
        {
            lock (_actions)
            {
                _currentActions.Clear();
                _currentActions.AddRange(_actions);
                _actions.Clear();
            }
            foreach (var a in _currentActions)
            {
                a();
            }
            lock (_delayed)
            {
                _currentDelayed.Clear();
                _currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));
                foreach (var item in _currentDelayed)
                    _delayed.Remove(item);
            }
            foreach (var delayed in _currentDelayed)
            {
                delayed.action();
            }
        }
    }

}
