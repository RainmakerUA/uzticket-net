using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace RM.UzTicket.Hosting.Environment
{
	public sealed class ModuleConfigReader
	{
		private const string _schemaUrn = "urn:rm/module.config";

		private readonly List<ConfigDependency> _dependenciesInternal = new List<ConfigDependency>();

		//public void Read()
		//{
		//	var assembly = Assembly.GetExecutingAssembly();
		//	var assemblyName = assembly.GetName().Name;
		//	using (var resourceStream = assembly.GetManifestResourceStream(assemblyName + ".module.config"))
		//	{
		//		if (resourceStream == null)
		//			throw new Exception(string.Format("Resource module.config not found in assembly {0}.", assemblyName));

		//		LoadConfig(resourceStream);
		//	}
		//}

		public void Read(string configFilePath)
		{
			if (string.IsNullOrWhiteSpace(configFilePath))
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

		private void LoadConfig(Stream fileStream)
		{
			var doc = XDocument.Load(fileStream);
			LoadElements(doc);
		}

		private void LoadElements(XDocument doc)
		{
			foreach (var componentNode in doc.Root.Element(XName.Get("externalDependencies", _schemaUrn)).Elements(XName.Get("dependency", _schemaUrn)))
			{
				var element = new ConfigDependency();

				var attr = componentNode.Attribute("from");
				element.From = attr?.Value;

				attr = componentNode.Attribute("to");								
				element.To = attr?.Value;

				attr = componentNode.Attribute("lifetime");
				element.Lifetime = attr == null ? Lifetime.PerCall : (Lifetime)Enum.Parse(typeof(Lifetime), attr.Value);

				attr = componentNode.Attribute("construction");
				element.Construction = attr == null ? Construction.Mapped : (Construction)Enum.Parse(typeof(Construction), attr.Value);

				attr = componentNode.Attribute("constructionFactory");
				element.ConstructionFactory = attr?.Value;

				attr = componentNode.Attribute("constructionMethod");
				element.ConstructionMethod = attr?.Value;

				attr = componentNode.Attribute("name");
				element.Name = attr?.Value;

				_dependenciesInternal.Add(element);
			}
		}

		public IEnumerable<ConfigDependency> Dependencies => _dependenciesInternal;
	}
}
