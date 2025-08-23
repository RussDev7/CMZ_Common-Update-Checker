using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using DNA.IO;
using DNA.Reflection;

namespace DNA.Runtime.Serialization.Formatters.HTF
{
	public class HTFFormatter : IFormatter
	{
		static HTFFormatter()
		{
			HTFFormatter.PrimitiveLookup[typeof(bool)] = "bool";
			HTFFormatter.PrimitiveLookup[typeof(byte)] = "byte";
			HTFFormatter.PrimitiveLookup[typeof(sbyte)] = "sbyte";
			HTFFormatter.PrimitiveLookup[typeof(short)] = "short";
			HTFFormatter.PrimitiveLookup[typeof(ushort)] = "ushort";
			HTFFormatter.PrimitiveLookup[typeof(int)] = "int";
			HTFFormatter.PrimitiveLookup[typeof(uint)] = "uint";
			HTFFormatter.PrimitiveLookup[typeof(long)] = "long";
			HTFFormatter.PrimitiveLookup[typeof(ulong)] = "ulong";
			HTFFormatter.PrimitiveLookup[typeof(char)] = "char";
			HTFFormatter.PrimitiveLookup[typeof(double)] = "double";
			HTFFormatter.PrimitiveLookup[typeof(float)] = "float";
			HTFFormatter.PrimitiveLookup[typeof(string)] = "string";
		}

		public HTFDocument Serialize(object graph)
		{
			this._nextID = 0;
			this._objectLookup = new Dictionary<object, int>();
			this._objectIDLookup = new Dictionary<int, object>();
			HTFDocument doc = new HTFDocument();
			doc.Root.Children.Add(new HTFElement("DocType", "HTFGraph"));
			doc.Root.Children.Add(new HTFElement("Version", HTFFormatter.Version.ToString()));
			HTFElement childrenElement = new HTFElement();
			childrenElement.ID = "Graph";
			doc.Root.Children.Add(childrenElement);
			this.Serialize(childrenElement, graph);
			return doc;
		}

		public void Serialize(Stream serializationStream, object graph)
		{
			HTFDocument doc = this.Serialize(graph);
			doc.Save(serializationStream);
		}

		private void SerializeArray(HTFElement parentElement, Array graph)
		{
			parentElement.Value = "Array";
			Type graphType = graph.GetType();
			int rank = graphType.GetArrayRank();
			Type elementType = graphType.GetElementType();
			string elementAssemblyName;
			string typeName;
			this.Binder.BindToName(elementType, out elementAssemblyName, out typeName);
			parentElement.Children.Add(new HTFElement("ElementTypeAssembly", elementAssemblyName));
			parentElement.Children.Add(new HTFElement("ElementTypeName", typeName));
			HTFElement lengthsElement = new HTFElement();
			lengthsElement.ID = "Lengths";
			parentElement.Children.Add(lengthsElement);
			int[] lengths = new int[rank];
			for (int i = 0; i < rank; i++)
			{
				int length = graph.GetLength(i);
				lengths[i] = length;
				lengthsElement.Children.Add(new HTFElement(length.ToString()));
			}
			int[] indices = new int[rank];
			HTFElement valuesElement = new HTFElement();
			valuesElement.ID = "Values";
			parentElement.Children.Add(valuesElement);
			this.SerializeArrayRank(valuesElement, graph, lengths, indices, 0);
		}

		private void SerializeArrayRank(HTFElement element, Array graph, int[] lengths, int[] indices, int rank)
		{
			if (rank == lengths.Length - 1)
			{
				for (int i = 0; i < lengths[rank]; i++)
				{
					indices[rank] = i;
					HTFElement valueElement = new HTFElement();
					element.Children.Add(valueElement);
					this.Serialize(valueElement, graph.GetValue(indices));
				}
				return;
			}
			for (int j = 0; j < lengths[rank]; j++)
			{
				HTFElement rankElement = new HTFElement();
				element.Children.Add(rankElement);
				indices[rank] = j;
				this.SerializeArrayRank(rankElement, graph, lengths, indices, rank + 1);
			}
		}

		private void SerializeObject(HTFElement element, object graph)
		{
			element.Value = "Class";
			Type graphType = graph.GetType();
			this.SerializeObjectData(element, graph, graphType);
		}

		private void SerializeObjectData(HTFElement element, object graph, Type graphType)
		{
			string typeAssembly;
			string typeName;
			this.Binder.BindToName(graphType, out typeAssembly, out typeName);
			element.Children.Add(new HTFElement("TypeAssembly", typeAssembly));
			element.Children.Add(new HTFElement("TypeName", typeName));
			if (graph is ISerializable)
			{
				ISerializable iserial = (ISerializable)graph;
				SerializationInfo seriaInfo = new SerializationInfo(graphType, new HTFFormatter.FormatConverter());
				iserial.GetObjectData(seriaInfo, this.Context);
				SerializationInfoEnumerator enumerator = seriaInfo.GetEnumerator();
				HTFElement fieldsElement = new HTFElement("ClassData");
				element.Children.Add(fieldsElement);
				while (enumerator.MoveNext())
				{
					HTFElement fieldElement = new HTFElement();
					fieldElement.ID = enumerator.Name;
					fieldsElement.Children.Add(fieldElement);
					this.Serialize(fieldElement, enumerator.Value);
				}
				return;
			}
			HTFElement fieldsElement2 = new HTFElement("Fields");
			Type currentType = graphType;
			do
			{
				HTFElement typeElement = new HTFElement(currentType.FullName);
				FieldInfo[] fields = currentType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				bool saveField = false;
				foreach (FieldInfo field in fields)
				{
					if (field.GetCustomAttributes(typeof(NonSerializedAttribute), true).Length == 0)
					{
						HTFElement fieldElement2 = new HTFElement();
						fieldElement2.ID = field.Name;
						typeElement.Children.Add(fieldElement2);
						object data = field.GetValue(graph);
						this.Serialize(fieldElement2, data);
						saveField = true;
					}
				}
				if (saveField)
				{
					fieldsElement2.Children.Add(typeElement);
				}
				currentType = currentType.BaseType;
			}
			while (currentType != typeof(object));
			element.Children.Add(fieldsElement2);
		}

		public void Serialize(HTFElement element, object graph)
		{
			if (graph == null)
			{
				element.Value = "Null";
				return;
			}
			Type graphType = graph.GetType();
			string primitiveName;
			if (HTFFormatter.PrimitiveLookup.TryGetValue(graphType, out primitiveName))
			{
				element.Value = primitiveName;
				element.Children.Add(new HTFElement(graph.ToString()));
				return;
			}
			int id;
			if (!graphType.IsValueType && this._objectLookup.TryGetValue(graph, out id))
			{
				element.Value = "Reference";
				element.Children.Add(new HTFElement(id.ToString()));
				return;
			}
			if (!graphType.IsValueType)
			{
				this._objectLookup[graph] = this._nextID++;
			}
			if (graphType.IsArray)
			{
				this.SerializeArray(element, (Array)graph);
				return;
			}
			this.SerializeObject(element, graph);
		}

		public object Deserialize(HTFElement element)
		{
			this._nextID = 0;
			this._objectIDLookup = new Dictionary<int, object>();
			this._objectLookup = new Dictionary<object, int>();
			if (element.Children[0].ID != "DocType" || element.Children[0].Value != "HTFGraph")
			{
				throw new FileLoadException("Not a HTF Graph");
			}
			if (element.Children[1].ID != "Version")
			{
				throw new FileLoadException("Not a HTF Graph Format Error");
			}
			int version = int.Parse(element.Children[1].Value);
			if (version > HTFFormatter.Version)
			{
				throw new FileLoadException("This reader cannot read this version " + version.ToString());
			}
			return this.DeserializeFrom(element.Children[2]);
		}

		public object Deserialize(Stream serializationStream)
		{
			HTFDocument doc = new HTFDocument();
			doc.Load(serializationStream);
			return this.Deserialize(doc.Root);
		}

		private object DeserializeFrom(HTFElement element)
		{
			string value;
			if ((value = element.Value) != null)
			{
				if (value == "Null")
				{
					return null;
				}
				if (value == "Reference")
				{
					return this.DeserializeReference(element);
				}
				if (value == "Class")
				{
					return this.DeserializeClass(element);
				}
				if (value == "Array")
				{
					return this.DeserializeArray(element);
				}
			}
			return HTFFormatter.DeserializePrimitive(element);
		}

		private static object DeserializePrimitive(HTFElement element)
		{
			HTFElement htfelement = element.Children[0];
			string typeString = element.Value;
			string data = element.Children[0].Value;
			string text;
			switch (text = typeString)
			{
			case "bool":
				return bool.Parse(data);
			case "byte":
				return byte.Parse(data);
			case "sbyte":
				return sbyte.Parse(data);
			case "short":
				return short.Parse(data);
			case "ushort":
				return ushort.Parse(data);
			case "int":
				return int.Parse(data);
			case "uint":
				return uint.Parse(data);
			case "long":
				return long.Parse(data);
			case "ulong":
				return ulong.Parse(data);
			case "char":
				return char.Parse(data);
			case "double":
				return double.Parse(data);
			case "float":
				return float.Parse(data);
			case "string":
				return data;
			}
			throw new FileLoadException("Unknown Primitive Type: " + typeString);
		}

		private object DeserializeReference(HTFElement element)
		{
			string idString = element.Children[0].Value;
			int id = int.Parse(idString);
			return this._objectIDLookup[id];
		}

		private object DeserializeClass(HTFElement element)
		{
			string typeAssemblyName = element.Children[0].Value;
			string typeName = element.Children[1].Value;
			Type graphType = this.Binder.BindToType(typeAssemblyName, typeName);
			object graph = null;
			if (graphType.ImplementsInterface(typeof(ISerializable)))
			{
				ConstructorInfo constructor = graphType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[]
				{
					typeof(SerializationInfo),
					typeof(StreamingContext)
				}, null);
				SerializationInfo seriaInfo = new SerializationInfo(graphType, new HTFFormatter.FormatConverter());
				HTFElement fieldsElement = element.Children[2];
				int oldID = 0;
				if (!graphType.IsValueType)
				{
					oldID = this._nextID++;
				}
				foreach (HTFElement fieldElement in fieldsElement.Children)
				{
					object fieldValue = this.DeserializeFrom(fieldElement);
					seriaInfo.AddValue(fieldElement.ID, fieldValue, fieldValue.GetType());
				}
				graph = constructor.Invoke(new object[] { seriaInfo, this.Context });
				if (!graphType.IsValueType)
				{
					this._objectIDLookup[oldID] = graph;
				}
			}
			else
			{
				bool initalized = false;
				try
				{
					graph = Activator.CreateInstance(graphType);
					initalized = true;
				}
				catch
				{
					graph = FormatterServices.GetSafeUninitializedObject(graphType);
				}
				if (!graphType.IsValueType)
				{
					this._objectIDLookup[this._nextID++] = graph;
				}
				bool lazy = this.DeserializeClassData(element, graph, graphType);
				if (lazy && !initalized)
				{
					throw new FileLoadException("Cannot Default Values to a class without a default constructor");
				}
			}
			if (graph is IDeserializationCallback)
			{
				IDeserializationCallback idc = (IDeserializationCallback)graph;
				idc.OnDeserialization(null);
			}
			return graph;
		}

		private bool DeserializeClassData(HTFElement element, object graph, Type graphType)
		{
			bool lazy = false;
			HTFElement fieldsElement = element.Children[2];
			if (fieldsElement.Value != "Fields")
			{
				throw new FileLoadException("HTF Graph Format Error");
			}
			Dictionary<string, List<FieldInfo>> fields = this.GetFields(graphType);
			foreach (HTFElement typeElement in fieldsElement.Children)
			{
				foreach (HTFElement fieldElement in typeElement.Children)
				{
					string fieldName = fieldElement.ID;
					bool found = false;
					List<FieldInfo> localFields;
					if (fields.TryGetValue(fieldName, out localFields))
					{
						object value = this.DeserializeFrom(fieldElement);
						for (int i = 0; i < localFields.Count; i++)
						{
							if (localFields[i].FieldType == value.GetType())
							{
								localFields[i].SetValue(graph, value);
								localFields.RemoveAt(i);
								found = true;
								break;
							}
						}
					}
					if (!found)
					{
						lazy = true;
					}
				}
			}
			return lazy;
		}

		private Dictionary<string, List<FieldInfo>> GetFields(Type graphType)
		{
			Dictionary<string, List<FieldInfo>> result = new Dictionary<string, List<FieldInfo>>();
			Type currentType = graphType;
			do
			{
				FieldInfo[] fields = currentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo field in fields)
				{
					if (field.GetCustomAttributes(typeof(NonSerializedAttribute), true).Length == 0)
					{
						List<FieldInfo> fieldList;
						if (!result.TryGetValue(field.Name, out fieldList))
						{
							fieldList = new List<FieldInfo>();
							result[field.Name] = fieldList;
						}
						fieldList.Add(field);
					}
				}
				currentType = currentType.BaseType;
			}
			while (currentType != typeof(object));
			return result;
		}

		private Array DeserializeArray(HTFElement element)
		{
			string elementTypeAssembly = element.Children[0].Value;
			string elementTypeName = element.Children[1].Value;
			Type elementType = this.Binder.BindToType(elementTypeAssembly, elementTypeName);
			HTFElement rankElement = element.Children[2];
			int rank = rankElement.Children.Count;
			int[] lengths = new int[rank];
			for (int i = 0; i < rank; i++)
			{
				lengths[i] = int.Parse(rankElement.Children[i].Value);
			}
			HTFElement valuesElement = element.Children[3];
			Array graph = Array.CreateInstance(elementType, lengths);
			this._objectIDLookup[this._nextID++] = graph;
			this.DeserializeArrayRank(valuesElement, graph, lengths, new int[rank], 0);
			return graph;
		}

		private void DeserializeArrayRank(HTFElement element, Array array, int[] lengths, int[] indicies, int rank)
		{
			if (rank == lengths.Length - 1)
			{
				for (int i = 0; i < lengths[rank]; i++)
				{
					HTFElement childElement = element.Children[i];
					indicies[rank] = i;
					object data = this.DeserializeFrom(childElement);
					array.SetValue(data, indicies);
				}
				return;
			}
			for (int j = 0; j < lengths[rank]; j++)
			{
				HTFElement childElement2 = element.Children[j];
				indicies[rank] = j;
				this.DeserializeArrayRank(childElement2, array, lengths, indicies, rank + 1);
			}
		}

		public SerializationBinder Binder
		{
			get
			{
				return this._binder;
			}
			set
			{
				this._binder = value;
			}
		}

		public StreamingContext Context
		{
			get
			{
				return this._context;
			}
			set
			{
				this._context = value;
			}
		}

		public ISurrogateSelector SurrogateSelector
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		private static Dictionary<Type, string> PrimitiveLookup = new Dictionary<Type, string>();

		private Dictionary<object, int> _objectLookup;

		private Dictionary<int, object> _objectIDLookup;

		private static int Version = 1;

		private int _nextID;

		private SerializationBinder _binder = new DefaultSerializationBinder();

		private StreamingContext _context = new StreamingContext(StreamingContextStates.All);

		private class FormatConverter : IFormatterConverter
		{
			public object Convert(object value, TypeCode typeCode)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public object Convert(object value, Type type)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public bool ToBoolean(object value)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public byte ToByte(object value)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public char ToChar(object value)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public DateTime ToDateTime(object value)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public decimal ToDecimal(object value)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public double ToDouble(object value)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public short ToInt16(object value)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public int ToInt32(object value)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public long ToInt64(object value)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public sbyte ToSByte(object value)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public float ToSingle(object value)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public string ToString(object value)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public ushort ToUInt16(object value)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public uint ToUInt32(object value)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public ulong ToUInt64(object value)
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}
	}
}
