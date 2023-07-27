using Gtk;
using NCBrowse.Frontend.Interfaces;

namespace NCBrowse.Frontend.Views;

/// <summary>
/// A view displayed in the main window when no file is selected.
/// </summary>
public class NoFileView : Label, IView
{
	/// <summary>
	/// Create a new <see cref="NoFileView"/> instance.
	/// </summary>
	public NoFileView() : base()
	{
		SetMarkup("<i>No .nc file selected</i>");
		Xalign = 0.5f;
		Yalign = 0.5f;
		Hexpand = true;
		Vexpand = true;
	}

	/// <inheritdoc />
	public Widget GetWidget() => this;
}
