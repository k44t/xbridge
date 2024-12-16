using System;
using System.IO;
using System.Threading.Tasks;
using NLog;

namespace xbridge.Modules
{
    public class Log
    {
        Logger log;
        public void Init()
        {
           
        }

        public NamedLog Create(String name)
        {
            var plog = NLog.LogManager.GetLogger(name);
            return new NamedLog(bridge, plog);
        }

        protected XBridge bridge;
        public Log(XBridge bridge)
        {
            this.bridge = bridge;
            log = NLog.LogManager.GetLogger("xbridge");
        }

        public async Task<NLog.Targets.FileTarget> _CreateExternalStorageLogTarget(String absoluteLogFilePath) {
            var result = await bridge.GetModule<Core>().GetPermission("write-file");
            if (result)
                return new NLog.Targets.FileTarget("ft") { FileName = absoluteLogFilePath };
            else
                throw new Exception("external storage permission not provided by user or system");
        }

        public string _AutoConfigure()
        {

            //var dir = Android.OS.Environment.ExternalStorageDirectory.ToString();
            var dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            dir = Path.Combine(dir, "xbridge.log");
            DoAutoConfigure(dir);
            return dir;
        }

        public async Task<object> _AutoConfigure(String externalFile)
        {
            var result = await bridge.GetModule<Core>().GetPermission("write-file");
            if (result)
            {
                DoAutoConfigure(externalFile);
            }
            else
            {
                var dir = _AutoConfigure();
                log.Error("no permission given by user or system to write log to: " + externalFile + ", defaulting to " + dir);
            }
            throw new Exception("external storage permission not provided by user or system");
        }

        private void DoAutoConfigure(String file)
        {
            var config = new NLog.Config.LoggingConfiguration();

            var ft = new NLog.Targets.FileTarget("ft") { FileName = file, AutoFlush = true, ForceManaged = true, FileNameKind = NLog.Targets.FilePathKind.Absolute };
            var ct = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddTarget("f", ft);
            config.AddRuleForAllLevels(ft);
            config.AddRuleForAllLevels(ct);
            //config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);

            NLog.LogManager.Configuration = config;
            NLog.LogManager.ReconfigExistingLoggers();
        }

        public void Info(String msg)
        {
            log.Info(msg);
        }

        public void Warn(String msg)
        {
            log.Warn(msg);
        }

        public void Error(String msg)
        {
            log.Error(msg);
        }

        public void Fatal(String msg)
        {
            log.Fatal(msg);
        }

        public void Debug(String msg)
        {
            log.Debug(msg);
        }

        public void Trace(String msg)
        {
            log.Trace(msg);
        }

    }

    public class NamedLog: XBridgeSharedObject
    {
        private Logger log;

        public NamedLog(XBridge xbridge, Logger log) : base(xbridge)
        {
            this.log = log;
        }

        public void Info(String msg)
        {
            log.Info(msg);
        }

        public void Warn(String msg)
        {
            log.Warn(msg);
        }

        public void Error(String msg)
        {
            log.Error(msg);
        }

        public void Fatal(String msg)
        {
            log.Fatal(msg);
        }

        public void Debug(String msg)
        {
            log.Debug(msg);
        }

        public void Trace(String msg)
        {
            log.Trace(msg);
        }

    }
}
