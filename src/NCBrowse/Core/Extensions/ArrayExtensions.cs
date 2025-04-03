namespace NCBrowse.Core.Extensions;

public static class ArrayExtensions
{
	/// <summary>
	/// Take a slice along the i dimension of the specified data.
	/// </summary>
	/// <param name="array">3-dimensional array.</param>
	/// <param name="j">j-value.</param>
	/// <param name="k">k-value</param>
	/// <returns></returns>
	public static IEnumerable<T> Slice3di<T>(this T[,,] array, int j, int k)
	{
		for (int i = 0; i < array.GetLength(0); i++)
		{
			yield return array[i, j, k];
		}
	}

	public static IEnumerable<T> Slice3dj<T>(this T[,,] array, int i, int k)
	{
		for (int j = 0; j < array.GetLength(1); j++)
		{
			yield return array[i, j, k];
		}
	}

	public static IEnumerable<T> Slice3dk<T>(this T[,,] array, int i, int j)
	{
		for (int k = 0; k < array.GetLength(2); k++)
		{
			yield return array[i, j, k];
		}
	}
}
