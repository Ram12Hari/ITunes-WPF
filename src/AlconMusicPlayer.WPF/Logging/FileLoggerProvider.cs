using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlconMusicPlayer.WPF.Logging
{
    public sealed class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;
        private readonly LogLevel _minimumLevel;

        public FileLoggerProvider(string filePath, LogLevel minimumLevel = LogLevel.Information)
        {
            _filePath = filePath;
            _minimumLevel = minimumLevel;

            // Ensure the log directory exists before any write attempt
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
        }

        public ILogger CreateLogger(string categoryName) =>
            new FileLogger(categoryName, _filePath, _minimumLevel);

        public void Dispose() { }
    }

    internal sealed class FileLogger : ILogger
    {
        private readonly string _category;
        private readonly string _filePath;
        private readonly LogLevel _minimumLevel;

        // Static lock shared across all FileLogger instances — all target the same file
        //private static readonly Lock _lock = new();

        internal FileLogger(string category, string filePath, LogLevel minimumLevel)
        {
            _category = category;
            _filePath = filePath;
            _minimumLevel = minimumLevel;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var shortCategory = _category.Contains('.')
                ? _category[((_category.LastIndexOf('.') + 1))..]
                : _category;

            var message = formatter(state, exception);
            var line = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} [{LevelLabel(logLevel)}] {shortCategory}: {message}";
            if (exception is not null)
                line += Environment.NewLine + exception;
            File.AppendAllText(_filePath, line + Environment.NewLine);
            //lock (_lock)
            //    File.AppendAllText(_filePath, line + Environment.NewLine);
        }

        private static string LevelLabel(LogLevel level) => level switch
        {
            LogLevel.Trace => "TRC",
            LogLevel.Debug => "DBG",
            LogLevel.Information => "INF",
            LogLevel.Warning => "WRN",
            LogLevel.Error => "ERR",
            LogLevel.Critical => "CRT",
            _ => "???"
        };
    }
}
