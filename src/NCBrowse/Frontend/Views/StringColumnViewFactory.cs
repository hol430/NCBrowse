using Gtk;

namespace NCBrowse.Frontend.Views;

// This exists as a wrapper around ColumnView until we can write a generic one.
// https://github.com/gircore/gir.core/issues/909
public class StringColumnViewFactory<TData> where TData : class
{
	private readonly Gio.ListStore model;
	private readonly SingleSelection selection;

	public ColumnView View { get; private init; }

	public StringColumnViewFactory()
	{
		model = Gio.ListStore.New(Wrapper.GetGType());
		selection = SingleSelection.New(model);

		View = new ColumnView();
		View.Model = selection;
	}

	public void AddColumn(string name, Func<TData, string> renderer)
	{
		SignalListItemFactory factory = SignalListItemFactory.New();
		factory.OnSetup += OnSetupLabelColumn;
		factory.OnBind += (_, args) => HandleBind<Label>(args, (l, row) => l.SetText(renderer(row)));
		ColumnViewColumn column = CreateColumn(name, factory);
		View.AppendColumn(column);
	}

	private ColumnViewColumn CreateColumn(string name, SignalListItemFactory factory)
	{
		ColumnViewColumn column = ColumnViewColumn.New(name, factory);
		return column;
	}

	public void Populate(IEnumerable<TData> rows)
	{
		foreach (TData row in rows)
			AddRow(row);
	}

	public void AddRow(TData row)
	{
		model.Append(new Wrapper(row));
	}

	public void Clear()
	{
		model.RemoveAll();
	}

	private void OnSetupLabelColumn(SignalListItemFactory sender, SignalListItemFactory.SetupSignalArgs args)
	{
		try
		{
			ListItem item = (ListItem)args.Object;
			item.SetChild(CreateLabel());
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	private Label CreateLabel()
	{
		return new Label()
		{
			Halign = Align.Start
		};
	}

	private void HandleBind<TWidget>(SignalListItemFactory.BindSignalArgs args, Action<TWidget, TData> bind)
		where TWidget : Widget
	{
		ListItem item = (ListItem)args.Object;
		Wrapper? wrapper = item.GetItem() as Wrapper;
		TWidget? widget = item.GetChild() as TWidget;
		if (widget != null && wrapper != null)
			bind(widget, (TData)wrapper.Data);
	}
}


internal class Wrapper : GObject.Object
{
	public Wrapper(object data) : base(true, Array.Empty<GObject.ConstructArgument>())
	{
		Data = data;
	}

	public object Data { get; private init; }
}