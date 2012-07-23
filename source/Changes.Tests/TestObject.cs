using System.Collections.Generic;

namespace Changes.Tests
{
	public class TestObject
	{
		public TestEnum TestEnum { get; set; }
		public int TestInteger { get; set; }
		public string TestString { get; set; }
		public IEnumerable<int> TestIntegerEnumeration { get; set; }
	}
}
