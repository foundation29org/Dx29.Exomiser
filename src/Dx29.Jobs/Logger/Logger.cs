using System;
using System.IO;

namespace Dx29
{
    public enum LogMode
    {
        Quiet = 0,
        Admin = 1,
        Error = 2,
        Warning = 3,
        Info = 4
    }

    public class Logger
    {
        private bool _firstTime = true;

        public Logger(LogMode mode, bool indented = true) : this(Console.Out, mode, indented)
        {
        }
        public Logger(TextWriter writer, LogMode mode, bool indented = true)
        {
            Mode = mode;
            Indented = indented;
            Writer = writer;
        }

        public LogMode Mode { get; set; }
        public bool Indented { get; set; }

        public TextWriter Writer { get; }

        public void Admin(string message, object details = null)
        {
            Log(LogMode.Admin, message, details);
        }

        public void Info(string message, object details = null)
        {
            Log(LogMode.Info, message, details);
        }

        public void Warning(Exception ex)
        {
            Log(LogMode.Warning, $"{ex.GetType()}: {ex.Message}", ex.StackTrace);
        }
        public void Warning(string message, object details = null)
        {
            Log(LogMode.Warning, message, details);
        }

        public void Error(Exception ex)
        {
            Log(LogMode.Error, $"{ex.GetType()}: {ex.Message}", ex.StackTrace);
        }
        public void Error(string message, object details = null)
        {
            Log(LogMode.Error, message, details);
        }

        virtual public void Log(LogMode mode, string message, object details = null)
        {
            try
            {
                if (mode <= Mode)
                {
                    DateTime date = DateTime.UtcNow;
                    Writer.WriteLine("{0}\t{1}\t{2}", date.ToString("yy/MM/dd HH:mm:ss.ffff"), mode, message);
                    Console.WriteLine("{0}\t{1}\t{2}", date.ToString("yy/MM/dd HH:mm:ss.ffff"), mode, message);
                    Writer.WriteLine(details?.Serialize(Indented));
                    Console.WriteLine(details?.Serialize(Indented));
                    Writer.WriteLine();
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void WriteSeparator()
        {
            if (_firstTime)
                _firstTime = false;
            else
            {
                Writer.WriteLine(",");
                Console.WriteLine(",");
            }
        }
    }
}
