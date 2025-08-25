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

		public static string GetCompanyName(this Assembly assembly)
		{
			object[] objects = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
			if (objects.Length == 0)
			{
				return "My Company Name Here";
			}
			AssemblyCompanyAttribute attrib = (AssemblyCompanyAttribute)objects[0];
			if (attrib.Company == null || attrib.Company == "")
			{
				return "My Company Name Here";
			}
			return attrib.Company;
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
			object[] attribs = type.GetCustomAttributes(typeof(T), inherit);
			if (attribs.Length == 0)
			{
				return default(T);
			}
			return (T)((object)attribs[0]);
		}

		public static Assembly[] GetAssemblies()
		{
			Assembly[] ret = new Assembly[ReflectionTools._assemblies.Count];
			ReflectionTools._assemblies.Keys.CopyTo(ret, 0);
			return ret;
		}

		public static void RegisterAssembly(Assembly callingAssembly, Assembly assembly)
		{
			Dictionary<Assembly, int> referencedAssemblies = null;
			if (!ReflectionTools._assemblies.TryGetValue(callingAssembly, out referencedAssemblies))
			{
				referencedAssemblies = new Dictionary<Assembly, int>();
				ReflectionTools._assemblies[callingAssembly] = referencedAssemblies;
			}
			referencedAssemblies[assembly] = 0;
			if (!ReflectionTools._assemblies.TryGetValue(assembly, out referencedAssemblies))
			{
				referencedAssemblies = new Dictionary<Assembly, int>();
				ReflectionTools._assemblies[assembly] = referencedAssemblies;
			}
		}

		public static bool ImplementsInterface(this Type type, Type interfaceType)
		{
			Type[] interfaces = type.GetInterfaces();
			foreach (Type t in interfaces)
			{
				if (t == interfaceType)
				{
					return true;
				}
			}
			return false;
		}

		public static Type[] GetTypes(Filter<Type> filter)
		{
			Dictionary<string, Type> typeList = new Dictionary<string, Type>();
			Assembly[] assemblies = ReflectionTools.GetAssemblies();
			int i = 0;
			while (i < assemblies.Length)
			{
				Assembly assembly = assemblies[i];
				Type[] types = null;
				try
				{
					types = assembly.GetTypes();
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
				foreach (Type t in types)
				{
					if (filter == null || filter(t))
					{
						Type foundType;
						if (typeList.TryGetValue(t.FullName, out foundType))
						{
							if (t.Assembly.GetName().Version > foundType.Assembly.GetName().Version)
							{
								typeList[t.FullName] = t;
							}
						}
						else
						{
							typeList[t.FullName] = t;
						}
					}
				}
				goto IL_00AB;
			}
			Type[] typesOut = new Type[typeList.Values.Count];
			typeList.Values.CopyTo(typesOut, 0);
			Array.Sort<Type>(typesOut, new Comparison<Type>(ReflectionTools.TypeNameComparison));
			return typesOut;
		}

		public static Type[] GetTypes()
		{
			return ReflectionTools.GetTypes(null);
		}

		private static Dictionary<Assembly, Dictionary<Assembly, int>> _assemblies = new Dictionary<Assembly, Dictionary<Assembly, int>>();
	}
}
