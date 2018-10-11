using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using RM.Lib.Common.Contracts.Log;
using RM.Lib.Hosting.Contracts.Environment;

namespace RM.Lib.Hosting.Environment
{
	public sealed class HostConfigReader
	{
		private const string _schemaUrn = "urn:rm/host.config";
		private const string _resNotFoundFormat = "Resource host.config not found in assembly {0}.";

		private readonly ILog _log;
		private readonly List<ConfigModule> _modules = new List<ConfigModule>();
		private readonly List<ConfigSection> _sections = new List<ConfigSection>();

		public HostConfigReader(ILog log)
		{
			_log = log;
		}

		public void Read()
		{
			var assemblies = new[] { Assembly.GetExecutingAssembly(), Assembly.GetCallingAssembly(), Assembly.GetEntryAssembly() };

			foreach (var asm in assemblies)
			{
				using (var stream = GetConfigResourceStream(asm))
				{
					if (stream != null)
					{
						_log?.Info("Host config resource found.");
						LoadConfig(stream);
						return;
					}

					_log?.Info(_resNotFoundFormat, asm.GetName().Name);
				}
			}

			throw new Exception("Host Configuration not found!");
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

		private Stream GetConfigResourceStream(Assembly assembly)
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
			foreach (var componentNode in doc.Root.Element(XName.Get("modules", _schemaUrn)).Elements(XName.Get("module", _schemaUrn)))
			{
				var element = new ConfigModule();

				var attr = componentNode.Attribute("assembly");
				element.Assembly = attr?.Value;

				_modules.Add(element);
			}

			foreach (var sectionNode in doc.Root.Element(XName.Get("sections", _schemaUrn)).Elements(XName.Get("section", _schemaUrn)))
			{
				var element = new ConfigSection();

				var attr = sectionNode.Attribute("section");
				element.SectionType = attr?.Value;

				attr = sectionNode.Attribute("provider");
				element.ProviderType = attr?.Value;

				_sections.Add(element);
			}
		}

		public IEnumerable<ConfigModule> Modules => _modules;

		public IEnumerable<ConfigSection> Sections => _sections;
	}
}