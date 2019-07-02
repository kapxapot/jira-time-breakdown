using JiraTimeBreakdown.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraTimeBreakdown
{
	class Tests
	{
		private static void LoaderTest(Loader loader, Action<string> log = null)
		{
			var issue = loader.LoadIssue("CCSPT-1598");

			log?.Invoke($"Issue {issue.Key}, {issue.Worklogs.Count()} worklogs, worklogs loaded? {(issue.WorklogsLoaded ? "yes" : "no")}");

			issue.Worklogs = loader.LoadIssueWorklogs(issue);
			issue.WorklogsLoaded = true;

			log?.Invoke($"Issue {issue.Key}, {issue.Worklogs.Count()} worklogs, worklogs loaded? {(issue.WorklogsLoaded ? "yes" : "no")}");

			foreach (var w in issue.Worklogs)
			{
				log?.Invoke($"Author: {w.Author.Key}, date: {w.Started}, month: {w.Started.Month}, duration: {w.Duration.TotalSeconds}");
			}
		}

		private static void IssuesTest(IEnumerable<Issue> issues, Action<string> log = null)
		{
			foreach (var issue in issues)
			{
				log?.Invoke($"[{issue.Key}] parent: {issue.ParentKey}, epic: {issue.ResultEpic?.Key}, worklogs: {issue.Worklogs.Count()}");
			}
		}
	}
}
