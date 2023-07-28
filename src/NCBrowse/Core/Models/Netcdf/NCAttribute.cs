namespace NCBrowse.Core.Models.Netcdf;

public class NCAttribute
{
	public string Name { get; private init; }

	public string Value { get; private init; }

	public NCAttribute(string name, string value)
	{
		Name = name;
		Value = value;
	}
}
