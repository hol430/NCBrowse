using Gtk;
using NCBrowse.Core.Models;
using NCBrowse.Core.Models.Netcdf;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.GtkSharp;
using NCBrowse.Plotting;
using NCBrowse.Frontend.Extensions;

namespace NCBrowse.Frontend.Views;

public class PlotWindow : Window
{
	private readonly PlotModel model;
	private readonly PlotView plotView;
	public PlotWindow(NCVariable variable, IEnumerable<DataPoint<double>> data, IColourScheme colourScheme) : base()
	{
		Title = variable.Name;
		DefaultWidth = 1080;
		DefaultHeight = 720;

		var timeseries = new LineSeries();
		timeseries.Points.AddRange(data.Select(d => new DataPoint(
			DateTimeAxis.ToDouble(d.Date), d.Value)
		));
		timeseries.Color = colourScheme.GetColours(1).ElementAt(0);

		Axis xAxis = new DateTimeAxis();
		xAxis.Title = "Date";
		Axis yAxis = new LinearAxis();
		string? units = variable.Units;
		if (string.IsNullOrEmpty(units))
			yAxis.Title = variable.Name;
		else
			yAxis.Title = $"{variable.Name} ({units})";

		OxyColor foregroundColour = GetForegroundColour();

		model = new PlotModel();
		model.Series.Add(timeseries);
		model.Axes.Add(xAxis);
		model.Axes.Add(yAxis);
		model.Title = variable.LongName ?? variable.Name;

		// Configure foreground colours.
		model.SetForegroundColour(foregroundColour);

		plotView = new PlotView();
		plotView.Model = model;

		Child = plotView;
	}

	private OxyColor GetForegroundColour()
	{
		if (Gtk.Settings.GetDefault()?.GtkApplicationPreferDarkTheme == true)
			return OxyColors.White;
		return OxyColors.Black;
	}
}
