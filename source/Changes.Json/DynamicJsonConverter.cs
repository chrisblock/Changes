using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;

using Microsoft.CSharp.RuntimeBinder;

using Newtonsoft.Json;

namespace Changes.Json
{
	public class DynamicJsonConverter : JsonConverter
	{
		private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, CallSite<Func<CallSite, object, object>>>> CallSiteCache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, CallSite<Func<CallSite, object, object>>>>();
		private static readonly object CallSiteCacheLock = new object();

		public static readonly Type DynamicType = typeof (DynamicObject);

		private static object GetDynamicMember(object obj, string memberName)
		{
			var type = obj.GetType();

			var binder = Binder.GetMember(CSharpBinderFlags.None, memberName, obj.GetType(), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

			if (CallSiteCache.ContainsKey(type) == false)
			{
				lock (CallSiteCacheLock)
				{
					if (CallSiteCache.ContainsKey(type) == false)
					{
						var typeCache = new ConcurrentDictionary<string, CallSite<Func<CallSite, object, object>>>();

						typeCache[memberName] = CallSite<Func<CallSite, object, object>>.Create(binder);

						CallSiteCache[type] = typeCache;
					}
					else
					{
						var typeCache = CallSiteCache[type];

						if (typeCache.ContainsKey(memberName))
						{
							typeCache[memberName] = CallSite<Func<CallSite, object, object>>.Create(binder);
						}
					}
				}
			}

			var callsite = CallSiteCache[type][memberName];

			return callsite.Target(callsite, obj);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var result = new Dictionary<string, object>();

			var dynamicObject = (DynamicObject) value;

			var dynamicProperties = dynamicObject.GetDynamicMemberNames();

			foreach (var dynamicProperty in dynamicProperties)
			{
				var key = dynamicProperty;

				object propertyValue = GetDynamicMember(dynamicObject, dynamicProperty);

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
			return DynamicType.IsAssignableFrom(objectType);
		}
	}
}
