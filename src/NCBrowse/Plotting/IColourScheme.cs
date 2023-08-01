using OxyPlot;

namespace NCBrowse.Plotting;

public interface IColourScheme
{
	public IEnumerable<OxyColor> GetColours(int n);	
}
