using System;
using System.Collections.Generic;
using System.Reflection;
using DNA.Collections;

namespace DNA.Reflection
{
	public static class ReflectionTools
	{
		public static int TypeNameComparison(Type a, Type b)
		{
			return string.Compare(a.FullName, b.FullName);
		}

		public static bool ImplementsInterface(this Type type, Type interfaceType)
		{
			Type[] interfaces = type.GetInterfaces();
			foreach (Type type2 in interfaces)
			{
				if (type2 == interfaceType)
				{
					return true;
				}
			}
			return false;
		}

		public static string GetCompanyName(this Assembly assembly)
		{
			object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
			if (customAttributes.Length == 0)
			{
				return "My Company Name Here";
			}
			AssemblyCompanyAttribute assemblyCompanyAttribute = (AssemblyCompanyAttribute)customAttributes[0];
			if (assemblyCompanyAttribute.Company == null || assemblyCompanyAttribute.Company == "")
			{
				return "My Company Name Here";
			}
			return assemblyCompanyAttribute.Company;
		}

		public static string GetCompanyName(this Type type)
		{
			return type.Assembly.GetCompanyName();
		}

		public static string GetCSharpName(this Type t)
		{
			if (t == typeof(void))
			{
				return "void";
			}
			if (t == typeof(int))
			{
				return "int";
			}
			if (t == typeof(bool))
			{
				return "bool";
			}
			if (t == typeof(byte))
			{
				return "byte";
			}
			if (t == typeof(sbyte))
			{
				return "sbyte";
			}
			if (t == typeof(char))
			{
				return "char";
			}
			if (t == typeof(decimal))
			{
				return "decimal";
			}
			if (t == typeof(float))
			{
				return "float";
			}
			if (t == typeof(uint))
			{
				return "uint";
			}
			if (t == typeof(long))
			{
				return "long";
			}
			if (t == typeof(object))
			{
				return "object";
			}
			if (t == typeof(short))
			{
				return "short";
			}
			if (t == typeof(ushort))
			{
				return "ushort";
			}
			if (t == typeof(string))
			{
				return "string";
			}
			return t.Name;
		}

		public static T GetAttribute<T>(this Type type, bool inherit)
		{
			object[] customAttributes = type.GetCustomAttributes(typeof(T), inherit);
			if (customAttributes.Length == 0)
			{
				return default(T);
			}
			return (T)((object)customAttributes[0]);
		}

		public static Assembly[] GetAssemblies()
		{
			Assembly[] array = new Assembly[ReflectionTools._assemblies.Count];
			ReflectionTools._assemblies.Keys.CopyTo(array, 0);
			return array;
		}

		public static void RegisterAssembly(Assembly callingAssembly, Assembly assembly)
		{
			Dictionary<Assembly, int> dictionary = null;
			if (!ReflectionTools._assemblies.TryGetValue(callingAssembly, out dictionary))
			{
				dictionary = new Dictionary<Assembly, int>();
				ReflectionTools._assemblies[callingAssembly] = dictionary;
			}
			dictionary[assembly] = 0;
			if (!ReflectionTools._assemblies.TryGetValue(assembly, out dictionary))
			{
				dictionary = new Dictionary<Assembly, int>();
				ReflectionTools._assemblies[assembly] = dictionary;
			}
		}

		public static Type[] GetTypes(Filter<Type> filter)
		{
			Dictionary<string, Type> dictionary = new Dictionary<string, Type>();
			Assembly[] assemblies = ReflectionTools.GetAssemblies();
			int i = 0;
			while (i < assemblies.Length)
			{
				Assembly assembly = assemblies[i];
				Type[] array = null;
				try
				{
					array = assembly.GetTypes();
				}
				catch
				{
					goto IL_00AB;
				}
				goto IL_002A;
				IL_00AB:
				i++;
				continue;
				IL_002A:
				foreach (Type type in array)
				{
					if (filter == null || filter(type))
					{
						Type type2;
						if (dictionary.TryGetValue(type.FullName, out type2))
						{
							if (type.Assembly.GetName().Version > type2.Assembly.GetName().Version)
							{
								dictionary[type.FullName] = type;
							}
						}
						else
						{
							dictionary[type.FullName] = type;
						}
					}
				}
				goto IL_00AB;
			}
			Type[] array2 = new Type[dictionary.Values.Count];
			dictionary.Values.CopyTo(array2, 0);
			Array.Sort<Type>(array2, new Comparison<Type>(ReflectionTools.TypeNameComparison));
			return array2;
		}

		public static Type[] GetTypes()
		{
			return ReflectionTools.GetTypes(null);
		}

		private static Dictionary<Assembly, Dictionary<Assembly, int>> _assemblies = new Dictionary<Assembly, Dictionary<Assembly, int>>();
	}
}
