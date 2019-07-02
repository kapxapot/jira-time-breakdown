using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiraTimeBreakdown.Reports
{
	class MonthlyReport : Report
	{
		public MonthlyReport(IssueCache cache, Action<string> log = null) : base(cache, log)
		{
		}

		protected override IEnumerable<string> Build()
		{
			log?.Invoke($"Issues count: {cache.IssueCount}");

			return Formatter.FormatLines(
				cache.GetWorklogs(),
				cache.GetMonths(),
				cache.GetUsers(),
				(value, month, user) => value.Author.Key == user.Key && value.Month == month,
				cv => cv.ToShortDateString(),
				rv => rv.Key,
				(values, cv, rv) => values.Sum(v => v.Days).ToString(Constants.NumberFormat)
			);
		}
	}
}
