using OxyPlot;

namespace NCBrowse.Plotting;

public class DefaultColours : IColourScheme
{
	private static readonly OxyColor[] colours = new OxyColor[]
	{
		OxyColor.FromArgb(255, 230, 159, 0),     // orange
		OxyColor.FromArgb(255, 86, 180, 233),    // sky blue
		OxyColor.FromArgb(255, 0, 158, 115),     // bluish green
		OxyColor.FromArgb(255, 0, 114, 178),     // blue
		OxyColor.FromArgb(255, 213, 94, 0),      // reddish purple
		OxyColor.FromArgb(255, 204, 121, 167),   // vermillion
		OxyColor.FromArgb(255, 240, 228, 66),    // yellow
		OxyColor.FromArgb(255, 0,0,0),           // black
	};

	public IEnumerable<OxyColor> GetColours(int n)
	{
		if (n >= colours.Length)
			throw new InvalidOperationException($"Unable to get {n} colours: {GetType().Name} only supports {colours.Length} colours");
		return colours.Take(n);
	}
}
