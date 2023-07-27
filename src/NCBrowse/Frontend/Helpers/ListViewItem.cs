using Gtk;

namespace NCBrowse.Frontend.Helpers;

/// <summary>
/// An item for a <see cref="ListView"/>.
/// </summary>
public class ListViewItem<T> : GObject.Object
{
	/// <summary>
	/// The list item data.
	/// </summary>
	public T? Data { get; private init; }

	/// <summary>
	/// Create a new <see cref="ListViewItem{T}"/> instance.
	/// </summary>
	/// <param name="data">The list view item data.</param>
	public ListViewItem(T data) : base(true, Array.Empty<GObject.ConstructArgument>())
	{
		Data = data;
	}
}
