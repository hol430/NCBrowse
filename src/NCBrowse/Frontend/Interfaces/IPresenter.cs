namespace NCBrowse.Frontend.Interfaces;

/// <summary>
/// An interface for a presenter.
/// </summary>
public interface IPresenter<out T> : IDisposable where T : IView
{
	/// <summary>
	/// Get the view owned by this presenter.
	/// </summary>
	public T GetView();
}
