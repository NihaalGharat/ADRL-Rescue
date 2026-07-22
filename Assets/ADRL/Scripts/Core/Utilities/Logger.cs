namespace ADRL.Core.Utilities
{
    using System;
    using System.Text;
    using UnityEngine;

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public static class Logger
    {
        private static LogLevel _currentLevel = LogLevel.Info;
        private static readonly StringBuilder _stringBuilder = new StringBuilder(256);

        public static LogLevel CurrentLevel
        {
            get => _currentLevel;
            set => _currentLevel = value;
        }

        public static void Debug(string message, string category = null)
        {
            if (_currentLevel > LogLevel.Debug)
                return;

            LogInternal("DEBUG", message, category, LogType.Log);
        }

        public static void Info(string message, string category = null)
        {
            if (_currentLevel > LogLevel.Info)
                return;

            LogInternal("INFO", message, category, LogType.Log);
        }

        public static void Warning(string message, string category = null)
        {
            if (_currentLevel > LogLevel.Warning)
                return;

            LogInternal("WARN", message, category, LogType.Warning);
        }

        public static void Error(string message, string category = null)
        {
            LogInternal("ERROR", message, category, LogType.Error);
        }

        public static void Exception(Exception exception, string category = null)
        {
            _stringBuilder.Clear();
            _stringBuilder.Append("[EXCEPTION]");

            if (!string.IsNullOrEmpty(category))
            {
                _stringBuilder.Append("[");
                _stringBuilder.Append(category);
                _stringBuilder.Append("]");
            }

            _stringBuilder.Append(" ");
            _stringBuilder.Append(exception.Message);
            _stringBuilder.Append("\n");
            _stringBuilder.Append(exception.StackTrace);

            UnityEngine.Debug.LogError(_stringBuilder.ToString());
        }

        private static void LogInternal(string level, string message, string category, LogType logType)
        {
            _stringBuilder.Clear();
            _stringBuilder.Append("[");
            _stringBuilder.Append(level);
            _stringBuilder.Append("]");

            if (!string.IsNullOrEmpty(category))
            {
                _stringBuilder.Append("[");
                _stringBuilder.Append(category);
                _stringBuilder.Append("]");
            }

            _stringBuilder.Append(" ");
            _stringBuilder.Append(message);

            var formattedMessage = _stringBuilder.ToString();

            switch (logType)
            {
                case LogType.Warning:
                    UnityEngine.Debug.LogWarning(formattedMessage);
                    break;
                case LogType.Error:
                    UnityEngine.Debug.LogError(formattedMessage);
                    break;
                default:
                    UnityEngine.Debug.Log(formattedMessage);
                    break;
            }
        }
    }
}
