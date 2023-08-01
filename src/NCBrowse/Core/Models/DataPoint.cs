namespace NCBrowse.Core.Models;

public class DataPoint<T>
{
	public DateTime Date { get; private init; }
	public T Value { get; private init; }

	public DataPoint(DateTime date, T value)
	{
		Date = date;
		Value = value;
	}
}
