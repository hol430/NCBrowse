namespace NCBrowse.Core.Extensions;

public static class ArrayExtensions
{
	public static IEnumerable<T> Slice3d<T>(this T[,,] array, int j, int k)
	{
		for (int i = 0; i < array.GetLength(0); i++)
		{
			yield return array[i, j, k];
		}
	}
}
