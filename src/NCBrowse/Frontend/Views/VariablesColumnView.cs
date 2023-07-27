using Gtk;
using NCBrowse.Core.Models.Netcdf;

namespace NCBrowse.Frontend.Views;

public class VariablesColumnView : ColumnView
{
	private readonly SignalListItemFactory nameFactory;
	private readonly SignalListItemFactory longNameFactory;
	private readonly Gio.ListStore model;

	public VariablesColumnView(IEnumerable<NCVariable> variables) : this()
	{
		foreach (NCVariable variable in variables)
			AddVariable(variable);
	}

	public VariablesColumnView()
	{
		nameFactory = SignalListItemFactory.New();
		longNameFactory = SignalListItemFactory.New();
		ConnectEvents();

		ColumnViewColumn nameColumn = CreateColumn("Name", nameFactory);
		ColumnViewColumn longNameColumn = CreateColumn("Long Name", longNameFactory);

		AppendColumn(nameColumn);
		AppendColumn(longNameColumn);

		model = Gio.ListStore.New(VariableWrapper.GetGType());
		var selection = SingleSelection.New(model);
		this.Model = selection;
	}

	private ColumnViewColumn CreateColumn(string name, SignalListItemFactory factory)
	{
		ColumnViewColumn column = ColumnViewColumn.New(name, factory);
		return column;
	}

	public override void Dispose()
	{
		DisconnectEvents();
		base.Dispose();
	}

	public void AddVariable(NCVariable variable)
	{
		model.Append(new VariableWrapper(variable));
	}

	private void ConnectEvents()
	{
		nameFactory.OnSetup += OnSetupLabelColumn;
		nameFactory.OnBind += OnBindName;
		longNameFactory.OnSetup += OnSetupLabelColumn;
		longNameFactory.OnBind += OnBindLongName;
	}

	private void DisconnectEvents()
	{
		nameFactory.OnSetup -= OnSetupLabelColumn;
		nameFactory.OnBind -= OnBindName;
		longNameFactory.OnSetup -= OnSetupLabelColumn;
		longNameFactory.OnBind -= OnBindLongName;
	}

	private void OnBindLongName(SignalListItemFactory sender, SignalListItemFactory.BindSignalArgs args)
	{
		try
		{
			HandleBind<Label>(sender, args, (l, variable) => l.SetText(variable.LongName ?? string.Empty));
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
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

	private void OnBindName(SignalListItemFactory sender, SignalListItemFactory.BindSignalArgs args)
	{
		try
		{
			HandleBind<Label>(sender, args, (l, variable) => l.SetText(variable.Name));
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	private void HandleBind<T>(SignalListItemFactory factory, SignalListItemFactory.BindSignalArgs args, Action<T, NCVariable> bind)
		where T : Widget
	{
		ListItem item = (ListItem)args.Object;
		VariableWrapper? wrapper = item.GetItem() as VariableWrapper;
		T? widget = item.GetChild() as T;
		if (widget != null && wrapper != null)
			bind(widget, wrapper.Data);
	}

	private class VariableWrapper : GObject.Object
	{
		public VariableWrapper(NCVariable variable) : base(true, Array.Empty<GObject.ConstructArgument>())
		{
			Data = variable;
		}

		public NCVariable Data { get; private init; }
	}
}
