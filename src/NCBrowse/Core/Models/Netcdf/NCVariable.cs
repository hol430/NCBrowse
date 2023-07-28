namespace NCBrowse.Core.Models.Netcdf;

/// <summary>
/// A variable in a netcdf file.
/// </summary>
public class NCVariable
{
	/// <summary>
	/// Name of the variable.
	/// </summary>
	public string Name { get; private init; }

	/// <summary>
	/// Type of the variable.
	/// </summary>
	public string DataType { get; private init; }

	/// <summary>
	/// Long name of the variable, or null if it doesn't have one.
	/// </summary>
	public string? LongName { get; private init; }

	/// <summary>
	/// Dimensions of this variable.
	/// </summary>
	public IEnumerable<string> Dimensions { get; private init; }

	/// <summary>
	/// Attributes of this variabe.
	/// </summary>
	public IEnumerable<NCAttribute> Attributes { get; private init; }

	/// <summary>
	/// Create a new <see cref="NCVariable"/> instance.
	/// </summary>
	/// <param name="name">Name of the variable.</param>
	/// <param name="dataType">Type of the variable.</param>
	/// <param name="dimensions">Dimensions of this variable.</param>
	/// <param name="longName">Long name of the variablel, or null if it doesn't have one.</param>
	/// <param name="attributes">Attributes of this variable.</param>
	public NCVariable(string name, string dataType, IEnumerable<string> dimensions, string? longName, IEnumerable<NCAttribute> attributes)
	{
		Name = name;
		DataType = dataType;
		Dimensions = dimensions;
		LongName = longName;
		Attributes = attributes;
	}
}
