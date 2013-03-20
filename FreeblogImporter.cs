using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Fb2Wp
{
	class FreeblogImporter : IImporter
	{
		const string XmlnsExport = "http://enyim.com/schemas/blossom/export/2008";
		const string XmlnsEnyim = "http://enyim.com/schemas/rss/core/2006";

		private readonly string root;
		private readonly Dictionary<long, AuthenticatedUser> authUserCache;

		public FreeblogImporter(string root)
		{
			this.root = root;
			this.authUserCache = LoadAuthUserCache();
		}

		public IEnumerable<Comment> GetComments()
		{
			var doc = OpenXml("comments.xml");
			var xnIp = XName.Get("ip", XmlnsExport);
			var xnEntry = XName.Get("entry", XmlnsExport);

			var retval = doc.Root
							.XPathSelectElements("/rss/channel/item")
							.Select(node => new Comment
							{
								Id = (long)node.Element("guid"),
								EntryId = (long)node.Element(xnEntry),

								Author = ToUser(node.Element("author")),
								Content = (string)node.Element("description"),
								Ip = (string)node.Element(xnIp),
								Timestamp = DateTimeOffset.Parse((string)node.Element("pubDate"), CultureInfo.InvariantCulture)
							})
							.ToArray();

			var tasks = authUserCache.Values.Select(u => u.Load()).ToArray();

			Task.WaitAll(tasks);

			SaveAuthUserCache();

			return retval;
		}

		private Dictionary<long, AuthenticatedUser> LoadAuthUserCache()
		{
			if (!File.Exists("users.txt"))
				return new Dictionary<long, AuthenticatedUser>();

			return File.ReadAllLines("users.txt")
						.Select(l => l.Split('\t'))
						.ToDictionary(l => Int64.Parse(l[0]), l => new AuthenticatedUser(l[1]));
		}

		private void SaveAuthUserCache()
		{
			using (var sw = File.CreateText("users.txt"))
			{
				foreach (var kvp in authUserCache)
				{
					sw.WriteLine(kvp.Key + "\t" + kvp.Value.Name);
				}
			}
		}

		public IEnumerable<Entry> GetEntries()
		{
			var doc = OpenXml("entries.xml");
			var xnExcerpt = XName.Get("excerpt", XmlnsEnyim);
			var xnTag = XName.Get("tag", XmlnsEnyim);
			var categoryMap = GetCategories()
								.ToDictionary(c => c.Id.ToString(CultureInfo.InvariantCulture));

			return doc.Root
						.XPathSelectElements("/rss/channel/item")
						.Select(node => new Entry
						{
							Id = (long)node.Element("guid"),
							Title = (string)node.Element("title"),
							Timestamp = DateTimeOffset.Parse((string)node.Element("pubDate"), CultureInfo.InvariantCulture),

							Content = (string)node.Element("description"),
							Excerpt = (string)node.Element(xnExcerpt),
							Author = (string)node.Element("author"),

							Categories = node.Elements("category").Select(c => categoryMap[(string)c]).ToList(),
							Tags = node.Elements(xnTag).Select(e => (string)e).ToList()
						});
		}

		private IEnumerable<Category> GetCategories()
		{
			var doc = OpenXml("categories.xml");
			var xnAlias = XName.Get("alias", XmlnsEnyim);

			return doc.Root
						.XPathSelectElements("/rss/channel/item")
						.Select(node => new Category
						{
							Id = (long)node.Element("guid"),

							Alias = (string)node.Element(xnAlias),
							Title = (string)node.Element("title")
						});
		}

		private User ToUser(XElement author)
		{
			var isAuth = author.Attribute("isAuthenticated");
			if (isAuth != null && isAuth.Value == "true")
			{
				AuthenticatedUser retval;
				var id = (long)author;

				if (!authUserCache.TryGetValue(id, out retval))
					authUserCache[id] = retval = new AuthenticatedUser(id);

				return retval;
			}

			return new AnonymousUser
			(
				(string)author,
				(string)author.Parent.Element(XName.Get("email", XmlnsExport))
			);
		}

		private XDocument OpenXml(string name)
		{
			return XDocument.Load(Path.Combine(root, name));
		}

		#region AuthenticatedUser

		class AuthenticatedUser : User
		{
			private string name;
			private string email;

			private long id;

			public AuthenticatedUser(long id)
			{
				this.id = id;
			}

			public AuthenticatedUser(string name)
			{
				this.name = name;
			}

			public override string Name { get { return name; } }
			public override string Email { get { return email; } }

			public async Task Load()
			{
				if (id == 0) return;

				var content = await new WebClient().DownloadStringTaskAsync("http://admin.freeblog.hu/profile/" + this.id + "/");

				Console.WriteLine("Got user " + id);

				// email parsing is left an excercise to the reader
				var start = content.IndexOf("<h1>") + 4;
				var end = content.IndexOf("</h1>", start);

				name = content.Substring(start, end - start).Replace("&#32;", " ");

				if (name.StartsWith("Ez a "))
				{
					name = id.ToString();
				}
				else
				{
					name = name.Replace(" bloggerina", String.Empty);
					name = name.Replace(" blogger", String.Empty);
				}
			}
		}

		#endregion
		#region AnonymousUser

		class AnonymousUser : User
		{
			private string name;
			private string email;

			public AnonymousUser(string name, string email)
			{
				this.name = name;
				this.email = email;
			}

			public override string Name { get { return name; } }
			public override string Email { get { return email; } }
		}

		#endregion
	}
}
