using NCBrowse.Core.Models.Netcdf;

namespace NCBrowse.Core.Interfaces;

public interface INCFile : IDisposable
{
	/// <summary>
	/// Enumerate all variables in the NetCDF file.
	/// </summary>
	IEnumerable<NCVariable> GetVariables();
}
