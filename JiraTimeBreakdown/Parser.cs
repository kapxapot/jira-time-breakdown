using JiraTimeBreakdown.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace JiraTimeBreakdown
{
	class Parser
	{
		public static Issue ParseIssue(JToken data)
		{
			var issue = new Issue
			{
				Id = (string)data["id"],
				Key = (string)data["key"],
				Name = (string)data["fields"]["summary"],
				Description = (string)data["fields"]["description"],
				Url = (string)data["self"],

				EpicKey = (string)data["fields"]["customfield_10900"],
			};

			// parent?
			var parent = data["fields"]["parent"];

			if (parent != null)
			{
				issue.ParentKey = (string)parent["key"];
			}

			// worklogs?
			var worklog = data["fields"]["worklog"];

			if (worklog != null)
			{
				issue.Worklogs = worklog["worklogs"].Select(w => ParseWorklog(w, issue));

				var total = (int)worklog["total"];

				issue.WorklogsLoaded = (issue.Worklogs.Count() == total);
			}

			return issue;
		}

		public static Worklog ParseWorklog(JToken data, Issue issue = null)
		{
			return new Worklog
			{
				Id = (string)data["id"],
				Author = ParseUser(data["author"]),
				Started = (DateTime)data["started"],
				Duration = TimeSpan.FromSeconds((int)data["timeSpentSeconds"]),
				Issue = issue,
				Url = (string)data["self"],
			};
		}

		public static User ParseUser(JToken data)
		{
			return new User
			{
				Key = (string)data["key"],
				Name = (string)data["name"],
				DisplayName = (string)data["displayName"],
				Url = (string)data["self"],
			};
		}
	}
}
