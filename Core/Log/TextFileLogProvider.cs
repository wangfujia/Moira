using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Reflection;
using System.Threading.Tasks;

namespace BAFactory.Moira.Core.Log
{
    internal class TextFileLogProvider : XmlConfigurationParser, ILogProvider
    {
        public enum RotateOptions
        {
            OnSizeInBytes,
            OnTimeSpanInMinutes
        }

        public class TextFileLogConfiguration
        {
            public string FilePath { get; set; }
            public RotateOptions RotateOption { get; set; }
            public ulong RotateLimit { get; set; }
            public ulong MaxFiles { get; set; }
            public LogLevel Level { get; set; }
        }

        public FileInfo LogFile { get; set; }

        private TextFileLogConfiguration Config { get; set; }

        public TextFileLogProvider()
            : base("http://BAFactory.net/schemas/TextLogProvider", "tlc")
        {
            InitializeLogProvider();
        }

        #region ILogProvider
        public void InitializeLogProvider()
        {
            if (!LoadConfigFile("TextLogProvider.xml"))
            {
                return;
            }
            ReadLogFileConfiguration();

            ProvideFile(Config.FilePath); // create or open

            if (!FileIsReady())
            {
                throw new IOException("Can't access log file");
            }
        }

        public void LogMessage(LogLevel l, string message)
        {
            if (l <= Config.Level)
            {
                LogMessageAsync(l, message).Wait();
            }
        }

        public void LogException(Exception e)
        {
            LogMessageAsync(LogLevel.Error, "--> EXECPTION <---").Wait();
            if (e != null)
            {
                LogInnerException(e);
            }
            LogMessageAsync(LogLevel.Error, string.Concat("Exception Message: ", e.Message)).Wait();
            LogMessageAsync(LogLevel.Error, string.Concat("Exception Stack Trace: ", e.StackTrace)).Wait();
            LogMessageAsync(LogLevel.Error, string.Concat("Exception Source: ", e.Source)).Wait();
            LogMessageAsync(LogLevel.Error, string.Concat("Exception Data: ", e.Data)).Wait();
        }

        public async Task LogMessageAsync(LogLevel l, string message)
        {
            ProvideFile(Config.FilePath);

            StringBuilder strBldr = new StringBuilder();
            strBldr.AppendFormat("{0:yyyy/MM/dd hh:mm:ss ffff} - {1}", DateTime.Now, message);

            try
            {
                using (StreamWriter sw = LogFile.AppendText())
                {
                    await sw.WriteLineAsync(strBldr.ToString());
                }
            }
            catch
            { }
        }
        #endregion

        private void LogInnerException(Exception e)
        {
            if (e.InnerException != null)
            {
                LogException(e.InnerException);
            }
        }

        private bool FileIsReady()
        {
            return LogFile.Exists;
        }

        private void ProvideFile(string filePath)
        {
            RotateFile(filePath);

            LogFile = new FileInfo(filePath);

            if (!LogFile.Exists)
            {
                string dirName = LogFile.DirectoryName;
                string fileName = LogFile.Name;

                DirectoryInfo directory = Directory.CreateDirectory(dirName);

                if (directory.Exists)
                {
                    using (FileStream fs = LogFile.Create())
                    {
                        fs.Close();
                    }
                }

                LogFile = new FileInfo(LogFile.FullName);
            }

            CleanOldFiles();
        }

        private void RotateFile(string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            if (file.Exists)
            {
                if ((Config.RotateOption == RotateOptions.OnSizeInBytes && (ulong)file.Length >= Config.RotateLimit)
                    || (Config.RotateOption == RotateOptions.OnTimeSpanInMinutes && file.CreationTime.AddMinutes(Config.RotateLimit) <= DateTime.Now))
                {
                    file.MoveTo(Path.Combine(file.DirectoryName, string.Format("{0:yyyyMMddhhMMssffff}_{1}", DateTime.Now, file.Name)));
                }
            }
        }

        private void CleanOldFiles()
        {
            string[] files = Directory.GetFiles(LogFile.DirectoryName, string.Format("*_{0}", LogFile.Name));
            if ((ulong)files.Length > Config.MaxFiles)
            {
                List<FileInfo> filesList = new List<FileInfo>();
                foreach (string fileName in files)
                {
                    FileInfo file = new FileInfo(fileName);
                    filesList.Add(file);
                }

                IEnumerable<FileInfo> filesToDelete = (from f in filesList
                                                       orderby f.LastAccessTime descending
                                                       select f).Skip((int)Config.MaxFiles);
                foreach (FileInfo f in filesToDelete)
                {
                    f.Delete();
                }
            }
        }

        #region IDisposable
        private bool disposed;

        private void Dispose(bool disposing)
        {
            CleanOldFiles();

            if (!this.disposed)
            {
                if (disposing)
                {
                    //if (LogFileStream != null)
                    //{
                    //    LogFileStream.Dispose();
                    //}
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TextFileLogProvider()
        {
            Dispose(false);
        }
        #endregion

        private bool ReadLogFileConfiguration()
        {
            bool result = false;

            XmlNode logFileConfigXml;

            logFileConfigXml = documentElement;

            if (logFileConfigXml != null && logFileConfigXml.HasChildNodes)
            {
                Config = ParseLogFileConfigXml(logFileConfigXml);
                result = true;
            }

            return result;
        }

        private TextFileLogConfiguration ParseLogFileConfigXml(XmlNode logFileConfigXml)
        {
            XPathNavigator nav = logFileConfigXml.CreateNavigator();
            MoveNavigatorToChildNode(ref nav, "Level");
            LogLevel level = (LogLevel)Enum.Parse(typeof(LogLevel), nav.Value);

            MoveNavigatorToSiblingNode(ref nav, "FilePath");
            string logFilePath = nav.Value;

            MoveNavigatorToSiblingNode(ref nav, "RotateOption");
            RotateOptions logRotateOption = (RotateOptions)Enum.Parse(typeof(RotateOptions), nav.Value);

            MoveNavigatorToSiblingNode(ref nav, "RotateLimit");
            uint logRotateLimit = uint.Parse(nav.Value);

            MoveNavigatorToSiblingNode(ref nav, "MaxFiles");
            uint logMaxFiles = uint.Parse(nav.Value);

            return new TextFileLogConfiguration()
            {
                Level = level,
                FilePath = logFilePath,
                RotateOption = logRotateOption,
                MaxFiles = logMaxFiles,
                RotateLimit = logRotateLimit
            };
        }
    }
}
