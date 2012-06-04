using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using Newtonsoft.Json;

namespace Changes.Json.Tests
{
	// the DynamicObject class does not behave as expected; dynamic members throw
	public class DefaultDynamicObject : DynamicObject
	{
		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			return base.TrySetMember(binder, value);
		}
	}

	[TestFixture]
	public class DynamicJsonConverterTests
	{
		[Test]
		public void ReadJson_PutsPropertiesOnDynamicObject()
		{
			var json = JsonConvert.SerializeObject(new
			{
				PropertyOne = "Value",
				PropertyTwo = 42
			});

			var dyn = JsonConvert.DeserializeObject<dynamic>(json);

			Assert.That(dyn.PropertyOne, Is.EqualTo("Value"));
			Assert.That(dyn.PropertyTwo, Is.EqualTo(42));
		}

		[Test]
		public void WriteJson_SerializesDynamicProperties()
		{
			dynamic dyn = new DefaultDynamicObject();

			dyn.PropertyOne = "Value";
			dyn.PropertyTwo = 42;

			var json = JsonConvert.SerializeObject(dyn, new DynamicJsonConverter());

			Assert.That(json, Is.EqualTo("{\"PropertyOne\":\"Value\"}"));
		}
	}
}
