using NCBrowse.Frontend.Interfaces;

namespace NCBrowse.Frontend.Presenters;

/// <summary>
/// Presenter which can hold any view type. It doesn't do anything by itself.
/// </summary>
public class FallbackPresenter<T> : PresenterBase<IView> where T : IView, new()
{
	/// <summary>
	/// Create a new <see cref="NoFilePresenter"/> instance.
	/// </summary>
	public FallbackPresenter() : base(new T())
	{
	}
}
