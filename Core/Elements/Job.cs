using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using System.Reflection;
using BAFactory.Moira.Core.Log;
using System.Threading.Tasks;

namespace BAFactory.Moira.Core.Elements
{
    internal class Job
    {
        private ExecutionManager executionManager;
        public string Id { get; set; }
        public bool Enabled { get; set; }
        public bool RunOnStartUp { get; set; }
        public uint Interval { get; set; }
        public string Path { get; set; }
        public string Error { get; set; }
        public FilePattern Pattern { get; set; }
        public List<StepsGroup> JobStepsGroups { get; set; }
        public Func<LogLevel, string, Task> LogMessageActionAsync { get; internal set; }
        private FileInfo CurrentFile { get; set; }
        public Job()
        {
            Pattern = new FilePattern();
            JobStepsGroups = new List<StepsGroup>();
            Id = string.Empty;
            Path = string.Empty;
            Pattern = new FilePattern();
            executionManager = new ExecutionManager();
        }
        public async Task<bool> ExecuteAsync()
        {
            await LogMessageActionAsync(LogLevel.Info, string.Concat("Executing Task with id: ", this.Id.ToString()));
            await LogMessageActionAsync(LogLevel.Debug, string.Concat("Executing Task on ", this.Path.ToString(), " with ", this.Pattern.Pattern, " as patern"));

            FileInfo[] matchingFiles = FilesManager.GetMatchingFiles(Path, Pattern);

            if (matchingFiles == null || matchingFiles.Length == 0)
            {
                await LogMessageActionAsync(LogLevel.Debug, "No matching files found");
                return (matchingFiles == null && matchingFiles.Length == 0);
            }

            await LogMessageActionAsync(LogLevel.Info, string.Format("Found {0} files", matchingFiles.Length));

            bool succeded = true;
            foreach (FileInfo fi in matchingFiles)
            {
                CurrentFile = fi;
                try
                {
                    await executionManager.ExecuteJob(fi, this.JobStepsGroups);
                }
                catch (Exception e)
                {
                    LogMessageActionAsync(LogLevel.Error, string.Format("Error: {0} - Stack {1}", e.Message, e.StackTrace)).RunSynchronously();
                }
            }

            await LogMessageActionAsync(LogLevel.Debug, "All files have been processed");

            return succeded;
        }
        public bool Revert()
        {
            // TODO: To be implemented
            return CurrentFile != null && CurrentFile.Exists;
        }
    }
}
