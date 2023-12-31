using NCBrowse.Core.Extensions;

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

	public Type DataType { get; private init; }

	/// <summary>
	/// Type of the variable.
	/// </summary>
	public string TypeName => DataType.FriendlyName();

	/// <summary>
	/// Long name of the variable, or null if it doesn't have one.
	/// </summary>
	public string? LongName { get; private init; }

	/// <summary>
	/// Get the units of this variable.
	/// </summary>
	public string? Units => Attributes.FirstOrDefault(a => a.Name.Equals("Units", StringComparison.InvariantCultureIgnoreCase))?.Value;

	/// <summary>
	/// Dimensions of this variable.
	/// </summary>
	public IEnumerable<NCDimension> Dimensions { get; private init; }

	/// <summary>
	/// Attributes of this variabe.
	/// </summary>
	public IEnumerable<NCAttribute> Attributes { get; private init; }

	/// <summary>
	/// The value of all missing data points for this variable.
	/// </summary>
	public object? MissingValue { get; private init; }

	/// <summary>
	/// Create a new <see cref="NCVariable"/> instance.
	/// </summary>
	/// <param name="name">Name of the variable.</param>
	/// <param name="dataType">Type of the variable.</param>
	/// <param name="dimensions">Dimensions of this variable.</param>
	/// <param name="longName">Long name of the variablel, or null if it doesn't have one.</param>
	/// <param name="attributes">Attributes of this variable.</param>
	/// <param name="missingValue">The value given to all missing data points of this variable.</param>
	public NCVariable(string name, Type dataType, IEnumerable<NCDimension> dimensions, string? longName, IEnumerable<NCAttribute> attributes, object? missingValue)
	{
		Name = name;
		DataType = dataType;
		Dimensions = dimensions;
		LongName = longName;
		Attributes = attributes;
		MissingValue = missingValue;
	}
}
