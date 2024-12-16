using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace xbridge.Modules
{
    public class Events
    {
        private XBridge bridge;

        public Events(XBridge bridge)
        {
            this.bridge = bridge;
        }

        

        public Task TriggerOnServer(string eventType, JObject data)
        {

            var task = new Task(() => TriggerOnServerAndAwait(eventType, data));
                task.Start();
            return task;
        }
        public void TriggerOnServerAndAwait(string eventType, JObject data)
        {
            //Console.WriteLine("Events.TriggerOnServerAndAwait: " + eventType);
            if (listeners.ContainsKey(eventType))
            {
                var list = listeners[eventType];
                foreach (var x in list)
                {
                    if (x.emptyAction != null)
                        x.emptyAction();
                    else
                        x.eventAction(new Event(eventType, data));
                }
            }
        }

        public void Trigger(string eventType, JObject data)
        {
            TriggerOnServer(eventType, data);
            bridge.ExecJSMethod("xbridge._tc", eventType, data);
        }

        public void TriggerOnSharedObject(IXBridgeSharedObject obj, string eventType, JObject data)
        {
            bridge.ExecJSMethod("xbridge._tso", obj.ID, eventType, data);
        }


        private class ActionData
        {
            public readonly Action<Event> eventAction;
            public readonly Action emptyAction;

            public ActionData(Action<Event> action)
            {
                this.eventAction = action;
            }


            public ActionData(Action action)
            {
                this.emptyAction = action;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ActionData))
                    return false;
                var data = (ActionData)obj;
                return data.eventAction == eventAction && data.emptyAction == emptyAction;
            }

            public override int GetHashCode()
            {
                if (emptyAction != null)
                    return emptyAction.GetHashCode();
                else
                    return eventAction.GetHashCode();
            }
        }

        Dictionary<string, List<ActionData>> listeners = new Dictionary<string, List<ActionData>>();


        public Events On(string eventType, Action<Event> action)
        {
            return on(eventType, new ActionData(action));

        }
        public Events On(string eventType, Action action)
        {
            return on(eventType, new ActionData(action));
        }

        private Events on(string eventType, ActionData actionData)
        {
            lock (listeners)
            {
                List<ActionData> list;
                if (listeners.ContainsKey(eventType))
                    list = this.listeners[eventType];
                else
                    list = this.listeners[eventType] = new List<ActionData>();
                list.Add(actionData);
            }
            return this;
        }


        Dictionary<int, TaskCompletionSource<bool>> Running = new Dictionary<int, TaskCompletionSource<bool>>();

        internal async Task TriggerAwaitable(string eventType, JObject data)
        {
            var task = this.TriggerOnServer(eventType, data);
            var awaitable = index.Create();
            bridge.ExecJSMethod("xbridge._tcw", awaitable.ID, eventType, data);
            await task;
            await awaitable.Task;
        }

        public void TriggerAndAwait(string eventType, JObject data)
        {
            var task = TriggerAwaitable(eventType, data);
            task.Wait();
        }
        private IndexedTasks<bool> index = new IndexedTasks<bool>();

        public void FinishOnServer(Int64 id)
        {
            index.Finish((int)id, true);
        }

        public bool Off(string eventType, Action<Event> action)
        {
            return off(eventType, new ActionData(action));

        }
        public bool Off(string eventType, Action action)
        {
            return off(eventType, new ActionData(action));

        }

        private bool off(string eventType, ActionData actionData)
        {

             lock (listeners)
            {
                if (!listeners.ContainsKey(eventType))
                    return false;
                return listeners[eventType].Remove(actionData);
            }
        }

        public bool Off(string eventType)
        {

            lock (listeners)
            {
                return listeners.Remove(eventType);
            }
        }
        public class Event
        {
            private string eventType;
            private JObject _data;
            private JObject data
            {
                get
                {
                    if (_data == null)
                        _data = new JObject();
                    return _data;
                }
                set => _data = value;
            }

            public Event(string eventType, JObject data)
            {
                this.eventType = eventType;
                this.data = data;
            }
        }
    }
}
