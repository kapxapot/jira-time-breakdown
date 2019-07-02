using JiraTimeBreakdown.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraTimeBreakdown
{
	class IssueCache
	{
		private ConcurrentDictionary<string, Issue> cache;
		private ILoader loader;
		private Action<string> log;

		private Func<Worklog, bool> worklogFilter;

		public IssueCache(
			IEnumerable<Issue> issues,
			ILoader loader,
			Action<string> log = null,
			Func<Worklog, bool> worklogFilter = null
		)
		{
			this.cache = new ConcurrentDictionary<string, Issue>(issues.ToDictionary(issue => issue.Key));
			this.loader = loader;
			this.log = log;

			this.worklogFilter = worklogFilter;
		}

		public Issue this[string key]
		{
			get
			{
				return cache.ContainsKey(key)
					? cache[key]
					: null;
			}
		}

		public IEnumerable<Issue> GetIssues() =>
			cache.Values.ToList()
			.Where(i => i.Worklogs
				.Any(w => worklogFilter == null || worklogFilter(w))
			);

		public IEnumerable<IGrouping<Issue, Issue>> GetIssueGroups()
		{
			return GetIssues().GroupBy(i => i.Group, new Issue.Comparer());
		}

		public IEnumerable<Worklog> GetWorklogs() => GetIssues()
			.SelectMany(i => i.Worklogs)
			.Where(w => worklogFilter == null || worklogFilter(w));

		public IEnumerable<User> GetUsers() => GetWorklogs()
			.Select(w => w.Author)
			.GroupBy(u => u.Key)
			.Select(g => g.First())
			.OrderBy(u => u.Key);

		public IEnumerable<DateTime> GetMonths() => GetWorklogs()
			.Select(w => w.Month)
			.Distinct()
			.OrderBy(m => m);

		public int IssueCount => cache.Values.Count;

		public IssueCache WithParents()
		{
			while (true)
			{
				var orphans = cache.Values
					.Where(i => i.ParentKey != null && i.Parent == null)
					.ToList();

				var parentsToLoad = orphans
					.Select(i => i.ParentKey)
					.Distinct()
					.ToList();

				if (!parentsToLoad.Any())
				{
					break;
				}

				Parallel.ForEach(parentsToLoad, parentKey =>
				{
					cache[parentKey] = loader.LoadIssue(parentKey);
				});

				foreach (var issue in orphans)
				{
					log?.Invoke($"Updating parent for issue {issue.Key}...");

					issue.Parent = cache[issue.ParentKey];
				}
			}

			return this;
		}

		public IssueCache WithEpics()
		{
			while (true)
			{
				var epicless = cache.Values
					.Where(i => i.EpicKey != null && i.Epic == null)
					.ToList();

				var epicsToLoad = epicless
					.Select(i => i.EpicKey)
					.Distinct()
					.ToList();

				if (!epicsToLoad.Any())
				{
					break;
				}

				Parallel.ForEach(epicsToLoad, epicKey =>
				{
					cache[epicKey] = loader.LoadIssue(epicKey);
				});

				foreach (var issue in epicless)
				{
					log?.Invoke($"Updating epic for issue {issue.Key}...");

					issue.Epic = cache[issue.EpicKey];
				}
			}

			return this;
		}

		public IssueCache WithWorklogs()
		{
			var issues = GetIssues().Where(i => !i.WorklogsLoaded);

			Parallel.ForEach(issues, issue =>
			{
				issue.Worklogs = loader.LoadIssueWorklogs(issue);
				issue.WorklogsLoaded = true;
			});

			return this;
		}

		public IssueCache FilterWorklogs(Func<Worklog, bool> worklogFilter)
		{
			this.worklogFilter = worklogFilter;

			return this;
		}

		public IssueCache FilterBy(Func<Issue, bool> func)
			=> new IssueCache(GetIssues().Where(func), loader, log, worklogFilter);

		public IssueCache FilterByIssueKeys(IEnumerable<string> keys)
			=> keys?.Any() == true
				? FilterBy(i => keys.Contains(i.Group.Key))
				: this;
	}
}
