using System.Globalization;

namespace JiraTimeBreakdown
{
	class Constants
	{
		public const int WorkHours = 8;
		public const int ApiPageSize = 100;
		public const string CsvDelimiter = ";";

		public static readonly NumberFormatInfo NumberFormat = new NumberFormatInfo { NumberDecimalSeparator = "," };
	}
}
