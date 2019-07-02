using JiraTimeBreakdown.Models;
using System.Collections.Generic;

namespace JiraTimeBreakdown
{
	interface ILoader
	{
		Issue LoadIssue(string key);
		IEnumerable<Worklog> LoadIssueWorklogs(Issue issue);
	}
}
