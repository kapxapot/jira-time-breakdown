using JiraTimeBreakdown.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraTimeBreakdown
{
	class Loader : ILoader
	{
		private JiraApi api;
		private int pageSize;

		public Loader(JiraApi api, int pageSize) => (this.api, this.pageSize) = (api, pageSize);

		public IEnumerable<Issue> LoadIssues(string project, DateTime start, DateTime end)
		{
			var startStr = start.ToString("yyyy-MM-dd");
			var endStr = end.ToString("yyyy-MM-dd");

			return api.LoadMany(
				"search",
				new Dictionary<string, string>
				{
					{ "jql", $"project+%3D+{project}+and+created+%3C+{endStr}+and+updated+%3E+{startStr}+and+timespent+%3E+0" },
					{ "fields", "key,parent,summary,description,customfield_10900,worklog" }
				},
				pageSize,
				data =>
				{
					return data["issues"]?.Select(i => Parser.ParseIssue(i));
				}
			);
		}

		public Issue LoadIssue(string key)
		{
			return api.LoadOne($"issue/{key}", data => Parser.ParseIssue(data));
		}

		public IEnumerable<Worklog> LoadIssueWorklogs(Issue issue)
		{
			return api.LoadMany(
				$"issue/{issue.Key}/worklog",
				null,
				pageSize,
				data =>
				{
					return data["worklogs"]?.Select(w => Parser.ParseWorklog(w, issue));
				}
			);
		}
	}
}
