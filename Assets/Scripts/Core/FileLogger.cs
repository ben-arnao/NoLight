using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace RogueLike2D.Core
{
    // Mirrors all Unity Console logs to a file and starts a fresh log each app start.
    // Also writes a session header with useful environment details.
    public static class FileLogger
    {
        private static readonly object _lock = new object();
        private static StreamWriter _writer;
        private static string _logDir;
        private static string _logFilePath;
        private static bool _initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            Initialize();
        }

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                // In Editor, write logs to <project_root>/Logs for convenience.
                // In builds, write logs under persistentDataPath/Logs.
                string baseDir = Application.isEditor
                    ? Path.GetFullPath(Path.Combine(Application.dataPath, ".."))
                    : Application.persistentDataPath;

                _logDir = Path.Combine(baseDir, "Logs");

                // Fresh logs each session: clear existing .log files.
                if (Directory.Exists(_logDir))
                {
                    foreach (var f in Directory.GetFiles(_logDir, "*.log"))
                    {
                        try { File.Delete(f); } catch { /* ignore */ }
                    }
                }
                else
                {
                    Directory.CreateDirectory(_logDir);
                }

                _logFilePath = Path.Combine(_logDir, "debug.log");
                _writer = new StreamWriter(_logFilePath, true, Encoding.UTF8) { AutoFlush = true };

                Application.logMessageReceivedThreaded -= OnLogMessageReceived;
                Application.logMessageReceivedThreaded += OnLogMessageReceived;
                Application.quitting -= OnQuitting;
                Application.quitting += OnQuitting;

                WriteSessionHeader();
                Debug.Log($"[FileLogger] Initialized. Writing to {_logFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FileLogger] Failed to initialize: {ex}");
            }
        }

        private static void WriteSessionHeader()
        {
            lock (_lock)
            {
                if (_writer == null) return;
                _writer.WriteLine("=======================================================");
                _writer.WriteLine($"Session start: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                _writer.WriteLine($"Product: {Application.companyName} / {Application.productName} v{Application.version}");
                _writer.WriteLine($"Unity: {Application.unityVersion} Platform: {Application.platform}");
                _writer.WriteLine($"persistentDataPath: {Application.persistentDataPath}");
                _writer.WriteLine($"dataPath: {Application.dataPath}");
                _writer.WriteLine($"Device: {SystemInfo.deviceModel} | OS: {SystemInfo.operatingSystem}");
                _writer.WriteLine("=======================================================");
            }
        }

        private static void OnQuitting()
        {
            Log("[FileLogger] Application.quitting");
            Dispose();
        }

        private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            try
            {
                string level = type.ToString();
                int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                string time = DateTime.Now.ToString("HH:mm:ss.fff");

                lock (_lock)
                {
                    if (_writer == null) return;
                    _writer.WriteLine($"[{time}][{level}][T{threadId}|F{Time.frameCount}] {condition}");
                    if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
                    {
                        if (!string.IsNullOrEmpty(stackTrace))
                        {
                            using (var sr = new StringReader(stackTrace))
                            {
                                string line;
                                while ((line = sr.ReadLine()) != null)
                                {
                                    _writer.WriteLine($"    {line}");
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Swallow any logging exceptions to avoid cascading failures.
            }
        }

        // Convenience method if you want to explicitly log via this class.
        public static void Log(string message)
        {
            Debug.Log(message); // mirrored to file via OnLogMessageReceived
        }

        public static void Dispose()
        {
            lock (_lock)
            {
                try
                {
                    Application.logMessageReceivedThreaded -= OnLogMessageReceived;
                    Application.quitting -= OnQuitting;
                    _writer?.Flush();
                    _writer?.Dispose();
                    _writer = null;
                }
                catch { }
            }
        }

        public static string GetLogDirectory() => _logDir;
        public static string GetLogFilePath() => _logFilePath;
    }
}
