using Gio;
using GObject;
using Gtk;
using NCBrowse.Frontend.Views;
using System.Reflection;
using System.Runtime.InteropServices;
using Action = System.Action;
using Application = Gtk.Application;
using File = System.IO.File;
using Task = System.Threading.Tasks.Task;

namespace NCBrowse.Frontend.Extensions;

/// <summary>
/// Extension methods for gtk types.
/// </summary>
public static class GtkExtensions
{
	[DllImport("libgtk-4.so.1", EntryPoint = "gtk_file_chooser_cell_get_type")]
	public static extern nuint FixFileChooser();

	/// <summary>
	/// Load style from a resource file embedded in the current assembly.
	/// </summary>
	/// <param name="provider">The style provider.</param>
	/// <param name="resourceName">Name of the embedded resource.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	public static async Task LoadFromEmbeddedResourceAsync(this CssProvider provider, string resourceName
		, CancellationToken cancellationToken = default(CancellationToken))
	{
		string tempFile = Path.GetTempFileName();
		try
		{
			using (Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
			{
				if (stream == null)
					throw new InvalidOperationException($"Resource not found: '{resourceName}'");

				using (Stream writer = System.IO.File.OpenWrite(tempFile))
					await stream.CopyToAsync(writer);
				provider.LoadFromFile(FileHelper.NewForPath(tempFile));
			}
		}
		finally
		{
			if (File.Exists(tempFile))
				File.Delete(tempFile);
		}
	}

	/// <summary>
	/// Load style from a resource file embedded in the current assembly.
	/// </summary>
	/// <param name="provider">The style provider.</param>
	/// <param name="resourceName">Name of the embedded resource.</param>
	public static void LoadFromEmbeddedResource(this CssProvider provider, string resourceName)
	{
		provider.LoadFromEmbeddedResourceAsync(resourceName).Wait();
	}

	/// <inheritdoc />
	public static void AddMenuItem(this Menu menu, string domain, string name, Action callback, string? hotkey = null)
	{
		menu.AddMenuItem(domain, name, (_, __) => callback(), hotkey);
	}

	/// <inheritdoc />
	public static void AddMenuItem(this Menu menu, string domain, string name, SignalHandler<SimpleAction, SimpleAction.ActivateSignalArgs> callback, string? hotkey = null)
	{
		Application app = MainView.AppInstance;

		string actionName = name.ToLower().Replace(" ", "-");
		string fullName = $"{domain}.{actionName}";

		menu.Append($"_{name}", fullName);
		var action = SimpleAction.New(actionName, null);
		action.OnActivate += callback;
		if (hotkey != null)
			app.SetAccelsForAction(fullName, new[] { hotkey });
		app.AddAction(action);
		// action.Dispose();
	}

	public sealed class SelectionChangedSignalArgs : SignalArgs
	{
		public uint Position => Args[1].Extract<uint> ();
		public uint NItems => Args[2].Extract<uint> ();

	}

	private static readonly Signal<SingleSelection, SelectionChangedSignalArgs> SelectionChangedSignal = new (
		unmanagedName: "selection-changed",
		managedName: string.Empty
	);

	// TODO-GTK4 (bindings) - the Gtk.SelectionModel::selection-changed signal is not generated (https://github.com/gircore/gir.core/issues/831)
	public static void ConnectOnSelectionChanged (this SingleSelection o, SignalHandler<SingleSelection, SelectionChangedSignalArgs> handler)
	{
		SelectionChangedSignal.Connect (o, handler);
	}
	// TODO-GTK4 (bindings) - the Gtk.SelectionModel::selection-changed signal is not generated (https://github.com/gircore/gir.core/issues/831)
	public static void DisconnectOnSelectionChanged (this SingleSelection o, SignalHandler<SingleSelection, SelectionChangedSignalArgs> handler)
	{
		SelectionChangedSignal.Disconnect (o, handler);
	}
}
