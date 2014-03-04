using System;
using System.Collections.Generic;

namespace Changes.Tests
{
	public class TestObject
	{
		public TestEnum TestEnum { get; set; }
		public int TestInteger { get; set; }
		public int? TestNullableInteger { get; set; }
		public string TestString { get; set; }
		public Guid TestGuid { get; set; }
		public Guid? TestNullableGuid { get; set; }
		public IEnumerable<int> TestIntegerEnumeration { get; set; }
		public IEnumerable<TestObject> Children { get; set; }
	}
}
