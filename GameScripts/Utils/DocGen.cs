using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using UnityEngine;

public static class DocGen
{
	public static void Go()
	{
		foreach (Type type in typeof(CardData).Assembly.GetTypes())
		{
			if (!DocGen.TypeBlacklist.Contains(type.ToString()) && type.Namespace == null)
			{
				if (type.IsEnum)
				{
					DocGen.DoEnum(type);
				}
				else if ((type.IsClass || type.IsValueType || type.IsInterface) && !type.IsDefined(typeof(CompilerGeneratedAttribute), false))
				{
					DocGen.DoClass(type);
				}
			}
		}
		File.WriteAllText("F:/doctest/doc.xml", DocGen.Doc.ToString());
		Debug.Log("Done!");
	}

	public static void DoEnum(Type type)
	{
		XElement xelement = new XElement("Enum", new XAttribute("Name", type));
		foreach (object obj in Enum.GetValues(type))
		{
			int num = (int)obj;
			xelement.Add(new XElement("Value", new object[]
			{
				new XAttribute("Name", Enum.GetName(type, num)),
				new XAttribute("Value", num),
				new XElement("Description", "")
			}));
		}
		xelement.Add(new XElement("Description", ""));
		DocGen.Doc.Root.Add(xelement);
		File.WriteAllText("F:/doctest/source/_doc/enums/" + type.Name + ".xml", xelement.ToString());
	}

	public static void DoClass(Type type)
	{
		Type type2 = type;
		List<Type> list = new List<Type>();
		while (type2 != typeof(MonoBehaviour) && type2.BaseType != null)
		{
			type2 = type2.BaseType;
			if (type2 != typeof(object))
			{
				list.Add(type2);
			}
		}
		XElement xelement = new XElement("Class", new XAttribute("Name", type));
		if (type.IsValueType)
		{
			xelement.Add(new XAttribute("IsStruct", true));
		}
		if (type.IsInterface)
		{
			xelement.Add(new XAttribute("IsInterface", true));
		}
		if (type.IsAbstract)
		{
			xelement.Add(new XAttribute("IsAbstract", true));
		}
		if (type.IsSealed)
		{
			xelement.Add(new XAttribute("IsSealed", true));
		}
		foreach (Type type3 in list)
		{
			xelement.Add(new XElement("Inherits", new XAttribute("Name", type3)));
		}
		foreach (Type type4 in type.GetTypeInfo().ImplementedInterfaces)
		{
			xelement.Add(new XElement("Implements", new XAttribute("Name", type4)));
		}
		List<MethodInfo> list2 = new List<MethodInfo>();
		XElement xelement2 = new XElement("Properties");
		foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
		{
			list2.AddRange(propertyInfo.GetAccessors(true));
			XElement xelement3 = new XElement("Property", new object[]
			{
				new XAttribute("Name", propertyInfo.Name),
				new XAttribute("Type", propertyInfo.PropertyType)
			});
			MethodInfo getMethod = propertyInfo.GetGetMethod(true);
			MethodInfo setMethod = propertyInfo.GetSetMethod(true);
			if (getMethod != null)
			{
				if (getMethod.IsAbstract && !getMethod.IsFinal)
				{
					xelement3.Add(new XAttribute("IsAbstract", true));
				}
				if (getMethod.IsVirtual && !getMethod.IsFinal)
				{
					xelement3.Add(new XAttribute("IsVirtual", true));
				}
				if (getMethod.IsStatic)
				{
					xelement3.Add(new XAttribute("IsStatic", true));
				}
				if (getMethod.GetBaseDefinition() != getMethod)
				{
					xelement3.Add(new XAttribute("Override", getMethod.GetBaseDefinition().DeclaringType));
				}
				xelement3.Add(new XAttribute("Access", DocGen.GetAccess(getMethod)));
				xelement3.Add(new XAttribute("Accessors", (setMethod != null) ? "get; set" : "get"));
			}
			else
			{
				if (setMethod.IsAbstract && !setMethod.IsFinal)
				{
					xelement3.Add(new XAttribute("IsAbstract", true));
				}
				if (setMethod.IsVirtual && !setMethod.IsFinal)
				{
					xelement3.Add(new XAttribute("IsVirtual", true));
				}
				if (setMethod.IsStatic)
				{
					xelement3.Add(new XAttribute("IsStatic", true));
				}
				if (setMethod.GetBaseDefinition() != setMethod)
				{
					xelement3.Add(new XAttribute("Override", setMethod.GetBaseDefinition().DeclaringType));
				}
				xelement3.Add(new XAttribute("Access", DocGen.GetAccess(setMethod)));
				xelement3.Add(new XAttribute("Accessors", "set"));
			}
			xelement3.Add(new XElement("Description", ""));
			xelement2.Add(xelement3);
		}
		xelement.Add(xelement2);
		XElement xelement4 = new XElement("Fields");
		foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
		{
			if (!fieldInfo.IsDefined(typeof(CompilerGeneratedAttribute), false))
			{
				XElement xelement5 = new XElement("Field", new object[]
				{
					new XAttribute("Name", fieldInfo.Name),
					new XAttribute("Type", fieldInfo.FieldType),
					new XAttribute("Access", DocGen.GetAccess(fieldInfo.FieldType))
				});
				if (fieldInfo.IsStatic)
				{
					xelement5.Add(new XAttribute("IsStatic", true));
				}
				xelement5.Add(new XElement("Description", ""));
				xelement4.Add(xelement5);
			}
		}
		xelement.Add(xelement4);
		XElement xelement6 = new XElement("Methods");
		foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
		{
			if (!methodInfo.IsDefined(typeof(CompilerGeneratedAttribute), false) && !list2.Contains(methodInfo))
			{
				XElement xelement7 = new XElement("Method", new object[]
				{
					new XAttribute("Name", methodInfo.Name),
					new XAttribute("Type", methodInfo.ReturnType),
					new XAttribute("Access", DocGen.GetAccess(methodInfo))
				});
				if (methodInfo.IsAbstract && !methodInfo.IsFinal)
				{
					xelement7.Add(new XAttribute("IsAbstract", true));
				}
				if (methodInfo.IsVirtual && !methodInfo.IsFinal)
				{
					xelement7.Add(new XAttribute("IsVirtual", true));
				}
				if (methodInfo.IsStatic)
				{
					xelement7.Add(new XAttribute("IsStatic", true));
				}
				if (methodInfo.GetBaseDefinition() != methodInfo)
				{
					xelement7.Add(new XAttribute("Override", methodInfo.GetBaseDefinition().DeclaringType));
				}
				if (methodInfo.IsGenericMethod)
				{
					xelement7.Add(new XAttribute("IsGeneric", true));
					XElement xelement8 = new XElement("GenericArguments");
					foreach (Type type5 in methodInfo.GetGenericArguments())
					{
						xelement8.Add(new XElement("Argument", new XAttribute("Type", type5)));
					}
					xelement7.Add(xelement8);
				}
				XElement xelement9 = new XElement("Parameters");
				foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
				{
					XElement xelement10 = new XElement("Parameter", new object[]
					{
						new XAttribute("Name", parameterInfo.Name),
						new XAttribute("Type", parameterInfo.ParameterType)
					});
					if (parameterInfo.HasDefaultValue)
					{
						xelement10.Add(new XAttribute("DefaultValue", (parameterInfo.DefaultValue == null) ? "null" : parameterInfo.DefaultValue.ToString()));
					}
					if (parameterInfo.IsOptional)
					{
						xelement10.Add(new XAttribute("IsOptional", true));
					}
					xelement9.Add(xelement10);
				}
				xelement7.Add(xelement9);
				xelement7.Add(new XElement("Description", ""));
				xelement6.Add(xelement7);
			}
		}
		xelement.Add(xelement6);
		XElement xelement11 = new XElement("Events");
		foreach (EventInfo eventInfo in type.GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
		{
			if (!eventInfo.IsDefined(typeof(CompilerGeneratedAttribute), false))
			{
				XElement xelement12 = new XElement("Event", new object[]
				{
					new XAttribute("Name", eventInfo.Name),
					new XAttribute("Type", eventInfo.EventHandlerType),
					new XAttribute("Access", DocGen.GetAccess(eventInfo.EventHandlerType))
				});
				xelement12.Add(new XElement("Description", ""));
				xelement11.Add(xelement12);
			}
		}
		xelement.Add(xelement11);
		xelement.Add(new XElement("Description", ""));
		DocGen.Doc.Root.Add(xelement);
		File.WriteAllText("F:/doctest/source/_doc/classes/" + type.Name.Replace('[', '(').Replace(']', ')') + ".xml", xelement.ToString());
	}

	public static string GetAccess(MethodInfo method)
	{
		if (method.IsAssembly)
		{
			if (!method.IsFamily)
			{
				return "internal";
			}
			return "internal protected";
		}
		else
		{
			if (method.IsPublic)
			{
				return "public";
			}
			if (method.IsPrivate)
			{
				return "private";
			}
			if (method.IsFamily)
			{
				return "protected";
			}
			return "public";
		}
	}

	public static string GetAccess(Type type)
	{
		if (type.IsPublic || type.IsNestedPublic)
		{
			return "public";
		}
		if (type.IsNestedFamORAssem)
		{
			return "protected internal";
		}
		if (type.IsNestedAssembly)
		{
			return "internal";
		}
		if (type.IsNestedFamily)
		{
			return "protected";
		}
		if (type.IsNestedFamANDAssem)
		{
			return "private protected";
		}
		return "private";
	}

	public const BindingFlags Flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	public static List<string> TypeBlacklist = new List<string> { "<PrivateImplementationDetails>", "<PrivateImplementationDetails>+__StaticArrayInitTypeSize=20" };

	public static XDocument Doc = new XDocument(new object[]
	{
		new XElement("Root")
	});
}
