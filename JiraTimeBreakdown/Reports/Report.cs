using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JiraTimeBreakdown.Reports
{
	abstract class Report
	{
		protected IssueCache cache;
		protected Action<string> log;

		public Report(IssueCache cache, Action<string> log) => (this.cache, this.log) = (cache, log);

		protected abstract IEnumerable<string> Build();

		public void Process(string fileName = null, bool? noLog = null)
		{
			var lines = Build();

			if (noLog != true)
			{
				foreach (var line in lines)
				{
					log(line);
				}
			}

			if (!string.IsNullOrWhiteSpace(fileName))
			{
				File.WriteAllLines(fileName, lines, Encoding.UTF8);
			}
		}
	}
}
