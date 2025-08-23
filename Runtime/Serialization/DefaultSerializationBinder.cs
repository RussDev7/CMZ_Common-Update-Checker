using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace DNA.Runtime.Serialization
{
	public class DefaultSerializationBinder : SerializationBinder
	{
		public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
		{
			assemblyName = serializedType.Assembly.FullName;
			typeName = serializedType.FullName;
		}

		public override Type BindToType(string assemblyName, string typeName)
		{
			string newTypeName;
			if (this.TypeNameReplacements.TryGetValue(typeName, out newTypeName))
			{
				typeName = newTypeName;
			}
			Type t = null;
			string strongTypeName = Assembly.CreateQualifiedName(assemblyName, typeName);
			try
			{
				t = Type.GetType(strongTypeName);
			}
			catch
			{
				t = Type.GetType(typeName);
			}
			Type newType;
			if (this.TypeReplacements.TryGetValue(t, out newType))
			{
				t = newType;
			}
			if (t == null)
			{
				throw new SerializationException(string.Concat(new string[] { "Type ", typeName, " ", assemblyName, " Not Found" }));
			}
			return t;
		}

		public Dictionary<Type, Type> TypeReplacements = new Dictionary<Type, Type>();

		public Dictionary<string, string> TypeNameReplacements = new Dictionary<string, string>();
	}
}
