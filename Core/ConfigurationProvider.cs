using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using BAFactory.Moira.Core.Elements;
using BAFactory.Moira.FileAnalyzers;
using System.IO;
using BAFactory.Moira.Core.Log;
using System.Threading.Tasks;

namespace BAFactory.Moira.Core
{
    public class ConfigurationProvider : XmlConfigurationParser
    {
        private Dictionary<string, StepAssemblyInfo> stepsAssemblies;
        private Dictionary<string, FileAnalyzerAssemblyInfo> analyzersAssemblies;

        internal ILogProvider LogWriter { get; set; }
        internal Timetable TasksLists { get; set; }
        internal LogProviderAssemblyInfo LogProviderAssembly { get; set; }
        internal DispatcherProviderAssemblyInfo DispatcherAssembly { get; set; }
        internal Dispatcher Dispatcher { get; set; }
        internal ConfigurationProvider()
            : base("http://BAFactory.net/schemas/FilesDispatcher", "fdc")
        {
            if (!LoadConfigFile("FilesDispatcher.xml"))
            {
                throw new ApplicationException("Failed to read configuration file");
            }

            ReadStepsAssembliesList();

            ReadFileAnalyzersList();

            ReadTasksList();

            ReadLogAssembly();

            ReadDispatcherAssembly();

            InstantiateLogProvider();

            InstantiateDispatcher();        
        }
        private void InstantiateLogProvider()
        {
            LogProviderFactory factory = new LogProviderFactory();
            LogWriter = factory.InstantiateLogProvider(LogProviderAssembly);

            LogWriter.LogMessage(LogLevel.Debug, "Initializing Configuration");
            LogWriter.LogMessage(LogLevel.Debug, string.Concat("Configuration: ", TasksLists.Count, " tasks loaded"));
        }
        private void InstantiateDispatcher()
        {
            DispatcherFactory factory = new DispatcherFactory();
            Dispatcher = factory.InstantiateDispatcher(DispatcherAssembly);
            Dispatcher.Configuration = this;

            LogWriter.LogMessage(LogLevel.Debug, "Initializing Dispatcher");
        }
        private bool ReadStepsAssembliesList()
        {
            bool result = false;

            stepsAssemblies = new Dictionary<string, StepAssemblyInfo>();

            XmlNode stepsAssembliesConfig;

            stepsAssembliesConfig = GetDocumentNode("Assemblies/Steps");

            if (stepsAssembliesConfig != null && stepsAssembliesConfig.HasChildNodes)
            {
                foreach (XmlNode stepsAssembliesXml in stepsAssembliesConfig.ChildNodes)
                {
                    AssemblyInformation ai;
                    string stxml = ParseAssemblyInfoXml<StepAssemblyInfo>(stepsAssembliesXml, out ai);
                    if (!stepsAssemblies.ContainsKey(stxml))
                    {
                        stepsAssemblies.Add(stxml, ai as StepAssemblyInfo);
                    }
                }
                result = true;
            }

            return result;
        }
        private bool ReadFileAnalyzersList()
        {
            bool result = false;

            analyzersAssemblies = new Dictionary<string, FileAnalyzerAssemblyInfo>();

            XmlNode analyzersAssembliesConfig;

            analyzersAssembliesConfig = GetDocumentNode("Assemblies/FileAnalyzers");

            if (analyzersAssembliesConfig != null && analyzersAssembliesConfig.HasChildNodes)
            {
                foreach (XmlNode analyzersAssembliesXml in analyzersAssembliesConfig.ChildNodes)
                {
                    AssemblyInformation ai;
                    string stxml = ParseAssemblyInfoXml<FileAnalyzerAssemblyInfo>(analyzersAssembliesXml, out ai);
                    if (!analyzersAssemblies.ContainsKey(stxml))
                    {
                        analyzersAssemblies.Add(stxml, ai as FileAnalyzerAssemblyInfo);
                    }
                }
                result = true;
            }

            return result;
        }
        private string ParseAssemblyInfoXml<T>(XmlNode xml, out AssemblyInformation ai) where T : AssemblyInformation
        {
            string typeName = xml.Attributes["Type"].Value;
            string className = xml.Attributes["Class"].Value;

            string assemblyName = xml.Attributes["Assembly"].Value;
            string assemblyKey = xml.Attributes["PublicKeyToken"].Value;
            string assemblyVersion = xml.Attributes["Version"].Value;

            ai = Activator.CreateInstance(typeof(T), className, assemblyName, assemblyKey, assemblyVersion) as T;

            return typeName;
        }
        private bool ReadTasksList()
        {
            bool result = false;

            TasksLists = new Timetable();

            XmlNode tasks = GetDocumentNode("Tasks");

            if (tasks != null && tasks.HasChildNodes)
            {
                foreach (XmlNode taskXml in tasks.ChildNodes)
                {
                    Job t;

                    string taskName = ParseTask(taskXml, out t);
                    Timetable.Entry entry = new Timetable.Entry();
                    entry.Task = t;
                    TasksLists.Add(entry);
                }
            }

            return result;
        }
        private bool ReadLogAssembly()
        {
            bool result = false;

            XmlNode logAssemblyConfig;

            logAssemblyConfig = GetDocumentNode("Assemblies/LogProvider");

            if (logAssemblyConfig != null)
            {
                AssemblyInformation ai;
                string stxml = ParseAssemblyInfoXml<LogProviderAssemblyInfo>(logAssemblyConfig, out ai);
                LogProviderAssembly = ai as LogProviderAssemblyInfo;

                result = true;
            }

            return result;
        }
        private bool ReadDispatcherAssembly()
        {
            bool result = false;

            XmlNode dispatcherAssemblyConfig;

            dispatcherAssemblyConfig = GetDocumentNode("Assemblies/Dispatcher");

            if (dispatcherAssemblyConfig != null)
            {
                AssemblyInformation ai;
                string stxml = ParseAssemblyInfoXml<DispatcherProviderAssemblyInfo>(dispatcherAssemblyConfig, out ai);
                DispatcherAssembly = ai as DispatcherProviderAssemblyInfo;

                result = true;
            }

            return result;
        }
        private string ParseTask(XmlNode taskXml, out Job t)
        {
            t = new Job();

            string taskId = taskXml.Attributes["id"].Value;
            t.Id = taskId;

            bool enabled;

            if (!bool.TryParse(taskXml.Attributes["enabled"].Value, out enabled))
            {
                return string.Empty;
            }

            t.Enabled = enabled;

            if (taskXml.Attributes["runonstartup"] != null)
            {
                bool runOnStartUp;
                if (bool.TryParse(taskXml.Attributes["runonstartup"].Value, out runOnStartUp))
                {
                    t.RunOnStartUp = runOnStartUp;
                }
            }

            XPathNavigator nav = taskXml.CreateNavigator();
            MoveNavigatorToChildNode(ref nav, "BaseDirectory");
            MoveNavigatorToChildNode(ref nav, "Path");
            t.Path = nav.Value;

            MoveNavigatorToParentNode(ref nav);
            MoveNavigatorToChildNode(ref nav, "Pattern");
            t.Pattern.Pattern = nav.Value;

            string isRegExString = nav.GetAttribute("isRegEx", string.Empty);
            bool isRegEx = false;
            if (!bool.TryParse(isRegExString, out isRegEx))
            {
                isRegEx = false;
            }
            t.Pattern.IsRegEx = isRegEx;

            MoveNavigatorToParentNode(ref nav);
            MoveNavigatorToChildNode(ref nav, "Interval");

            uint interval = 60;
            if (!uint.TryParse(nav.Value, out interval))
            {
                interval = 1;
            }
            t.Interval = interval;

            t.LogMessageActionAsync = new Func<LogLevel, string, Task>(LogMessageAsync);

            MoveNavigatorToParentNode(ref nav, 2);
            MoveNavigatorToChildNode(ref nav, "StepsGroups");

            ParseTaskStepsGroups(nav, ref t);

            return taskId;
        }
        private void ParseTaskStepsGroups(XPathNavigator nav, ref Job t)
        {
            nav.MoveToFirstChild();

            do
            {
                ParseTaskStepsGroup(nav, ref t);
            } while (nav.MoveToNext());
        }
        private void ParseTaskStepsGroup(XPathNavigator nav, ref Job t)
        {
            StepsGroup g = new StepsGroup();

            string sgId = nav.GetAttribute("id", string.Empty);

            string breakConditionString = nav.GetAttribute("breakcondition", string.Empty);
            BreakCondition breakcondition = BreakCondition.Never;
            if (!string.IsNullOrEmpty(breakConditionString))
            {
                breakcondition = (BreakCondition)Enum.Parse(typeof(BreakCondition), breakConditionString);
            }
            g.BreakCondition = breakcondition;
            g.Id = sgId;

            g.LogMessageAction = new Func<LogLevel, string, Task>(LogMessageAsync);

            t.JobStepsGroups.Add(g);

            g = ParseTaskSteps(nav, g);

        }
        private StepsGroup ParseTaskSteps(XPathNavigator nav, StepsGroup g)
        {
            nav.MoveToFirstChild();
            do
            {
                ParseTaskStep(nav, ref g);
            } while (nav.MoveToNext());
            MoveNavigatorToParentNode(ref nav, 1);
            return g;
        }
        private void ParseTaskStep(XPathNavigator nav, ref StepsGroup g)
        {
            string stepType = nav.GetAttribute("Type", string.Empty);

            string breakconditionString = nav.GetAttribute("breakcondition", string.Empty);
            BreakCondition breakCondition = BreakCondition.Never;
            if (!string.IsNullOrEmpty(breakconditionString))
            {
                breakCondition = (BreakCondition)Enum.Parse(typeof(BreakCondition), breakconditionString);
            }

            // TODO: Add execution flgs parsing

            MoveNavigatorToChildNode(ref nav, "Parameters");
            Parameters parameters = ParseTaskStepParameters(nav);

            Step s = StepsFactory.CreateStep(stepsAssemblies[stepType], parameters);
            s.BreakCondition = breakCondition;

            s.LogMessageAction = new Func<LogLevel, string, Task>(LogMessageAsync);

            g.Add(s);

            MoveNavigatorToParentNode(ref nav, 2);
        }
        private Parameters ParseTaskStepParameters(XPathNavigator nav)
        {
            Parameters parameters = new Parameters();
            bool fromLastResult = false;

            if (nav.HasChildren)
            {
                nav.MoveToFirstChild();

                do
                {
                    Parameter parameter = new Parameter();
                    string name = nav.GetAttribute("name", string.Empty);
                    string analyzer = nav.GetAttribute("analyzer", string.Empty);
                    string attributeName = nav.GetAttribute("attributeName", string.Empty);
                    string attributeType = nav.GetAttribute("attributeType", string.Empty);
                    bool.TryParse(nav.GetAttribute("fromLastResult", string.Empty), out fromLastResult);

                    parameter.Name = name;
                    parameter.IsFromLastResult = fromLastResult;
                    if (!string.IsNullOrEmpty(analyzer))
                    {
                        FileAnalyzersFactory factory = new FileAnalyzersFactory();
                        parameter.Attribute.Analyzer = factory.InstantiateFileAnalyzer(analyzersAssemblies[analyzer]);
                        parameter.Attribute.Name = attributeName;
                        parameter.Attribute.Type = attributeType;
                    }
                    if (!fromLastResult)
                    {
                        parameter.Value = nav.Value;
                    }

                    parameters.Add(name, parameter);

                } while (nav.MoveToNext());
            }
            return parameters;
        }
        private async Task LogMessageAsync(LogLevel l, string m)
        {
            await LogWriter.LogMessageAsync(l, m);
        }
    }
}
