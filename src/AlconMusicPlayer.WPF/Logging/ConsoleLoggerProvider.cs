using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AlconMusicPlayer.WPF.Logging
{
    public sealed class ConsoleLoggerProvider : ILoggerProvider
    {
        private readonly LogLevel _minimumLevel;

        public ConsoleLoggerProvider(LogLevel minimumLevel = LogLevel.Information)
        {
            _minimumLevel = minimumLevel;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new ConsoleLogger(categoryName, _minimumLevel);
        }

        public void Dispose()
        {
        }
    }

    internal sealed class ConsoleLogger : ILogger
    {
        private readonly LogLevel _minimumLevel;
        private readonly string _category;

        internal ConsoleLogger(string category, LogLevel minimum)
        {
            _category = category;
            _minimumLevel = minimum;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _minimumLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            var message = formatter(state, exception);
            var line = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} [{LevelLabel(logLevel)}] {_category}: {message}";
            Console.ForegroundColor = LevelColor(logLevel);
            Console.WriteLine(line);
            if (exception is not null)
                Console.WriteLine(exception);
            Console.ResetColor();
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

        private static ConsoleColor LevelColor(LogLevel level) => level switch
        {
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Critical => ConsoleColor.DarkRed,
            _ => ConsoleColor.Gray
        };
    }
}
