using NCBrowse.Core.Interfaces;
using NCBrowse.Core.Models;
using NCBrowse.Core.Models.Netcdf;
// using NCBrowse.Frontend.Delegates;
// using NCBrowse.Frontend.Enumerations;
using NCBrowse.Frontend.Extensions;
using NCBrowse.Frontend.Interfaces;
using NCBrowse.Frontend.Views;

namespace NCBrowse.Frontend.Presenters;

/// <summary>
/// A presenter for a view which displays a NetCDF file. This presenter handles
/// logic for populating the GUI elements with data from the file.
/// </summary>
public class FilePresenter : IPresenter<IFileView>
{
	/// <summary>
	/// The current workspace metadata.
	/// </summary>
	private readonly INCFile file;

	/// <summary>
	/// The view object.
	/// </summary>
	private readonly IFileView view;

	/// <summary>
	/// Create a new <see cref="FilePresenter"/> instance for the given file.
	/// </summary>
	/// <param name="file">The instruction file.</param>
	public FilePresenter(INCFile file)
	{
		this.file = file;
		view = new FileView();
		Populate();
	}

	/// <summary>
	/// Populate the view object.
	/// </summary>
	private void Populate()
	{
		foreach (NCVariable variable in file.GetVariables().OrderBy(v => v.Name))
			view.AddVariable(variable);
	}

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public void Dispose()
	{
		view.Dispose();
		file.Dispose();
	}

	/// <inheritdoc />
	public IFileView GetView() => view;
}
