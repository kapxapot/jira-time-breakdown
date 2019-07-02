using JiraTimeBreakdown.Reports;
using System;

namespace JiraTimeBreakdown
{
	class Program
	{
		const string api = "jira api url";
		const string user = "username";
		const string password = "password";

		static void Main(string[] args)
		{
			Action<string> log = msg => Console.WriteLine(msg);

			var jiraApi = new JiraApi(api, user, password, log);
			var loader = new Loader(jiraApi, Constants.ApiPageSize);

			var start = new DateTime(2019, 1, 1);
			var end = new DateTime(2019, 12, 31);

			var issues = loader.LoadIssues("ABC", start, end);

			var cache = new IssueCache(issues, loader, log)
				.WithParents()
				.WithEpics()
				.WithWorklogs();

			//new MonthlyReport(cache.FilterByIssueKeys(new[] { "ABC-123", "ABC-456" }), log)
			//	.Process("filtered_monthly.csv");

			//new MonthlyReport(cache, log)
			//	.Process("total_monthly.csv");

			var userKeys = new[] { "user1", "user2", "user3" };

			foreach (var userKey in userKeys)
			{
				new UserHalfYearReport(
					cache.FilterWorklogs(w =>
						w.Started >= start.Date &&
						w.Started < end.Date.AddDays(1) &&
						w.Author.Key == userKey), 
					log)
					.Process($"2019_{userKey}.csv", true);
			}
		}
	}
}
