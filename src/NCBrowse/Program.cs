using Adw;

using NCBrowse.Frontend.Presenters;
using NCBrowse.Frontend.Views;

const string appName = "org.Hie.ncbrowse"; // todo: better name

var app = Application.New(appName, Gio.ApplicationFlags.FlagsNone);

PangoCairo.Module.Initialize();
Pango.Module.Initialize();
Cairo.Module.Initialize();

app.OnStartup += OnStartup;
app.OnActivate += OnActivated;
app.OnShutdown += OnShutdown;

app.Run(args.Length, args);

void OnStartup(object sender, EventArgs args)
{
	// Perform one-time application initialisation here.
}

void OnActivated(object sender, EventArgs data)
{
	Application app = (Application)sender;
	MainView window = new MainView(app);
	try
	{
		MainPresenter presenter = new MainPresenter(window);
		window.Show();
		if (args.Length > 0)
		{
			string arg = args[0];
			if (File.Exists(arg))
				presenter.OpenFile(arg);
		}
	}
	catch (Exception error)
	{
		Console.Error.WriteLine(error);
		window.ReportError(error);
		app.Quit();
	}
}

void OnShutdown(object sender, EventArgs args)
{
	// Application has been closed. Any closing logic can go here.
}
