using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace GrpcNetProxyTest.Setup
{

    /// <summary>
    /// Test logger provider
    /// </summary>
    public class TestLoggerProvider : ILoggerProvider
    {

        /// <summary>
        /// Test log sink
        /// </summary>
        public class TestLogSink
        {
            public List<string> Logs { get; private set; } = new List<string>();
            public void AddEntry(string logEntry) => Logs.Add(logEntry);
        }

        /// <summary>
        /// Test logger
        /// </summary>
        public class TestLogger : ILogger
        {

            private readonly string _categoryName;
            private readonly Action<string> _addEntryAction;

            public TestLogger(string categoryName, Action<string> addEntryAction)
            {
                _categoryName = categoryName;
                _addEntryAction = addEntryAction;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var logEntry = $"{_categoryName}:{state.ToString()}";
                _addEntryAction.Invoke(logEntry);
            }
        }

        private readonly TestLogSink _sink;

        public TestLoggerProvider(TestLogSink sink)
        {
            _sink = sink;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestLogger(categoryName, (value) => _sink.AddEntry(value));
        }

        public void Dispose()
        {
        }

    }
}
