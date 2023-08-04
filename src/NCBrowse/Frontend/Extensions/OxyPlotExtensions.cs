using OxyPlot;
using OxyPlot.Axes;

namespace NCBrowse.Frontend.Extensions;

public static class OxyPlotExtensions
{
	public static void SetForegroundColour(this PlotModel model, OxyColor colour)
	{
		model.TextColor = colour;
		model.TitleColor = colour;
		model.SubtitleColor = colour;
		model.PlotAreaBorderColor = colour;
		foreach (Axis axis in model.Axes)
			axis.SetForegroundColour(colour);
	}

	public static void SetForegroundColour(this Axis axis, OxyColor colour)
	{
		axis.TextColor = colour;
		axis.TitleColor = colour;
		axis.AxislineColor = colour;
		axis.TicklineColor = colour;
		axis.MajorGridlineColor = colour;
		axis.MinorGridlineColor = colour;
		axis.ExtraGridlineColor = colour;
		axis.MinorTicklineColor = colour;
	}
}
