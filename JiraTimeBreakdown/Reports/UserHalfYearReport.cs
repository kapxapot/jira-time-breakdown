using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraTimeBreakdown.Reports
{
	class UserHalfYearReport : Report
	{
		public UserHalfYearReport(IssueCache cache, Action<string> log = null) : base(cache, log)
		{
		}

		protected override IEnumerable<string> Build()
		{
			var fields = new[] { "Name", "Description", "Hours" };

			var groups = cache.GetIssueGroups();

			var groupKeys = groups
				.OrderBy(g => g.Key.Id)
				.Select(g => g.Key.Key);

			var worklogs = cache.GetWorklogs();

			return Formatter.FormatLines(
				worklogs,
				fields,
				groupKeys,
				(value, field, groupKey) => value.Group.Key == groupKey,
				cv => cv,
				rv => rv,
				(values, cv, rv) => {
					var groupKey = rv;
					var root = cache[groupKey];
					var group = groups.FirstOrDefault(g => g.Key.Key == groupKey);

					if (group == null || root == null)
					{
						return "INVALID GROUP";
					}

					switch (cv)
					{
						case "Name":
							return root.Name;

						case "Description":
							return root.Description
								.Replace("\r\n", " ")
								.Replace("\r", " ")
								.Replace("\n", " ")
								.Replace(Constants.CsvDelimiter, " ");

						case "Hours":
							return values.Sum(v => v.Hours).ToString(Constants.NumberFormat);
					}

					return "INVALID COLUMN";
				}
			);
		}
	}
}
