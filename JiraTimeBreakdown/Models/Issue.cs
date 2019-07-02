using System.Collections.Generic;

namespace JiraTimeBreakdown.Models
{
	class Issue
	{
		public string Id;
		public string Key;
		public string Name;
		public string Description;
		public string Url;

		public string ParentKey;
		public string EpicKey;

		public Issue Parent;
		public Issue Epic;
		public Issue Group => ResultEpic ?? Parent ?? this;

		public Issue ResultEpic => Epic ?? Parent?.ResultEpic;

		public IEnumerable<Worklog> Worklogs;

		public bool WorklogsLoaded;

		public class Comparer : IEqualityComparer<Issue>
		{
			public bool Equals(Issue x, Issue y)
			{
				return x.Key.Equals(y.Key);
			}

			public int GetHashCode(Issue obj)
			{
				return obj.Key.GetHashCode();
			}
		}
	}
}
