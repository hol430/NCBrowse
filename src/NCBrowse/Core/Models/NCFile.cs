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
			yield return new NCVariable(
				variable.Name,
				variable.TypeOfData.Name,
				variable.Dimensions.Select(d => d.Name),
				longName);
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
