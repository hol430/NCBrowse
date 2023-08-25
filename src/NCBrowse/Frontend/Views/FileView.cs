using System.Text;
using Gio;
using Gtk;
using NCBrowse.Frontend.Extensions;
using NCBrowse.Frontend.Interfaces;

using File = System.IO.File;
using Action = System.Action;
using NCBrowse.Core.Models.Netcdf;
using NCBrowse.Frontend.Signals;
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
	private readonly Paned panel;

	private readonly Event<NCVariable> onVariableActivated;

	private bool first = true;

	public IEvent<NCVariable> OnVariableActivated => onVariableActivated;

	/// <summary>
	/// Create a new <see cref="FileView"/> instance.
	/// </summary>
	public FileView() : base()
	{
		onVariableActivated = new Event<NCVariable>();

		list = new VariablesColumnView();
		list.Vexpand = true;
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
		metadataFrame.Hexpand = true;

		panel = new Paned();
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
		list.OnVariableActivated.ConnectTo(VariableActivated);
	}

	private void DisconnectEvents()
	{
	}

	/// <inheritdoc />
	public void AddVariable(NCVariable variable)
	{
		list.AddVariable(variable);
		// todo: fix initial panel sizing
		if (first)
		{
			OnVariableSelected(variable);
			first = false;
		}
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
		if (!first)
			panel.PositionSet = true;
		var dim2text = (NCDimension dim) => $"{dim.Name}={dim.Size}";
		string dims = string.Join(", ", variable.Dimensions.Select(dim2text));
		string label = $"{variable.TypeName} {variable.Name} ({dims})";
		metadataHeader.SetText(label);
		metadataHeader.Show();
		metadataView.Update(variable);
	}

	private void VariableActivated(NCVariable variable)
	{
		onVariableActivated.Invoke(variable);
	}
}
