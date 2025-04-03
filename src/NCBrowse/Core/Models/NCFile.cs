using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;
using NCBrowse.Core.Extensions;
using NCBrowse.Core.Interfaces;
using NCBrowse.Core.Models.Netcdf;

using Range = Microsoft.Research.Science.Data.Range;

namespace NCBrowse.Core.Models;

public class NCFile : INCFile
{
	/// <summary>
	/// The long name attribute as specified by the CF convention.
	/// </summary>
	private const string attribLongName = "long_name";

	private readonly DataSet dataset;

	public NCFile(string path)
	{
		// Ensure file exists.
		if (!File.Exists(path))
			throw new FileNotFoundException($"File not found: '${path}'", path);

		dataset = DataSet.Open(path, ResourceOpenMode.Open);
	}

	/// <inheritdoc />
	public IEnumerable<NCVariable> GetVariables()
	{
		foreach (Variable variable in dataset.Variables)
		{
			string? longName = null;
			if (variable.Metadata.ContainsKey(attribLongName))
				longName = variable.Metadata[attribLongName] as string;
			object? missingValue = variable.MissingValue;
			if (missingValue == null)
			{
				string[] names = new[]
				{
					"missingvalue",
					"missing_value",
					"_missingvalue"
				};
				foreach (string name in names)
				{
					if (variable.Metadata.ContainsKey(name))
					{
						missingValue = variable.Metadata[name];
						if (missingValue != null)
							break;
					}
				}
			}
			IEnumerable<NCAttribute> attributes = GetAttributes(variable);
			yield return new NCVariable(
				variable.Name,
				variable.TypeOfData,
				variable.Dimensions.Select(d => new NCDimension(d.Name, false, d.Length)),
				longName,
				attributes,
				missingValue);
		}
	}

	private IEnumerable<NCAttribute> GetAttributes(Variable variable)
	{
		foreach ( (string key, object value) in variable.Metadata)
		{
			string? valueString = value.ToString();
			if (valueString != null)
				yield return new NCAttribute(key, valueString);
			// todo: else warning
		}
	}

	/// <summary>
	/// Dispose of unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		string path = dataset.URI;
		dataset.Dispose();
	}

	public IEnumerable<DataPoint<T>> ReadTimeSeries<T>(NCVariable variable)
	{

		IReadOnlyList<NCDimension> dims = variable.Dimensions.ToList();
		Range[] indices = new Range[dims.Count];
		for (int i = 0; i < dims.Count; i++)
			indices[i] = DataSet.Range(0, IsTime(dims[i]) ? dims[i].Size - 1 : Math.Min(dims[i].Size - 1, 1));

		// Read variable data.
		T[,,] result = dataset.GetData<T[,,]>(variable.Name, indices);
		IEnumerable<T> result1d;
		if (dims.Count > 3)
			throw new InvalidOperationException($"TBI: more than 3 dimensions");

		if (IsTime(dims[0]))
			result1d = result.Slice3di(0, 0);
		else if (IsTime(dims[1]))
			result1d = result.Slice3dj(0, 0);
		else
			result1d = result.Slice3dk(0, 0);

		// Read time data.
		Dimension time = GetTimeDimension();
		double[] times = ReadTemporalData(time.Name);
		TimeUnits timeUnits = GetTimeUnits(time);

		IEnumerable<(T, double)> zipped = result1d.Zip(times);
		// zipped = zipped.Where(x => x.Item1 != missingValue);

		object? missingValue = variable.MissingValue;
		if (missingValue != null)
		{
			zipped = zipped.Where(x => x.Item1?.Equals(missingValue) == false);
		}

		return zipped.Select(x => new DataPoint<T>(GetDate(timeUnits, x.Item2), x.Item1));
		// return result.Slice3d(0, 0).Zip(time).Select(x => new DataPoint<T>(x.Second, x.First));
	}

    private double[] ReadTemporalData(string timeName)
    {
		try
		{
			Variable variable = dataset.Variables[timeName];
			if (variable.Dimensions.Count != 1)
				throw new InvalidOperationException($"Time variable has {variable.Dimensions.Count} variables. This is not supported by this app (time must be 1-dimensional)");

			Range range = DataSet.Range(0, dataset.Dimensions[timeName].Length - 1);
			Type dataType = variable.TypeOfData;
			Type arrayType = dataType.MakeArrayType();
			BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
			Type[] argumentTypes = new[] { typeof(DataSet), typeof(string), typeof(Range[]) };
			MethodInfo? method = typeof(DataSetExtensions).GetMethod(nameof(DataSetExtensions.GetData), flags, argumentTypes);
			if (method == null)
				throw new InvalidOperationException($"No GetData() method exists for time variable.");

			method = method.MakeGenericMethod(arrayType);

			object? result = method.Invoke(dataset, new object[] { dataset, timeName, new[] { range } });

			if (result == null)
				throw new InvalidCastException($"Time variable contains no data");

			// if (dataType == typeof(double))
			// 	return (double[])result!;

			// if (dataType == typeof(ulong))
			// 	return ((ulong[])result).Select(Convert.ToDouble).ToArray();

			// Call Enumerable.Select() on the array, using Convert.ToDouble().
			//  IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector);
			Type[] selectArgumentTypes = new[] { typeof(IEnumerable<>), typeof(Func<,>) };
			MethodInfo select = typeof(Enumerable).GetGenericMethod(nameof(Enumerable.Select), flags, selectArgumentTypes)!;
			MethodInfo genericSelect = select.MakeGenericMethod(new[] { dataType, typeof(double) });
			// Func<object, double> converter = Convert.ToDouble;
			Type[] toDoubleArgumentTypes = new[] { dataType };
			MethodInfo? toDouble = typeof(Convert).GetMethod(nameof(Convert.ToDouble), flags, toDoubleArgumentTypes);
			if (toDouble == null)
				throw new InvalidOperationException($"Unable to process time variable with type {dataType}; this does not appear to be a numeric type");
			Type converterType = typeof(Func<,>).MakeGenericType(dataType, typeof(double));
			object converter = Delegate.CreateDelegate(converterType, toDouble);
			object[] selectArgs = new object[] { result, converter };

			// Note: this will fail if the data type cannot be converted to  double.
			IEnumerable<double> enumerable = (IEnumerable<double>)genericSelect.Invoke(null, selectArgs)!;
			return enumerable.ToArray();

			// double[] result = dataset.GetData<double[]>(timeName, range);
			// throw new Exception($"Time variable has invalid data type: {dataType.Name}");
			// return result;
		}
		catch (Exception error)
		{
			throw new InvalidOperationException($"Unable to read time data", error);
		}
    }

    private TimeUnits GetTimeUnits(Dimension dim)
	{
		Variable time = dataset.Variables[dim.Name];
		string calendar = (string)time.Metadata["calendar"];
		string units = (string)time.Metadata["units"];

		string pattern = @"([^ ]+) since (.+)";
		Match match = Regex.Match(units, pattern);

		if (!match.Success)
			throw new InvalidOperationException($"Unable to parse time units: '{units}'");

		string deltaStr = match.Groups[1].Value;
		string baseTimeStr = match.Groups[2].Value;

		if (!Enum.TryParse<TimeDelta>(deltaStr, true, out TimeDelta delta))
			throw new InvalidOperationException($"Unable to parse time delta: '{deltaStr}'");

		if (!DateTime.TryParse(baseTimeStr, out DateTime baseTime))
			throw new InvalidOperationException($"Unable to parse datetime: '{baseTimeStr}'");

		return new TimeUnits(baseTime, delta, dim.Name);
	}

	private enum TimeDelta
	{
		Seconds,
		Minutes,
		Hours,
		Days,
		Months,
		Years
	}

	private class TimeUnits
	{
		public DateTime BaseTime { get; private init; }
		public TimeDelta Delta { get; private init; }
		public string Name { get; private init; }
		public TimeUnits(DateTime baseTime, TimeDelta delta, string name)
		{
			BaseTime = baseTime;
			Delta = delta;
			Name = name;
		}
	}

	private DateTime GetDate(TimeUnits units, double timeValue)
	{
		switch (units.Delta)
		{
			case TimeDelta.Seconds:
				return units.BaseTime.AddSeconds(timeValue);
			case TimeDelta.Minutes:
				return units.BaseTime.AddMinutes(timeValue);
			case TimeDelta.Days:
				return units.BaseTime.AddDays(timeValue);
			case TimeDelta.Hours:
				return units.BaseTime.AddHours(timeValue);
			case TimeDelta.Months:
				return units.BaseTime.AddMonths((int)timeValue); // + remainder
			case TimeDelta.Years:
				return units.BaseTime.AddYears((int)timeValue); // + remainder
			default:
				throw new InvalidOperationException($"Unknown time units: {units.Delta}");
		}
	}

	private Dimension GetTimeDimension()
	{
		foreach (Dimension dimension in dataset.Dimensions)
			if (IsTime(dimension.Name))
				return dimension;
		throw new InvalidOperationException($"Time dimension not found");
	}

	private bool IsTime(NCDimension dimension) => IsTime(dimension.Name);

	private bool IsTime(string dimensionName)
	{
		string name = dimensionName.ToLower();
		return name.Contains("time") || name.Contains("date");
	}
}
