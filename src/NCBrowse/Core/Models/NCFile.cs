using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;
using NCBrowse.Core.Interfaces;
using NCBrowse.Core.Models.Netcdf;

namespace NCBrowse.Core.Models;

public class NCFile : INCFile
{
	/// <summary>
	/// The long name attribute as specified by the CF convention.
	/// </summary>
	private const string attribLongName = "long_name";

	private readonly DataSet dataset;

	public NCFile(string path)
	{
		// Ensure file exists.
		if (!System.IO.File.Exists(path))
			throw new FileNotFoundException($"File not found: '${path}'", path);

		dataset = DataSet.Open(path, ResourceOpenMode.Open);
	}

	/// <inheritdoc />
	public IEnumerable<NCVariable> GetVariables()
	{
		foreach (Variable variable in dataset.Variables)
		{
			string? longName = variable.Metadata[attribLongName] as string;
			IEnumerable<NCAttribute> attributes = GetAttributes(variable);
			yield return new NCVariable(
				variable.Name,
				variable.TypeOfData.Name,
				variable.Dimensions.Select(d => d.Name),
				longName,
				attributes);
		}
	}

	private IEnumerable<NCAttribute> GetAttributes(Variable variable)
	{
		foreach ( (string key, object value) in variable.Metadata)
		{
			string? valueString = value.ToString();
			if (valueString != null)
				yield return new NCAttribute(key, valueString);
			// todo: else warning
		}
	}

	/// <summary>
	/// Dispose of unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		string path = dataset.URI;
		dataset.Dispose();
	}
}
