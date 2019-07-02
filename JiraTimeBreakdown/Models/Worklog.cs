using System;
using System.Collections.Generic;
using System.Text;

namespace JiraTimeBreakdown.Models
{
	class Worklog
	{
		public string Id;
		public DateTime Started;
		public TimeSpan Duration;
		public string Url;

		public User Author;
		public Issue Issue;
		public Issue Group => Issue.Group;

		public DateTime Month => new DateTime(Started.Year, Started.Month, 1);

		public double Hours => Duration.TotalHours;

		public double Days => Hours / Constants.WorkHours;
	}
}
