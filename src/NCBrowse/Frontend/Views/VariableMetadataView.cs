using Gtk;
using NCBrowse.Core.Models.Netcdf;
using NCBrowse.Frontend.Extensions;

namespace NCBrowse.Frontend.Views;

// This exists as a wrapper around ColumnView until we can write a generic one.
// https://github.com/gircore/gir.core/issues/909
public class VariableMetadataView : Box
{
	private readonly StringColumnViewFactory<NCAttribute> factory;

	public VariableMetadataView() : base()
	{
		// this.SetMargins(10);
		SetOrientation(Orientation.Vertical);

		factory = new StringColumnViewFactory<NCAttribute>();
		factory.AddColumn("Name", a => a.Name);
		factory.AddColumn("Value", a => a.Value);

		Append(factory.View);
	}

	public void Update(NCVariable variable)
	{
		Clear();
		foreach (NCAttribute attribute in variable.Attributes.OrderBy(a => a.Name))
			AddAttribute(attribute);
	}

	private void AddAttribute(NCAttribute attribute)
	{
		factory.AddRow(attribute);
	}

	private Label CreateLabel(string text)
	{
		Label label = Label.New(text);
		return label;
	}

	private void Clear()
	{
		factory.Clear();
	}
}
