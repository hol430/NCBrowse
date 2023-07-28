using System.Text;
using Gio;
using Gtk;
using NCBrowse.Frontend.Extensions;
using NCBrowse.Frontend.Interfaces;

using File = System.IO.File;
using Action = System.Action;
using NCBrowse.Core.Models.Netcdf;
// using NCBrowse.Frontend.Delegates;
// using NCBrowse.Frontend.Enumerations;

namespace NCBrowse.Frontend.Views;

/// <summary>
/// A view which displays the contents of a NetCDF file to the user.
/// </summary>
public class FileView : Box, IFileView
{
	/// <summary>
	/// Spacing between internal widgets (in px).
	/// </summary>
	private const int spacing = 5;

	/// <summary>
	/// Domain for file-specific actions.
	/// </summary>
	private const string actionDomain = "file";

	private readonly VariablesColumnView list;

	private readonly VariableMetadataView metadataView;

	private readonly Label metadataHeader;

	/// <summary>
	/// Create a new <see cref="FileView"/> instance.
	/// </summary>
	public FileView() : base()
	{
		list = new VariablesColumnView();
		ScrolledWindow variablesScroller = GtkExtensions.CreateExpandingScrolledWindow();
		variablesScroller.Child = list;

		Box variablesBox = new Box();
		variablesBox.SetOrientation(Orientation.Vertical);
		Label variablesHeader = CreateHeaderLabel("Variables");
		variablesBox.Append(variablesHeader);
		variablesBox.Append(variablesScroller);
		Frame variablesFrame = CreateFrame(variablesBox);
		variablesFrame.Hexpand = false;

		metadataView = new VariableMetadataView();
		ScrolledWindow metadataScroller = GtkExtensions.CreateExpandingScrolledWindow();
		metadataScroller.Child = metadataView;

		Box metadataBox = new Box();
		metadataBox.SetOrientation(Orientation.Vertical);
		metadataHeader = CreateHeaderLabel("Metadata");
		metadataBox.Append(metadataHeader);
		metadataBox.Append(metadataScroller);
		metadataHeader.Hide();

		Frame metadataFrame = CreateFrame(metadataBox);

		Paned panel = new Paned();
		panel.WideHandle = true;
		panel.SetOrientation(Orientation.Horizontal);
		panel.StartChild = variablesFrame;
		panel.EndChild = metadataFrame;
		Append(panel);

		ConnectEvents();
	}

	private Frame CreateFrame(Widget child)
	{
		Frame frame = new Frame();
		frame.Child = child;
		frame.Hexpand = true;
		return frame;
	}

	private Label CreateHeaderLabel(string text)
	{
		Label label = Label.New(text);
		label.AddCssClass(StyleClasses.Heading);
		label.Halign = Align.Start;
		label.SetMargins(10);
		return label;
	}

	private void ConnectEvents()
	{
		list.OnSelectionChanged.ConnectTo(OnVariableSelected);
	}

	private void DisconnectEvents()
	{
	}

	/// <inheritdoc />
	public void AddVariable(NCVariable variable)
	{
		list.AddVariable(variable);
	}

	/// <inheritdoc />
	public Widget GetWidget() => this;

	public override void Dispose()
	{
		DisconnectEvents();
		base.Dispose();
	}

	private void OnVariableSelected(NCVariable variable)
	{
		metadataHeader.SetText($"{variable.Name} metadata");
		metadataHeader.Show();
		metadataView.Update(variable);
	}

	private class VariableWrapper : GObject.Object
	{
		public NCVariable Data { get; private init; }
		public VariableWrapper(NCVariable data) : base(true, Array.Empty<GObject.ConstructArgument>())
		{
			Data = data;
		}
	}
}
