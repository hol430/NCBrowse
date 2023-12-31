using NCBrowse.Core.Models;
using NCBrowse.Frontend.Interfaces;
using NCBrowse.Frontend.Views;

namespace NCBrowse.Frontend.Presenters;

/// <summary>
/// The main presenter type. This doesn't implement IPresenter or PresenterBase,
/// because it doesn't have a model object, so it's not quite a presenter in
/// that sense.
/// </summary>
public class MainPresenter
{
	/// <summary>
	/// Default window title when no files are open.
	/// </summary>
	private const string defaultTitle = "NCBrowse (todo: better name)";

	/// <summary>
	/// The view object.
	/// </summary>
	private readonly IMainView view;

	/// <summary>
	/// The current child presenter.
	/// </summary>
	private IPresenter<IView> child;

	/// <summary>
	/// Presenter for the preferences dialog.
	/// </summary>
	// private PreferencesPresenter? propertiesPresenter;

	/// <summary>
	/// Create a new <see cref="MainPresenter"/> instance connected to the
	/// specified view object.
	/// </summary>
	/// <param name="view"></param>
	public MainPresenter(IMainView view)
	{
		this.view = view;
		view.AddMenuItem("Preferences", OnPreferences);
		view.AddMenuItem("About", OnAbout);
		view.AddMenuItem("Quit", () => view.Close(), "<Ctrl>Q");
		view.OpenFile += OpenFile;

		child = new FallbackPresenter<NoFileView>();
		view.SetChild(child.GetView());

		view.SetTitle(defaultTitle);
	}

	/// <summary>
	/// Open the specified file.
	/// </summary>
	/// <param name="path">File path.</param>
	public void OpenFile(string path)
	{
		// Close previous file.
		child.Dispose();

		// Get absolute path if the file exists on the local filesystem.
		if (File.Exists(path))
			path = Path.GetFullPath(path);

		// Open new file.
		NCFile file = new NCFile(path);
		child = new FilePresenter(file);
		view.SetChild(child.GetView());

		// Update window title.
		view.SetTitle(Path.GetFileName(path), Path.GetDirectoryName(path));
	}

	/// <summary>
	/// User has selected the "Preferences" menu item.
	/// </summary>
	private void OnPreferences()
	{
		try
		{
			Console.WriteLine("TBI: preferences");
			// propertiesPresenter = new PreferencesPresenter(Configuration.Instance, OnPreferencesClosed);
			// propertiesPresenter.Show();
		}
		catch (Exception error)
		{
			view.ReportError(error);
		}
	}

	/// <summary>
	/// User has closed the preferences dialog. Save preferences to disk.
	/// </summary>
	private void OnPreferencesClosed()
	{
		// Configuration.Instance.Save();
		// propertiesPresenter?.Dispose();
		// propertiesPresenter = null;

		// if (child is FilePresenter fp)
		// 	fp.PopulateRunners();
	}

	/// <summary>
	/// User has selected the "About" menu item.
	/// </summary>
	private void OnAbout()
	{
		try
		{
			Console.WriteLine("TBI: about");
			// new AboutView(view).Show();
		}
		catch (Exception error)
		{
			view.ReportError(error);
		}
	}
}
