using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace JiraTimeBreakdown
{
	internal class JiraApi
	{
		private string api;
		private string auth;
		private Action<string> log;

		public JiraApi(string api, string user, string password, Action<string> log = null)
		{
			this.api = api;

			var bytes = Encoding.UTF8.GetBytes($"{user}:{password}");
			this.auth = Convert.ToBase64String(bytes);

			this.log = log;
		}

		public IEnumerable<T> LoadMany<T>(string baseUrl, Dictionary<string, string> baseParams, int pageSize, Func<JObject, IEnumerable<T>> process)
		{
			if (baseParams == null)
			{
				baseParams = new Dictionary<string, string>();
			}

			var result = new List<T>();
			var page = 0;
			bool complete;

			do
			{
				var offset = page * pageSize;
				var parameters = new Dictionary<string, string> {
					{ "startAt", offset.ToString() },
					{ "maxResults", pageSize.ToString() },
				};

				var url = BuildUrl(baseUrl, baseParams.Concat(parameters).ToDictionary(kv => kv.Key, kv => kv.Value));
				var data = GetJson(url);

				try
				{
					var total = (int)data["total"];
					var chunk = process(data);

					if (chunk == null)
					{
						throw new ApplicationException($"Error loading chunk! Offset: {offset}");
					}

					var loaded = chunk.Count();

					log?.Invoke($"Successfully loaded {loaded} items ({offset + loaded} / {total}).");

					result.AddRange(chunk);

					complete = (offset + loaded >= total);

					page++;
				}
				catch (Exception ex)
				{
					throw new ApplicationException("Unexpected JSON format", ex);
				}
			} while (!complete);

			return result;
		}

		public T LoadOne<T>(string url, Func<JObject, T> process)
		{
			url = BuildUrl(url);

			var data = GetJson(url);
			var errors = data["errorMessages"];

			if (errors != null && errors.Any())
			{
				throw new ApplicationException($"Error: {errors.First()}");
			}

			try
			{
				return process(data);
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Unexpected JSON format", ex);
			}
		}

		private string BuildUrl(string url, Dictionary<string, string> parameters = null)
			=> AppendParams($"{api}/{url}", parameters);

		private string AppendParams(string url, Dictionary<string, string> parameters)
		{
			if (parameters == null || parameters.Count == 0)
			{
				return url;
			}

			var sb = new StringBuilder(url);
			sb.Append("?");
			sb.Append(string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}")));

			return sb.ToString();
		}

		private JObject GetJson(string url)
		{
			try
			{
				using (var wc = new WebClient())
				{
					wc.Headers.Add("Authorization", $"Basic {auth}");

					log?.Invoke($"Loading page {url}...");

					var raw = wc.DownloadString(url);
					var json = (JObject)JsonConvert.DeserializeObject(raw);

					return json;
				}
			}
			catch (Exception ex)
			{
				throw new ApplicationException($"Error loading JSON data for url: {url}", ex);
			}
		}
	}
}
