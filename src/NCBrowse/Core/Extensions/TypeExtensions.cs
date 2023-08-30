using System.Reflection;
using System.Reflection.Metadata;

namespace NCBrowse.Core.Extensions;

public static class TypeExtensions
{
	private static readonly IDictionary<Type, string> friendlyNameLookup = new Dictionary<Type, string>()
	{
		{ typeof(short), "short" },
		{ typeof(ushort), "ushort" },
		{ typeof(int), "int" },
		{ typeof(uint), "uint" },
		{ typeof(long), "long" },
		{ typeof(ulong), "ulong" },
		{ typeof(float), "float" },
		{ typeof(double), "double" },
		{ typeof(char), "char" },
		{ typeof(string), "string" },
		{ typeof(byte), "byte" },
	};

	public static string FriendlyName(this Type type)
	{
		if (type.IsGenericType)
		{
			string name = type.Name;
			if (name.Contains("`"))
				name = name.Substring(0, name.IndexOf('`'));
			return $"{name}<{string.Join(", ", type.GenericTypeArguments.Select(t => FriendlyName(t)))}>";
		}
		if (friendlyNameLookup.TryGetValue(type, out string? friendly))
			return friendly;
		return type.Name;
	}

	public static MethodInfo GetGenericMethod(this Type type, string name, BindingFlags flags, params Type[] argumentTypes)
	{
		IEnumerable<MethodInfo> methods = typeof(Enumerable)
			.GetMethods(flags)
			.Where(m => m.Name == name)
			.Where(m => m.GetParameters().Length == argumentTypes.Length);
		foreach (MethodInfo method in methods)
		{
			// Check if parameter types match the expected types.
			IEnumerable<Type> types = method.GetParameters()
				.Select(p => p.ParameterType)
				.Select(t => t.IsGenericType ? t.GetGenericTypeDefinition() : t);
			if (types.Zip(argumentTypes).All(x => x.First == x.Second))
				return method;
		}
		throw new InvalidOperationException($"Unable to get {type.Name}.{name} method. Method with the provided parameters does not exist.");
	}
}