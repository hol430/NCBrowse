// using NCBrowse.Frontend.Delegates;

using NCBrowse.Core.Models.Netcdf;

namespace NCBrowse.Frontend.Interfaces;

/// <summary>
/// An interface for a view which may display a NetCDF file to the user.
/// </summary>
public interface IFileView : IView
{
	/// <summary>
	/// Add a variable to the view.
	/// </summary>
	void AddVariable(NCVariable variable);
}
