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

	/// <summary>
	/// Create a new <see cref="FileView"/> instance.
	/// </summary>
	public FileView() : base()
	{
		list = new VariablesColumnView();
		ScrolledWindow scroller = new ScrolledWindow();
		scroller.Child = list;
		scroller.PropagateNaturalHeight = true;
		scroller.PropagateNaturalWidth = true;
		Append(scroller);
	}

	/// <inheritdoc />
	public void AddVariable(NCVariable variable)
	{
		list.AddVariable(variable);
	}

	/// <inheritdoc />
	public Widget GetWidget() => this;

	private class VariableWrapper : GObject.Object
	{
		public NCVariable Data { get; private init; }
		public VariableWrapper(NCVariable data) : base(true, Array.Empty<GObject.ConstructArgument>())
		{
			Data = data;
		}
	}

	private class VariableRow : ListViewWidget<VariableWrapper>
	{
		private readonly Label nameLabel;
		private readonly Label longNameLabel;

		public VariableRow()
		{
			Spacing = 6;
			MarginTop = MarginBottom = MarginStart = MarginEnd = 10;
			SetOrientation(Orientation.Vertical);

			nameLabel = CreateLabel();
			longNameLabel = CreateLabel();

			Append(nameLabel);
			Append(longNameLabel);
		}

		private Label CreateLabel()
		{
			Label label = new Label();
			label.Halign = Align.Start;
			label.Hexpand = true;
			label.Ellipsize = Pango.EllipsizeMode.End;
			return label;
		}

		/// <inheritdoc />
		public override void Update(VariableWrapper datum)
		{
			nameLabel.SetText(datum.Data.Name);
			longNameLabel.SetText(datum.Data.LongName ?? "--");
		}
	}
}
