// ReSharper disable InconsistentNaming

using System;

using NUnit.Framework;

using Newtonsoft.Json;

namespace Changes.Tests
{
	[TestFixture]
	public class ChangeSetTests
	{
		[Test]
		public void TryGetIndex_DynamicIndexSet_ReturnsIndexedValue()
		{
			dynamic changeSet = new ChangeSet<TestObject>();

			changeSet["Hello"] = "World";

			Assert.That(changeSet["Hello"], Is.EqualTo("World"));
		}

		[Test]
		public void TryGetIndex_DynamicPropertyExists_ReturnsPropertyValue()
		{
			dynamic changeSet = new ChangeSet<TestObject>();

			changeSet.Hello = "World";

			Assert.That(changeSet["Hello"], Is.EqualTo("World"));
		}

		[Test]
		public void TryGetIndex_DynamicIndexNotSet_ThrowsException()
		{
			dynamic changeSet = new ChangeSet<TestObject>();

			Assert.That(() => changeSet["Hello"], Throws.Exception);
		}

		[Test]
		public void TryGetMember_DynamicPropertyExists_ReturnsPropertyValue()
		{
			dynamic changeSet = new ChangeSet<TestObject>();

			changeSet.Hello = "World";

			Assert.That(changeSet.Hello, Is.EqualTo("World"));
		}

		[Test]
		public void TryGetMember_DynamicIndexSet_ReturnsPropertyValue()
		{
			dynamic changeSet = new ChangeSet<TestObject>();

			changeSet["Hello"] = "World";

			Assert.That(changeSet.Hello, Is.EqualTo("World"));
		}

		[Test]
		public void TryGetMember_DynamicPropertyDoesNotExist_ThrowsException()
		{
			dynamic changeSet = new ChangeSet<TestObject>();

			Assert.That(() => changeSet.Hello, Throws.Exception);
		}

		[Test]
		public void GetDynamicMemberNames_NoDynamicPropertiesSet_ReturnsEmptySet()
		{
			var changeSet = new ChangeSet<TestObject>();

			Assert.That(changeSet.GetDynamicMemberNames(), Is.Empty);
		}

		[Test]
		public void GetDynamicMemberNames_ThreeDynamicPropertiesSet_ReturnsSetOfDynamicPropertyNames()
		{
			dynamic changeSet = new ChangeSet<TestObject>();

			changeSet.HelloWorld = 0;
			changeSet.OhMyGoggles = "Hello, World.";
			changeSet.Count = Guid.NewGuid();

			var dynamicMemberNames = changeSet.GetDynamicMemberNames();

			Assert.That(dynamicMemberNames, Is.Not.Empty);
			Assert.That(dynamicMemberNames, Has.Count.EqualTo(3));
			Assert.That(dynamicMemberNames, Contains.Item("HelloWorld"));
			Assert.That(dynamicMemberNames, Contains.Item("HelloWorld"));
			Assert.That(dynamicMemberNames, Contains.Item("HelloWorld"));
		}

		[Test]
		public void HasChanges_EmptyChangeSet_ReturnsFalse()
		{
			var changeSet = new ChangeSet<TestObject>();

			Assert.That(changeSet.HasChanges, Is.False);
		}

		[Test]
		public void HasChanges_NonEmptyChangeSet_ReturnsFalse()
		{
			dynamic changeSet = new ChangeSet<TestObject>();

			changeSet.TestString = "The quick brown fox jumped over the lazy dog.";

			Assert.That(changeSet.HasChanges, Is.True);
		}

		[Test]
		public void HasChangeFor_NonPropertyExpression_ThrowsArgumentException()
		{
			var changeSet = new ChangeSet<TestObject>();

			Assert.That(() => changeSet.HasChangeFor(x => String.Format("{0}", x.TestString)), Throws.ArgumentException);
		}

		[Test]
		public void HasChangeFor_PropertyFoundInChangeSet_ReturnsTrue()
		{
			const string changeSetValue = "Changed";

			dynamic changeSet = new ChangeSet<TestObject>();

			changeSet.TestString = changeSetValue;

			Assert.That(((ChangeSet<TestObject>)changeSet).HasChangeFor(x => x.TestInteger), Is.False);
		}

		[Test]
		public void HasChangeFor_PropertyNotFoundInChangeSet_ReturnsFalse()
		{
			const string changeSetValue = "Changed";

			dynamic changeSet = new ChangeSet<TestObject>();

			changeSet.TestString = changeSetValue;

			Assert.That(((ChangeSet<TestObject>) changeSet).HasChangeFor(x => x.TestInteger), Is.False);
		}

		[Test]
		public void SetChangeFor_NonPropertyExpression_ThrowsArgumentException()
		{
			var changeSet = new ChangeSet<TestObject>();

			Assert.That(() => changeSet.SetChangeFor(x => String.Format("{0}", x.TestString), "OMG"), Throws.ArgumentException);
		}

		[Test]
		public void SetChangeFor_ValueAccessibleAsProperty()
		{
			const string changeSetValue = "Changed";

			var changeSet = new ChangeSet<TestObject>();

			changeSet.SetChangeFor(x => x.TestString, changeSetValue);

			dynamic cs = changeSet;

			Assert.That(cs.TestString, Is.EqualTo(changeSetValue));
		}

		[Test]
		public void SetChangeFor_ValueAccessibleAsIndexedValue()
		{
			const string changeSetValue = "Changed";

			var changeSet = new ChangeSet<TestObject>();

			changeSet.SetChangeFor(x => x.TestString, changeSetValue);

			dynamic cs = changeSet;

			Assert.That(cs["TestString"], Is.EqualTo(changeSetValue));
		}

		[Test]
		public void GetChangeFor_NonPropertyExpression_ThrowsArgumentException()
		{
			var changeSet = new ChangeSet<TestObject>();

			Assert.That(() => changeSet.GetChangeFor(x => String.Format("{0}", x.GetType())), Throws.ArgumentException);
		}

		[Test]
		public void GetChangeFor_ReferenceProperty_ReturnsChangedValue()
		{
			const string changeSetValue = "hello";

			var serializedChangeSet = JsonConvert.SerializeObject(new
			{
				TestString = changeSetValue
			});

			var changeSet = JsonConvert.DeserializeObject<ChangeSet<TestObject>>(serializedChangeSet);

			Assert.That(changeSet.GetChangeFor(x => x.TestString), Is.EqualTo(changeSetValue));
		}

		[Test]
		public void GetChangeFor_ValueTypeProperty_ReturnsChangedValue()
		{
			const int changeSetValue = 3;

			var serializedChangeSet = JsonConvert.SerializeObject(new
			{
				TestInteger = changeSetValue
			});

			var changeSet = JsonConvert.DeserializeObject<ChangeSet<TestObject>>(serializedChangeSet);

			Assert.That(changeSet.GetChangeFor(x => x.TestInteger), Is.EqualTo(changeSetValue));
		}

		[Test]
		public void GetChangeFor_EnumProperty_ReturnsChangedValue()
		{
			const TestEnum changeSetValue = TestEnum.ValueOne;

			var serializedChangeSet = JsonConvert.SerializeObject(new
			{
				TestEnum = changeSetValue
			});

			var changeSet = JsonConvert.DeserializeObject<ChangeSet<TestObject>>(serializedChangeSet);

			Assert.That(changeSet.GetChangeFor(x => x.TestEnum), Is.EqualTo(changeSetValue));
		}

		[Test]
		public void GetChangeFor_ReferencePropertyNotInChangeSet_ThrowsChangeNotFoundException()
		{
			var changeSet = new ChangeSet<TestObject>();

			Assert.That(() => changeSet.GetChangeFor(x => x.TestString), Throws.InstanceOf<ChangeNotFoundException>());
		}

		[Test]
		public void GetChangeFor_ValuePropertyNotInChangeSet_ThrowsChangeNotFoundException()
		{
			var changeSet = new ChangeSet<TestObject>();

			Assert.That(() => changeSet.GetChangeFor(x => x.TestInteger), Throws.InstanceOf<ChangeNotFoundException>());
		}

		[Test]
		public void GetChangeFor_EnumPropertyNotInChangeSet_ThrowsChangeNotFoundException()
		{
			var changeSet = new ChangeSet<TestObject>();

			Assert.That(() => changeSet.GetChangeFor(x => x.TestEnum), Throws.InstanceOf<ChangeNotFoundException>());
		}

		[Test]
		public void TryGetChangeFor_NonPropertyExpression_ReturnsFalse_OutputsDefaultValue()
		{
			var changeSet = new ChangeSet<TestObject>();

			string result;
			var success = changeSet.TryGetChangeFor(x => String.Format("{0}", x.TestString), out result);

			Assert.That(result, Is.EqualTo(default(string)));
			Assert.That(success, Is.False);
		}

		[Test]
		public void TryGetChangeFor_ReferenceProperty_ReturnsTrue_OutputsChangedValue()
		{
			const string changeSetValue = "hello";

			var serializedChangeSet = JsonConvert.SerializeObject(new
			{
				TestString = changeSetValue
			});

			var changeSet = JsonConvert.DeserializeObject<ChangeSet<TestObject>>(serializedChangeSet);

			string result;
			var success = changeSet.TryGetChangeFor(x => x.TestString, out result);

			Assert.That(result, Is.EqualTo(changeSetValue));
			Assert.That(success, Is.True);
		}

		[Test]
		public void TryGetChangeFor_ValueTypeProperty_ReturnsTrue_OutputsChangedValue()
		{
			const int changeSetValue = 3;

			var serializedChangeSet = JsonConvert.SerializeObject(new
			{
				TestInteger = changeSetValue
			});

			var changeSet = JsonConvert.DeserializeObject<ChangeSet<TestObject>>(serializedChangeSet);

			int result;
			var success = changeSet.TryGetChangeFor(x => x.TestInteger, out result);

			Assert.That(result, Is.EqualTo(changeSetValue));
			Assert.That(success, Is.True);
		}

		[Test]
		public void TryGetChangeFor_EnumProperty_ReturnsTrue_OutputsChangedValue()
		{
			const TestEnum changeSetValue = TestEnum.ValueOne;

			var serializedChangeSet = JsonConvert.SerializeObject(new
			{
				TestEnum = changeSetValue
			});

			var changeSet = JsonConvert.DeserializeObject<ChangeSet<TestObject>>(serializedChangeSet);

			TestEnum result;
			var success = changeSet.TryGetChangeFor(x => x.TestEnum, out result);

			Assert.That(result, Is.EqualTo(changeSetValue));
			Assert.That(success, Is.True);
		}

		[Test]
		public void TryGetChangeFor_ReferencePropertyNotInChangeSet_ReturnsFalse_OutputsDefaultValue()
		{
			var changeSet = new ChangeSet<TestObject>();

			string result;
			var success = changeSet.TryGetChangeFor(x => x.TestString, out result);

			Assert.That(result, Is.EqualTo(default(string)));
			Assert.That(success, Is.False);
		}

		[Test]
		public void TryGetChangeFor_ValueTypePropertyNotInChangeSet_ReturnsFalse_OutputsDefaultValue()
		{
			var changeSet = new ChangeSet<TestObject>();

			int result;
			var success = changeSet.TryGetChangeFor(x => x.TestInteger, out result);

			Assert.That(result, Is.EqualTo(default(int)));
			Assert.That(success, Is.False);
		}

		[Test]
		public void TryGetChangeFor_EnumPropertyNotInChangeSet_ReturnsFalse_OutputsDefaultValue()
		{
			var changeSet = new ChangeSet<TestObject>();

			TestEnum result;
			var success = changeSet.TryGetChangeFor(x => x.TestEnum, out result);

			Assert.That(result, Is.EqualTo(default(TestEnum)));
			Assert.That(success, Is.False);
		}

		[Test]
		public void ApplyChanges_ReferencePropertyOnTypeChanged_PropertyInChangeSetIsChanged()
		{
			const TestEnum testEnumValue = TestEnum.ValueOne;
			const int testIntegerValue = 42;
			const string testStringValue = "The quick brown fox jumped over the lazy dog.";
			var testIntegerEnumeration = new[] { 1, 2, 3, 4, 5 };

			const string changedStringValue = "Hello, World";

			var testObject = new TestObject
			{
				TestEnum = testEnumValue,
				TestInteger = testIntegerValue,
				TestString = testStringValue,
				TestIntegerEnumeration = testIntegerEnumeration
			};

			dynamic changeSet = new ChangeSet<TestObject>();

			changeSet.TestString = changedStringValue;

			changeSet.ApplyChanges(ref testObject);

			Assert.That(testObject, Is.Not.Null);
			Assert.That(testObject.TestEnum, Is.EqualTo(testEnumValue));
			Assert.That(testObject.TestInteger, Is.EqualTo(testIntegerValue));
			Assert.That(testObject.TestString, Is.EqualTo(changedStringValue));
		}

		[Test]
		public void ApplyChanges_ValuePropertyOnTypeChanged_PropertyInChangeSetIsChanged()
		{
			const TestEnum testEnumValue = TestEnum.ValueOne;
			const int testIntegerValue = 42;
			const string testStringValue = "The quick brown fox jumped over the lazy dog.";
			var testIntegerEnumeration = new[] { 1, 2, 3, 4, 5 };

			const int changedIntegerValue = 24;

			var testObject = new TestObject
			{
				TestEnum = testEnumValue,
				TestInteger = testIntegerValue,
				TestString = testStringValue,
				TestIntegerEnumeration = testIntegerEnumeration
			};

			dynamic changeSet = new ChangeSet<TestObject>();

			changeSet.TestInteger = changedIntegerValue;

			changeSet.ApplyChanges(ref testObject);

			Assert.That(testObject, Is.Not.Null);
			Assert.That(testObject.TestEnum, Is.EqualTo(testEnumValue));
			Assert.That(testObject.TestInteger, Is.EqualTo(changedIntegerValue));
			Assert.That(testObject.TestString, Is.EqualTo(testStringValue));
		}

		[Test]
		public void ApplyChanges_EnumPropertyOnTypeChanged_PropertyInChangeSetIsChanged()
		{
			const TestEnum testEnumValue = TestEnum.ValueOne;
			const int testIntegerValue = 42;
			const string testStringValue = "The quick brown fox jumped over the lazy dog.";
			var testIntegerEnumeration = new[] { 1, 2, 3, 4, 5 };

			const TestEnum changedEnumValue = TestEnum.ValueTwo;

			var testObject = new TestObject
			{
				TestEnum = testEnumValue,
				TestInteger = testIntegerValue,
				TestString = testStringValue,
				TestIntegerEnumeration = testIntegerEnumeration
			};

			dynamic changeSet = new ChangeSet<TestObject>();

			changeSet.TestEnum = changedEnumValue;

			changeSet.ApplyChanges(ref testObject);

			Assert.That(testObject, Is.Not.Null);
			Assert.That(testObject.TestEnum, Is.EqualTo(changedEnumValue));
			Assert.That(testObject.TestInteger, Is.EqualTo(testIntegerValue));
			Assert.That(testObject.TestString, Is.EqualTo(testStringValue));
		}

		[Test]
		public void ApplyChanges_IntegerEnumerationPropertyOnTypeChanged_PropertyInChangeSetIsChanged()
		{
			const TestEnum testEnumValue = TestEnum.ValueOne;
			const int testIntegerValue = 42;
			const string testStringValue = "The quick brown fox jumped over the lazy dog.";
			var testIntegerEnumeration = new[] { 1, 2, 3, 4, 5 };

			var changedIntegerEnumeration = new[] { 6, 7, 8 };

			var testObject = new TestObject
			{
				TestEnum = testEnumValue,
				TestInteger = testIntegerValue,
				TestString = testStringValue,
				TestIntegerEnumeration = testIntegerEnumeration
			};

			dynamic changeSet = new ChangeSet<TestObject>();

			changeSet.TestIntegerEnumeration = changedIntegerEnumeration;

			changeSet.ApplyChanges(ref testObject);

			Assert.That(testObject, Is.Not.Null);
			Assert.That(testObject.TestEnum, Is.EqualTo(testEnumValue));
			Assert.That(testObject.TestInteger, Is.EqualTo(testIntegerValue));
			Assert.That(testObject.TestString, Is.EqualTo(testStringValue));
			Assert.That(testObject.TestIntegerEnumeration, Is.EquivalentTo(changedIntegerEnumeration));
		}
	}
}
