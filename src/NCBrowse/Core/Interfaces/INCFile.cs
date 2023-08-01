using NCBrowse.Core.Models;
using NCBrowse.Core.Models.Netcdf;

namespace NCBrowse.Core.Interfaces;

public interface INCFile : IDisposable
{
	/// <summary>
	/// Enumerate all variables in the NetCDF file.
	/// </summary>
	IEnumerable<NCVariable> GetVariables();

	/// <summary>
	/// Read a timeseries for a single variable.
	/// </summary>
	/// <param name="variable">The variable.</param>
	IEnumerable<DataPoint<T>> ReadTimeSeries<T>(NCVariable variable);
}
