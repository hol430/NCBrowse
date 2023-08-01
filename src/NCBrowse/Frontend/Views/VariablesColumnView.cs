using Gtk;
using NCBrowse.Core.Models.Netcdf;
using NCBrowse.Frontend.Extensions;
using NCBrowse.Frontend.Signals;

namespace NCBrowse.Frontend.Views;

public class VariablesColumnView : ColumnView
{
	private readonly SignalListItemFactory nameFactory;
	private readonly SignalListItemFactory longNameFactory;
	private readonly Gio.ListStore model;
	private readonly SingleSelection selection;
	private readonly Event<NCVariable> onSelectionChanged;
	private readonly Event<NCVariable> onVariableActivated;

	public IEvent<NCVariable> OnSelectionChanged => onSelectionChanged;
	public IEvent<NCVariable> OnVariableActivated => onVariableActivated;

	public VariablesColumnView(IEnumerable<NCVariable> variables) : this()
	{
		foreach (NCVariable variable in variables)
			AddVariable(variable);
	}

	public VariablesColumnView()
	{
		nameFactory = SignalListItemFactory.New();
		longNameFactory = SignalListItemFactory.New();

		onSelectionChanged = new Event<NCVariable>();
		onVariableActivated = new Event<NCVariable>();

		model = Gio.ListStore.New(VariableWrapper.GetGType());
		selection = SingleSelection.New(model);

		ConnectEvents();

		ColumnViewColumn nameColumn = CreateColumn("Name", nameFactory);
		ColumnViewColumn longNameColumn = CreateColumn("Long Name", longNameFactory);

		AppendColumn(nameColumn);
		AppendColumn(longNameColumn);

		this.Model = selection;
	}

	private void VariableActivated(ColumnView sender, ActivateSignalArgs args)
	{
		try
		{
			VariableWrapper? wrapper = model.GetObject(args.Position) as VariableWrapper;
			if (wrapper != null)
				onVariableActivated.Invoke(wrapper.Data);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
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
		selection.ConnectOnSelectionChanged(SelectionChanged);

		this.OnActivate += VariableActivated;
	}

	private void DisconnectEvents()
	{
		nameFactory.OnSetup -= OnSetupLabelColumn;
		nameFactory.OnBind -= OnBindName;
		longNameFactory.OnSetup -= OnSetupLabelColumn;
		longNameFactory.OnBind -= OnBindLongName;
		selection.DisconnectOnSelectionChanged(SelectionChanged);
		onSelectionChanged.DisconnectAll();

		this.OnActivate -= VariableActivated;
	}

	private void SelectionChanged(SingleSelection sender, GtkExtensions.SelectionChangedSignalArgs args)
	{
		try
		{
			VariableWrapper? wrapper = model.GetObject(selection.Selected) as VariableWrapper;
			if (wrapper == null)
				return;

			NCVariable variable = wrapper.Data;
			onSelectionChanged.Invoke(variable);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
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
