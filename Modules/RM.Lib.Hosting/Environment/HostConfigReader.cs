using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using RM.Lib.Common.Contracts.Log;
using RM.Lib.Hosting.Contracts.Environment;
using StringLookup = RM.Lib.Utility.Lookup<string, string>;

namespace RM.Lib.Hosting.Environment
{
	internal sealed class HostConfigReader
	{
		public const string HostConfig = "host.config";
		public const string SchemaUrn = "urn:rm/" + HostConfig;

		private const string _resNotFoundFormat = "Resource host.config not found in assembly {0}.";

		private readonly ILog _log;
		private readonly List<ConfigModule> _modules;
		private readonly List<ConfigSection> _sections;
        private readonly StringLookup _configElements;
		
		public HostConfigReader(ILog log)
		{
			_log = log;
			_modules = new List<ConfigModule>();
			_sections = new List<ConfigSection>();
            _configElements = new StringLookup();
		}

		public IEnumerable<ConfigModule> Modules => _modules;

		public IEnumerable<ConfigSection> Sections => _sections;

        public ILookup<string, string> Configs => _configElements;

		public bool ReadDefaultResource(bool throwIfUnsuccessful = true)
		{
			var scannedAssemblies = new HashSet<Assembly>();
			var assemblies = new[] { Assembly.GetExecutingAssembly(), Assembly.GetCallingAssembly(), Assembly.GetEntryAssembly() };

			foreach (var asm in assemblies.Where(scannedAssemblies.Add))
			{
				try
				{
					Read(asm);
					_log?.Info("Host config resource found.");
					return true;
				}
				catch (Exception e)
				{
					_log?.Info(e.ToString());
				}
			}

			return throwIfUnsuccessful ? throw new Exception("Host Configuration not found!") : false;
		}

		public bool ReadDefaultFile(bool throwIfUnsuccessful = true)
		{
			var configs = new[] { HostConfig, @"Properties\" + HostConfig };
			var configDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? @".\";

			foreach (var configName in configs)
			{
				var fullName = Path.Combine(configDir, configName);

				try
				{
					Read(fullName);
					_log?.Info("Host config file loaded.");
					return true;
				}
				catch (Exception e)
				{
					_log?.Info("Cannot read host config '{0}':\n{1}", fullName, e);
				}
			}

			return throwIfUnsuccessful ? throw new Exception("Host Configuration not found!") : false;
		}

		public void Read(Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			using (var resStream = GetConfigResourceStream(assembly))
			{
				if (resStream == null)
				{
					throw new Exception($"Resource host.config not found in assembly {assembly.GetName().Name}.");
				}

				LoadConfig(resStream);
			}
		}

		public void Read(string configFilePath)
		{
			if (String.IsNullOrWhiteSpace(configFilePath))
			{
				throw new ArgumentNullException(nameof(configFilePath));
			}

			if (!File.Exists(configFilePath))
			{
				throw new FileNotFoundException("Environment configuration file not found.", configFilePath);
			}

			using (var stream = File.OpenRead(configFilePath))
			{
				LoadConfig(stream);
			}
		}

		public void Read(Stream configFileStream)
		{
			if (configFileStream == null)
			{
				throw new ArgumentNullException(nameof(configFileStream));
			}

			LoadConfig(configFileStream);
		}

		private static Stream GetConfigResourceStream(Assembly assembly)
		{
			var assemblyName = assembly.GetName().Name;
			return assembly.GetManifestResourceStream($"{assemblyName}.Properties.host.config");
		}

		private void LoadConfig(Stream fileStream)
		{
			var doc = XDocument.Load(fileStream);
			LoadElements(doc);
		}

		private void LoadElements(XDocument doc)
		{
            var root = doc.Root;

            if (root == null)
            {
                return;
            }

            var moduleElement = root.Element(XName.Get("modules", SchemaUrn));

            if (moduleElement != null)
            {
                foreach (var componentNode in moduleElement.Elements(XName.Get("module", SchemaUrn)))
                {

                    var attr = componentNode.Attribute("assembly");
                    var element = new ConfigModule(attr?.Value);

                    _modules.Add(element);
                }
            }

            var sectionElement = root.Element(XName.Get("sections", SchemaUrn));

            if (sectionElement != null)
            {
                foreach (var sectionNode in sectionElement.Elements(XName.Get("section", SchemaUrn)))
                {
                    var nameAttr = sectionNode.Attribute("name");
                    var typeAttr = sectionNode.Attribute("type");

                    var element = new ConfigSection(nameAttr?.Value, typeAttr?.Value);

                    _sections.Add(element);
                }
            }

            var configElement = root.Element(XName.Get("config", SchemaUrn));

            if (configElement != null)
            {
                foreach (var element in configElement.Elements())
                {
                    _configElements.Add(element.Name.LocalName, element.ToString(SaveOptions.DisableFormatting));
                }
            }
		}
	}
}