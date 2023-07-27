using Gtk;

/// <summary>
/// A widget displaying an item in a list view.
/// </summary>
/// <typeparam name="T">The list view data type (ie the type of the item to be displayed by this widget.</typeparam>
public abstract class ListViewWidget<T> : Box
{
	/// <summary>
	/// Create the widget to be used for a row in the list view.
	/// </summary>
	/// <param name="datum">The datum for this row.</param>
	public abstract void Update(T datum);
}
