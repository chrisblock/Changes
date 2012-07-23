using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Changes
{
	public class ChangeSet<T> : DynamicObject
	{
		// ReSharper disable StaticFieldInGenericType
		// ReSharper warns us here because, for example, the values in ChangeSet<string> are totally seperate from the values in ChangeSet<int>
		//   this behavior, however, is exactly what we want
		private static readonly IEnumerable<PropertyInfo> Properties = typeof (T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
		// ReSharper restore StaticFieldInGenericType

		private readonly IDictionary<string, object> _changedMembers = new Dictionary<string, object>();

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			return _changedMembers.TryGetValue(binder.Name, out result);
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			_changedMembers[binder.Name] = value;

			return true;
		}

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
		{
			var propertyName = String.Format("{0}", indexes.First());

			return _changedMembers.TryGetValue(propertyName, out result);
		}

		public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
		{
			var propertyName = String.Format("{0}", indexes.First());

			_changedMembers[propertyName] = value;

			return true;
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return _changedMembers.Keys;
		}

		public bool HasChanges { get { return (_changedMembers.Count > 0); } }

		public bool HasChangeFor<TProperty>(Expression<Func<T, TProperty>> property)
		{
			var propertyExpression = property.Body as MemberExpression;

			if (propertyExpression == null)
			{
				throw new ArgumentException(String.Format("Unable to determine property. '{0}' is not a MemberExpression.", property));
			}

			var propertyName = propertyExpression.Member.Name;

			return HasChangeFor(propertyName);
		}

		private bool HasChangeFor(string propertyName)
		{
			return _changedMembers.ContainsKey(propertyName);
		}

		public void SetChangeFor<TProperty>(Expression<Func<T, TProperty>> property, TProperty value)
		{
			var propertyExpression = property.Body as MemberExpression;

			if (propertyExpression == null)
			{
				throw new ArgumentException(String.Format("Unable to determine property. '{0}' is not a MemberExpression.", property));
			}

			var propertyName = propertyExpression.Member.Name;

			SetChangeFor(propertyName, value);
		}

		private void SetChangeFor(string propertyName, object value)
		{
			_changedMembers[propertyName] = value;
		}

		public TProperty GetChangeFor<TProperty>(Expression<Func<T, TProperty>> property)
		{
			var propertyExpression = property.Body as MemberExpression;

			if (propertyExpression == null)
			{
				throw new ArgumentException(String.Format("Unable to determine property. '{0}' is not a MemberExpression.", property));
			}

			var propertyName = propertyExpression.Member.Name;

			return (TProperty) GetChangeFor(propertyName, typeof (TProperty));
		}

		private object GetChangeFor(string propertyName, Type propertyType)
		{
			object result;

			var conversionType = propertyType;

			if (propertyType.IsEnum)
			{
				conversionType = Enum.GetUnderlyingType(propertyType);
			}

			object value;
			if (_changedMembers.TryGetValue(propertyName, out value))
			{
				if (conversionType.IsInstanceOfType(value))
				{
					result = value;
				}
				else if (typeof (IConvertible).IsAssignableFrom(conversionType))
				{
					result = Convert.ChangeType(value, conversionType);
				}
				else
				{
					throw new TypeConversionException(String.Format("Could not convert Change of type '{0}' to property '{1}' of type '{2}'.", value.GetType(), propertyName, conversionType));
				}
			}
			else
			{
				throw new ChangeNotFoundException(propertyName);
			}

			return result;
		}

		public bool TryGetChangeFor<TProperty>(Expression<Func<T, TProperty>> property, out TProperty result)
		{
			result = default(TProperty);

			bool success = false;

			var propertyExpression = property.Body as MemberExpression;

			if (propertyExpression != null)
			{
				var propertyName = propertyExpression.Member.Name;

				var type = typeof (TProperty);

				object value;
				if (TryGetChangeFor(propertyName, type, out value))
				{
					if ((type.IsValueType == false) || (value != null))
					{
						result = (TProperty)value;
						success = true;
					}
				}
			}

			return success;
		}

		private bool TryGetChangeFor(string propertyName, Type propertyType, out object result)
		{
			result = null;

			var success = false;

			var conversionType = propertyType;

			if (propertyType.IsEnum)
			{
				conversionType = Enum.GetUnderlyingType(propertyType);
			}

			object value;
			if (_changedMembers.TryGetValue(propertyName, out value))
			{
				result = Convert.ChangeType(value, conversionType);
				success = true;
			}

			return success;
		}

		public void ApplyChanges(ref T obj)
		{
			if (HasChanges)
			{
				foreach (var property in Properties)
				{
					var propertyName = property.Name;
					var propertyType = property.PropertyType;

					if (HasChangeFor(propertyName))
					{
						var changedValue = GetChangeFor(propertyName, propertyType);

						property.SetValue(obj, changedValue, new object[0]);
					}
				}
			}
		}
	}
}
