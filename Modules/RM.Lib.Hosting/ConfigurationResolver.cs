using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using RM.Lib.Hosting.Contracts.Config;
using RM.Lib.Hosting.Contracts.Environment;
using RM.Lib.Hosting.Environment;

namespace RM.Lib.Hosting
{
	internal sealed class ConfigurationResolver
	{
		private readonly IEnumerable<ConfigSection> _sectionMappings;
		private readonly ILookup<string, string> _configSections;

		public ConfigurationResolver(IEnumerable<ConfigSection> sectionMappings, ILookup<string, string> configSections)
		{
			_sectionMappings = sectionMappings;
			_configSections = configSections;
		}

		public object GetSingleSection(Type sectionType)
		{
			var section = GetSection(sectionType)?.FirstOrDefault();
			return ParseConfigSection(sectionType, section ?? ThrowSectionNotFound<string>(sectionType));
		}

		public Array GetMultipleSections(Type sectionType)
		{
			var sections = GetSection(sectionType)?.ToArray() ?? ThrowSectionNotFound<string[]>(sectionType);
			var length = sections.Length;
			var result = Array.CreateInstance(sectionType, length);

			for (int i = 0; i < length; i++)
			{
				result.SetValue(ParseConfigSection(sectionType, sections[i]), i);
			}

			return result;
		}

		private IEnumerable<string> GetSection(Type type)
		{
			var sectionName = _sectionMappings.FirstOrDefault(map => IsTypeName(type, map.Type))?.Name;

			if (String.IsNullOrEmpty(sectionName))
			{
				sectionName = type.GetCustomAttribute<ConfigurationSectionAttribute>()?.SectionName;
			}

			/*if (String.IsNullOrEmpty(sectionName))
			{
				sectionName = GetCamelCaseName(type.Name.Replace());
			}*/

			return _configSections.Contains(sectionName) ? _configSections[sectionName] : null;
		}

		private static object ParseConfigSection(Type type, string section)
		{
			var el = XElement.Parse(section);
			var props = type.GetProperties().Where(p => p.CanWrite);
			var result = Activator.CreateInstance(type);

			foreach (var prop in props)
			{
				var configPropAttr = prop.GetCustomAttribute<ConfigurationPropertyAttribute>();
				var propName = configPropAttr?.Name;
				var attrName = configPropAttr?.AttributeName;
				var propType = prop.PropertyType;
				var isArray = propType.IsArray;

				if (String.IsNullOrEmpty(propName))
				{
					propName = GetCamelCaseName(prop.Name);
				}

				if (isArray)
				{
					propType = propType.GetElementType();

					var xmlValues = el.Elements(GetName(propName)).Select(xel => GetElementValue(xel, attrName)).ToArray();
					var length = xmlValues.Length;
					var value = Array.CreateInstance(propType, length);

					for (var i = 0; i < length; i++)
					{
						value.SetValue(ConvertValue(xmlValues[i], propType), i);
					}

					prop.SetValue(result, value);
				}
				else
				{
					var xmlValue = GetElementValue(el.Elements(GetName(propName)).FirstOrDefault(), attrName)
									?? el.Attributes(propName).FirstOrDefault()?.Value;

					if (!String.IsNullOrEmpty(xmlValue))
					{
						prop.SetValue(result, ConvertValue(xmlValue, propType));
					}
				}
			}

			return result;

			XName GetName(string localName)
			{
				return XName.Get(localName, HostConfigReader.SchemaUrn);
			}

			string GetElementValue(XElement element, string attrName)
			{
				return element != null
					? String.IsNullOrEmpty(attrName) ? element.Value : element.Attribute(attrName)?.Value
					: null;
			}

			object ConvertValue(string xmlValue, Type targetType)
			{
				return Convert.ChangeType(xmlValue, targetType);
			}
		}

		private static bool IsTypeName(Type type, string name)
		{
			var nameParts = name.Split(new[] {',', '\u0020'}, StringSplitOptions.RemoveEmptyEntries);

			return type.FullName == nameParts[0] && type.Assembly.GetName().Name == nameParts[1];
		}

		private static T ThrowSectionNotFound<T>(Type sectionType)
		{
			throw new InvalidOperationException($"Section for {sectionType.FullName} is not found!");
		}

		private static string GetCamelCaseName(string name)
		{
			return Char.ToLowerInvariant(name[0]) + name.Substring(1);
		}
	}
}
