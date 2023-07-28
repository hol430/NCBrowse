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
}