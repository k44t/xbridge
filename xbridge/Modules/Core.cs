using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace xbridge.Modules
{
    public abstract class Core
    {
        private XBridge bridge;

        public Core(XBridge bridge)
        {
            this.bridge = bridge;
        }

        public void Alert(string s){
            bridge.ExecJS("alert(" + bridge.ToJS(s) + ")");
        }

        public void Log(string s)
        {
            Console.WriteLine(s);
        }

        public JObject GetSharedType(Int64 id)
        {
            var type = bridge.GetSharedObjectType((int)id);
            var methods = bridge.GetSharedMethods(type);
            return new JObject()
            {
                new JProperty("id", id),
                new JProperty("name", type.FullName),
                new JProperty("methods", methods)
            };
        }

        public virtual void Terminate()
        {
            System.Environment.Exit(0);
        }

        public void TriggerGC() {

            GC.Collect();
        }


        public bool HasMethod(string module, string method)
        {
            try
            {
                var o = bridge.GetModule(module);
                var met = o.GetType().GetMethod(method.Substring(0, 1).ToUpper() + method.Substring(1), System.Reflection.BindingFlags.Public);
                return met != null;
            }
            catch (Exception)
            {
                return false;
            }

        }

        private object GetModuleOrObject(string moduleOrSharedObject)
        {
            int n;
            bool isShared = false;
            if (isShared = int.TryParse(moduleOrSharedObject, out n))
            {
                return bridge.GetSharedObject(n);
            }
            else
                try
                {
                    return bridge.GetModule(moduleOrSharedObject);
                }
                catch (KeyNotFoundException)
                {
                    return null;
                }
        }

        public bool HasModule(string module)
        {
            try
            {
                bridge.GetModule(module);
                return true;
            }catch(KeyNotFoundException)
            {
                return false;
            }
        }


        public abstract string Os();
        public abstract string AppID();




        public abstract Task<bool> GetPermission(string v);
        public abstract Stream OpenPackagedFile(string name);
        public abstract void Stop();
    }
}
