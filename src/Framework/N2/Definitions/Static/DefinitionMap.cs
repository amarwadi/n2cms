﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using N2.Engine;

namespace N2.Definitions.Static
{
	[Service(StaticAccessor = "Instance")]
	public class DefinitionMap
	{
		// static

		static DefinitionMap()
		{
			N2.Engine.Singleton<DefinitionMap>.Instance = new DefinitionMap();
		}

		public static DefinitionMap Instance 
		{
			get { return N2.Engine.Singleton<DefinitionMap>.Instance; }
		}

		// instance

		private Dictionary<string, ItemDefinition> definitions = new Dictionary<string, ItemDefinition>();

		public ItemDefinition GetOrCreateDefinition(Type contentType)
		{
			return GetOrCreateDefinition(contentType, null);
		}

		public ItemDefinition GetOrCreateDefinition(Type contentType, string templateKey)
		{
			if (contentType == null) throw new ArgumentNullException("contentType");

			return GetDefinition(contentType, templateKey)
				?? CreateDefinition(contentType, templateKey);
		}

		public ItemDefinition GetOrCreateDefinition(ContentItem item)
		{
			return GetOrCreateDefinition(item.GetContentType(), item.TemplateKey);
		}

		private ItemDefinition GetDefinition(Type contentType, string templateKey)
		{
			string key = contentType.FullName + templateKey;
			ItemDefinition definition;
			if (definitions.TryGetValue(key, out definition))
				return definition;

			return null;
		}

		private ItemDefinition CreateDefinition(Type contentType, string templateKey)
		{
			ItemDefinition definition = GetDefinition(contentType, null);
			if (definition != null)
				definition = definition.Clone();
			else
				definition = new ItemDefinition(contentType);

			definition.Template = templateKey;
			definition.Initialize(contentType);

			SetDefinition(contentType, templateKey, definition);

			return definition;
		}

		public IEnumerable<ItemDefinition> GetDefinitions()
		{
			return definitions.Values.ToList();
		}

		public void SetDefinition(Type contentType, string templateKey, ItemDefinition definition)
		{
			if (contentType == null) throw new ArgumentNullException("contentType");

			string key = contentType.FullName + templateKey;

			var temp = new Dictionary<string, ItemDefinition>(definitions);
			if (definition != null)
				temp[key] = definition;
			else if (definitions.ContainsKey(key))
				temp.Remove(key);
			definitions = temp;
		}

		public void Clear()
		{
			definitions = new Dictionary<string, ItemDefinition>();
		}
	}
}
