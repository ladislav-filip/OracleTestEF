using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DAL.Tests
{
    internal class LoggerDb : ILoggerDb
    {
        public List<StringBuilder> Commands { get; } = new List<StringBuilder>();

        public LoggerDb()
        {
            StartNewRecord();
        }

        public void WriteLine(string text)
        {
            Debug.WriteLine(text);
            Current.Append(text);
        }

        public void StartNewRecord()
        {
            Commands.Add(new StringBuilder());
        }

        public StringBuilder Current => Commands.Last();
    }
}