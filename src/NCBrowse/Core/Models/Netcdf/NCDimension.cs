namespace NCBrowse.Core.Models.Netcdf;

public class NCDimension
{
	public string Name { get; private init; }
	public bool IsUnlimited { get; private init; }
	public int Size { get; private init; }

	public NCDimension(string name, bool isUnlimited, int size)
	{
		Name = name;
		IsUnlimited = isUnlimited;
		Size = size;
	}
}
