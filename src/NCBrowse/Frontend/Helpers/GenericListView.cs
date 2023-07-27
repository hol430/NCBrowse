using Gtk;
using NCBrowse.Frontend.Extensions;
using System.Reflection;

namespace NCBrowse.Frontend.Views;

/// <summary>
/// A wrapper around a gtk ListView widget which allows storing
/// </summary>
/// <typeparam name="T"></typeparam>
public class GenericListView<TData, TWidget> : Adw.Bin
	where TData : GObject.Object
	where TWidget : ListViewWidget<TData>, new()
{
	/// <summary>
	/// The list model which holds the data.
	/// </summary>
	private readonly Gio.ListStore model;

	/// <summary>
	/// The selection model.
	/// </summary>
	private readonly SingleSelection selection;

	/// <summary>
	/// The list item factory.
	/// </summary>
	private readonly SignalListItemFactory factory;

	/// <summary>
	/// The list view widget.
	/// </summary>
	private readonly Widget view;

	/// <summary>
	/// Create a new <see cref="GenericListView{TData, TWidget}"/> instance.
	/// </summary>
	public GenericListView()
	{
		// The generic type assertions guarantee that the widget type will have
		// a GetGType() method.
		string getGtypeName = nameof(GObject.Object.GetGType);
		const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
		MethodInfo getGtype = typeof(TData).GetMethod(getGtypeName, flags)!;
		GObject.Type gtype = (GObject.Type)getGtype.Invoke(null, null)!;
		model = Gio.ListStore.New(gtype);

		selection = SingleSelection.New(model);
		factory = SignalListItemFactory.New();

		ConnectEvents();

		view = ListView.New(selection, factory);

		ScrolledWindow scroller = new ScrolledWindow();
		scroller.PropagateNaturalHeight = true;
		scroller.PropagateNaturalWidth = true;
		scroller.Child = view;

		Child = scroller;
	}

	/// <summary>
	/// Add an item to the model.
	/// </summary>
	/// <param name="datum">The model.</param>
	public void AddItem(TData datum)
	{
		model.Append(datum);
	}

	/// <summary>
	/// Connect events sources to sinks.
	/// </summary>
	private void ConnectEvents()
	{
		selection.ConnectOnSelectionChanged(OnSelectionChanged);
		factory.OnSetup += OnSetup;
		factory.OnBind += OnBind;
	}

	/// <summary>
	/// Disconnect events sources from sinks.
	/// </summary>
	private void DisconnectEvents()
	{
		selection.DisconnectOnSelectionChanged(OnSelectionChanged);
		factory.OnSetup -= OnSetup;
		factory.OnBind -= OnBind;
	}

	private void OnBind(SignalListItemFactory factory, SignalListItemFactory.BindSignalArgs args)
	{
		try
		{
			ListItem item = (ListItem)args.Object;
			TData? datum = item.GetItem() as TData;
			TWidget? widget = (TWidget?)item.GetChild();
			if (widget != null && datum != null)
				widget.Update(datum);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	private void OnSetup(SignalListItemFactory factory, SignalListItemFactory.SetupSignalArgs args)
	{
		try
		{
			ListItem item = (ListItem)args.Object;
			item.SetChild(new TWidget());
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	private void OnSelectionChanged(SingleSelection sender, GtkExtensions.SelectionChangedSignalArgs args)
	{
		try
		{
			// todo
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	public override void Dispose()
	{
		DisconnectEvents();
		model.Dispose();
		selection.Dispose();
		factory.Dispose();
		base.Dispose();
	}
}
