using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Changes.Json
{
	public class ChangeSetJsonConverter : JsonConverter
	{
		public static readonly Type OpenGenericChangeSetType = typeof(ChangeSet<>);

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var result = new Dictionary<string, object>();

			dynamic dynamicObject = value;

			var dynamicProperties = dynamicObject.GetDynamicMemberNames();

			foreach (var dynamicProperty in dynamicProperties)
			{
				var key = dynamicProperty;

				object propertyValue = dynamicObject[dynamicProperty];

				result.Add(key, propertyValue);
			}

			serializer.Serialize(writer, result);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return serializer.Deserialize(reader, objectType);
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType.IsGenericType && (objectType.GetGenericTypeDefinition() == OpenGenericChangeSetType);
		}
	}
}
