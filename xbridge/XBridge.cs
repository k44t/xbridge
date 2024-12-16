using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Net;
using System.Linq;
using xbridge.Modules;
using NLog;
using System.ComponentModel;

namespace xbridge
{
    public class XBridge
    {
        private Logger log;

        public XBridge(IXBridgeAdapter adapter)
        {
            SetAdapter(adapter);
            log = LogManager.GetLogger("xbridge");
        }

        public XBridge SetAdapter(IXBridgeAdapter adapter)
        {
            this.Adapter = adapter;
            adapter.Bridge = this;
            return this;
        }

        internal Type GetSharedObjectType(int id)
        {
            return sharedTypesReverse[id];

        }

        internal void DestroyAllSharedObjects()
        {
            var array = sharedObjects.Keys.ToArray<int>();
            foreach (var key in array)
            {
                try
                {
                    DestroySharedObject(sharedObjects[key]);
                }catch(Exception err)
                {
                    Console.WriteLine(err.StackTrace);
                }
            };
        }

        private Dictionary<object, object> modules = new Dictionary<object, object>();

        public IXBridgeAdapter Adapter;

        public string ToJS(object o)
        {
            return JsonConvert.SerializeObject(o);
        }


        internal void report(string callback, Exception e)
        {
            report(callback, e.ToString());
        }

        internal void report(string callback, String e)
        {
            if (callback == null)
                callback = "null";
            var cmd = "xbridge.error(" + callback + ", " + ToJS(e) + ")"; 
            ExecJS(cmd);
        }


        public object FromJS(string s)
        {
            return JsonConvert.DeserializeObject(s);
        }

        public T GetModule<T>()
        {

            return (T)modules[typeof(T)];
        }
        public object GetModule(Type type)
        {
            return modules[type];
        }

        internal void Log(string v)
        {
            Console.WriteLine(v);
            ExecJS("console.log(" + ToJS(v) + ");");
        }

        public object GetModule(string type)
        {
            return modules[type];
        }


        public bool HandleURL(string url)
        {
            url = WebUtility.UrlDecode(url);
            return HandleDecodedURL(url);
        }
        public bool HandleDecodedURL(string url)
        {

            if (url.StartsWith("xbridge:", StringComparison.Ordinal))
            {
                doHandle(url);
                return true;
            }
            return false;

        }

        private void doHandle(string url)
        {
            try
            {
                var call = url.Substring(8);
                var callback = "null";
                for (int i = 0; i < call.Length; i++)
                {
                    var c = call[i];
                    if (!Char.IsDigit(c))
                    {
                        if (i > 0)
                        {
                            if (c != ':')
                            {
                                report("null", url, "xbridge url must be of form xbridge:[1:][module.]method([arguments])");
                                return;
                            }
                            callback = call.Substring(0, i);
                            call = call.Substring(i + 1);
                            break;
                        }
                    }


                }
                try
                {
                    var argsIndex = call.IndexOf('(');
                    if (argsIndex < 0)
                    {

                        report(callback, url, "xbridge url must be of form xbridge:[1:][module.]method([arguments])");
                        return;
                    }
                    var moduleAndMethod = call.Substring(0, argsIndex);


                    var parts = moduleAndMethod.Split('.');
                    String moduleName;
                    String methodName;
                    if (parts.Length == 1)
                    {
                        moduleName = "core";
                        methodName = parts[0];
                    }
                    else if (parts.Length == 2)
                    {
                        moduleName = parts[0];
                        methodName = parts[1];
                    }
                    else
                    {
                        report(callback, url, "xbridge url must be of form xbridge:[module.]method([arguments])");
                        return;
                    }
                    var argString = call.Substring(argsIndex + 1, call.Length - argsIndex - 2);
                    //ExecJS("console.log('xbridge-server: executing: ' + " + ToJS(call) + ")");


                    object obj;
                    int n;
                    bool isShared = false;
                    if (isShared = int.TryParse(moduleName, out n))
                    {
                        obj = GetSharedObject(n);
                        if (methodName == "destroy")
                        {
                            DestroySharedObject((xbridge.IXBridgeSharedObject)obj);
                            Return(callback);
                            return;
                        }
                    }
                    else
                    {

                        if (!modules.ContainsKey(moduleName))
                        {
                            report(callback, url, "module not available: " + ToJS(moduleName));
                            return;
                        }
                        obj = modules[moduleName];
                    }
                    var csharpmethodname = methodName.Substring(0, 1).ToUpper() + methodName.Substring(1);
                    if (csharpmethodname.StartsWith("_", StringComparison.Ordinal))
                    {
                        report(callback, url, "methods starting with an '_' (underscore) are not exported: `" + ToJS(methodName) + "`");
                        return;
                    }

                    object[] args;
                    try
                    {
                        args = JsonConvert.DeserializeObject<object[]>("[" + argString + "]");

                    }
                    catch (Exception e)
                    {
                        report(callback, url, e);
                        return;
                    }
                    var result = ExecOnObject(obj, csharpmethodname, args);
                    if (result.Item1)
                    {
                        if (result.Item2 is Task<object>)
                            run(callback, (Task<object>)result.Item2);
                        else
                            Return(callback, result.Item2);
                    }
                    else
                    {
                        Return(callback);
                    }
                }
                catch (Exception e)
                {
                    //dont delete me, I send the  error back to the rright caller!!!
                    Console.WriteLine(e.StackTrace.ToString());
                    report(callback, url, e);
                }


            }
            catch (Exception e)
            {
                log.Error(e, "exception on xbridge call");
                    report(null, url, e);
            }
        }


        public (bool, object) ExecOnObject(object o, string csharpmethodname, object[] args)
        {
            MethodInfo method = null;
            method = o.GetType().GetMethod(csharpmethodname);
            if (method == null)
            {
                throw new Exception("'object of type ' + " + ToJS(o.GetType().FullName) + " + ' has no (unique and public) method (on c# must start with an uppercase letter): ' + " + ToJS(csharpmethodname) + ")");
            }
            var parameters = method.GetParameters();
            if (parameters.Length > args.Length)
            {
                var newargs = new object[parameters.Length];
                for (var i = args.Length - 1; i >= 0; --i)
                {
                    newargs[i] = args[i];
                }
                args = newargs;
            }

            for (var i = parameters.Length - 1; i >= 0; --i)
            {
                var p = parameters[i];
                if (i < args.Length)
                {
                    var arg = args[i];
                    var t = p.ParameterType;

                    if (t.IsArray || t == typeof(object))
                    {
                        if (arg is JArray)
                        {
                            var elementType = typeof(object);
                            if (t.IsArray)
                                elementType = t.GetElementType();
                            args[i] = toArray((JArray)arg, elementType);
                        }
                    }
                    else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {

                        if (arg is JObject)
                        {

                            Type keyType = t.GetGenericArguments()[0];
                            if (keyType != typeof(string))
                                throw new Exception("only string keys are supported for dictionaries");
                            Type valueType = t.GetGenericArguments()[1];
                            Dictionary<String, dynamic> instance = (Dictionary<String, dynamic>)Activator.CreateInstance(t);
                            var jObj = (JObject)arg;
                            foreach (var e in jObj)
                            {
                                instance.Add(e.Key, e.Value);
                            }
                            args[i] = instance;
                        }
                    }
                    else if (typeof(IXBridgeSharedObject).IsAssignableFrom(t))
                    {
                        if (arg is Int64)
                        {
                            try
                            {
                                args[i] = GetSharedObject((Int64)arg);
                            }
                            catch (KeyNotFoundException)
                            {
                                throw new Exception("shared object with id `" + arg + "` not found");
                            }
                        }
                        else if (arg != null)
                        {
                            throw new Exception("not a shared object: " + arg);
                        }
                    }else if(Nullable.GetUnderlyingType(t) != null)
                    {
                        if(arg != null)
                        {
                            var nt = Nullable.GetUnderlyingType(t);
                            args[i] = Convert.ChangeType(arg, nt);
                        }
                    }

                }
            }
            return (method.ReturnType != typeof(void), method.Invoke(o, args));
        }
        private void Return(string callback)
        {
            ExecJS("xbridge._cc(" + callback + ")");
        }

        private void Return(string callback, object result)
        {
            if (result is IXBridgeSharedObject)
            {
                ReturnSharedObject(callback, (xbridge.IXBridgeSharedObject)result);
            }
            else
            {
                ExecJS("xbridge._cc(" + callback + ", " + ToJS(result) + ")");
            }
        }

        private void ReturnSharedObject(string callback, IXBridgeSharedObject o)
        {
            var ids = ShareObject(o);
            ExecJS("xbridge._ccso(" + callback + ", " + ids.Item1 + ", " + ids.Item2 + ")");
        }

        private object toArray(JArray arg, Type type)
        {
            JArray jArr = (Newtonsoft.Json.Linq.JArray)arg;
            Array newArr = Array.CreateInstance(type, jArr.Count);
            for (var x = jArr.Count - 1; x >= 0; --x)
            {
                object val;
                if (type.IsArray)
                    val = toArray((JArray)jArr[x], type.GetElementType());
                else if (type == typeof(object) && jArr[x] is JArray)
                    val = toArray((JArray)jArr[x], typeof(object));
                else
                    val = jArr[x].ToObject<object>();
                newArr.SetValue(val, x);
            }
            return newArr;
        }

        public void report(string callback, string url, string message)
        {
            report(callback, "error when executing url `" + url + "`: " + message);
        }


        public void report(string callback, string url, Exception e)
        {
            report(callback, url, "error when executing `" + url + "`: " + e.ToString());
        }

        public XBridge Export(String name, object controller)
        {
            modules.Add(name, controller);
            if (!modules.ContainsKey(controller.GetType()))
                modules.Add(controller.GetType(), controller);
            return this;
        }

        public XBridge Load(Type type)
        {

            ConstructorInfo ctor = type.GetConstructor(new[] { typeof(XBridge) });
            object instance = ctor.Invoke(new object[] { this });
            return Load(instance);
        }

        private XBridge Load(object instance)
        {
            modules[instance.GetType()] = instance;
            var t = instance.GetType();
            while (t != null && t != typeof(Object))
            {
                modules[t] = instance;
                foreach (var i in t.GetInterfaces())
                {
                    modules[i] = instance;
                }
                t = t.BaseType;
            }
            return this;
        }

        public XBridge Export(object controller)
        {
            var name = controller.GetType().Name;
            name = name.Substring(0, 1).ToLower() + name.Substring(1);
            return Export(name, controller);
        }

        public XBridge Export(Type type)
        {
            Load(type);
            return Export(GetModule(type));
        }
        public XBridge Export(string name, Type type)
        {
            Load(type);
            return Export(name, GetModule(type));
        }

        public void ExecJS(string command)
        {
            try
            {
                Adapter.ExecJS(command);
            }
            catch (Exception ex)
            {
                log.Error(ex, "error on executing javascript call: " + command);
            }
        }
        public void ExecJSMethod(string method, params object[] arguments)
        {
            var args = JsonConvert.SerializeObject(arguments);
            args = args.Substring(1, args.Length - 2);
            var command = method + "(" + args + ")";
            ExecJS(command);
        }


        private async void run(string id, Task<object> task)
        {
            try
            {
                object result = await task;
                Return(id, result);
            }
            catch (Exception e)
            {
                report(id, e);
            }
        }

        public void SetLocation(string url)
        {
            Adapter.SetLocation(url);
        }



        Dictionary<int, IXBridgeSharedObject> sharedObjects = new Dictionary<int, IXBridgeSharedObject>();
        Dictionary<Type, int> sharedTypes = new Dictionary<Type, int>();
        Dictionary<int, Type> sharedTypesReverse = new Dictionary<int, Type>();
        HashSet<int> sharedTypeIds = new HashSet<int>();

        Random r = new Random();

        private (int, int) ShareObject(IXBridgeSharedObject o)
        {
            lock (o)
            {
                if (o.ID == 0)
                {
                    int typeID = _PossiblyRegisterType(o.GetType());
                    int id = r.Next();
                    lock (sharedObjects)
                    {
                        while (sharedObjects.ContainsKey(id))
                            id = r.Next();
                        sharedObjects[id] = o;
                    }
                    o.ID = id;
                    o.TypeID = typeID;
                    return (typeID, id);
                }
                return ((int)o.TypeID, (int)o.ID);
            }
        }



        public object GetSharedObject(Int64 id)
        {
            lock (sharedObjects)
            {
                return sharedObjects[(int)id];
            }
        }


        public void DestroySharedObject(IXBridgeSharedObject o)
        {
            lock (sharedObjects)
            {
                sharedObjects.Remove((int)o.ID);
            }
            o.ID = 0;
            o.Destroy();
        }

        private int _PossiblyRegisterType(Type type)
        {
            int id;
            lock (type)
            {
                bool register = false;
                lock (sharedTypes)
                {
                    try
                    {
                        id = sharedTypes[type];
                    }
                    catch (KeyNotFoundException)
                    {
                        register = true;
                        id = r.Next();
                        while (id == 0 || sharedTypeIds.Contains(id))
                            id = r.Next();
                        sharedTypeIds.Add(id);
                        sharedTypes[type] = id;
                        sharedTypesReverse[id] = type;
                    }
                }
                if (register)
                {
                    _RegisterType(id, type);
                }
            }
            return id;
        }

        private void _RegisterType(int id, Type type)
        {
            var names = GetSharedMethods(type);
            ExecJS("xbridge._rst(" + id + ",'" + type.FullName + "'," + ToJS(names) + ")");
        }

        internal List<string> GetSharedMethods(Type type)
        {
            var methods = type.GetMethods();

            List<string> names = new List<string>();

            foreach (var method in methods)
            {
                if (method.IsPublic && method.DeclaringType == type && Char.IsUpper(method.Name[0]))
                {
                    names.Add(Char.ToLower(method.Name[0]) + method.Name.Substring(1));
                }
            }
            return names;
        }
    }
}
