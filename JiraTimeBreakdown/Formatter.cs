using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraTimeBreakdown
{
	class Formatter
	{
		public static IEnumerable<string> FormatLines<T, U, V>(
			IEnumerable<T> values,
			IEnumerable<U> columnValues,
			IEnumerable<V> rowValues,
			Func<T, U, V, bool> valueFilter,
			Func<U, string> columnFormatter,
			Func<V, string> rowFormatter,
			Func<IEnumerable<T>, U, V, string> valueFormatter)
		{
			var delim = Constants.CsvDelimiter;
			var lines = new List<string>
			{
				string.Join("", columnValues.Select(cv => $"{delim}{columnFormatter(cv)}"))
			};

			foreach (var rv in rowValues)
			{
				var vals = columnValues.Select(cv =>
				{
					var filtered = values.Where(v => valueFilter(v, cv, rv));
					var formatted = valueFormatter(filtered, cv, rv);

					return formatted;
				});

				lines.Add($"{rowFormatter(rv)}{string.Join("", vals.Select(v => $"{delim}{v}"))}");
			}

			return lines.AsReadOnly();
		}
	}
}
