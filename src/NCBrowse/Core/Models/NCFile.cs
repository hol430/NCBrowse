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
			IEnumerable<NCAttribute> attributes = GetAttributes(variable);
			yield return new NCVariable(
				variable.Name,
				variable.TypeOfData,
				variable.Dimensions.Select(d => new NCDimension(d.Name, false, d.Length)),
				longName,
				attributes);
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

		// Read time data.
		Dimension time = GetTimeDimension();
		double[] times = ReadTemporalData(time.Name);
		TimeUnits timeUnits = GetTimeUnits(time);

		return result.Slice3d(0, 0).Zip(times).Select(x => new DataPoint<T>(GetDate(timeUnits, x.Second), x.First));
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
			Type[] argumentTypes = new[] { typeof(DataSet), typeof(string), typeof(Range) };
			MethodInfo? method = typeof(DataSetExtensions).GetMethod(nameof(DataSetExtensions.GetData), flags, argumentTypes);
			if (method == null)
				throw new InvalidOperationException($"Time has invalid data type: {dataType.Name}. No GetData() method exists for this type.");

			method = method.MakeGenericMethod(arrayType);

			object? result = method.Invoke(dataset, new object[] { dataset, timeName, range });

			if (result == null)
				throw new InvalidCastException($"Time variable contains no data");

			if (dataType == typeof(double))
				return (double[])result!;

			// double[] result = dataset.GetData<double[]>(timeName, range);
			throw new Exception("TBI");
			// return result;
		}
		catch (Exception error)
		{
			throw new InvalidOperationException($"Unable to read time data", error);
		}
    }

    private TimeUnits GetTimeUnits(Dimension dim)
	{
		if (dataset.Variables[dim.Name].TypeOfData != typeof(double))
			throw new InvalidOperationException($"Only double typed time dimension is supported");

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
