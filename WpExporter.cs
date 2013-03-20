using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Fb2Wp
{
	class WpExporter : IExporter
	{
		private static class Xmlns
		{
			public static readonly XNamespace Excerpt = "http://wordpress.org/export/1.2/excerpt/";
			public static readonly XNamespace Content = "http://purl.org/rss/1.0/modules/content/";
			public static readonly XNamespace Wfw = "http://wellformedweb.org/CommentAPI/";
			public static readonly XNamespace Dc = "http://purl.org/dc/elements/1.1/";
			public static readonly XNamespace Wp = "http://wordpress.org/export/1.2/";
		}

		private readonly Entry[] entries;
		private readonly ILookup<long, Comment> comments;
		private readonly Dictionary<string, int> knownAuthors;

		public WpExporter(IEnumerable<Entry> entries, IEnumerable<Comment> comments)
		{
			this.entries = entries
							//.Skip(800)
							//.Take(100)
							.ToArray();

			// generate fake ids for authors to map their comments
			this.knownAuthors = this.entries
									.Select(e => e.Author)
									.Distinct()
									.Select((a, i) => Tuple.Create(a, i))
									.ToDictionary(t => t.Item1, t => t.Item2, StringComparer.OrdinalIgnoreCase);

			var knownEntryIds = new HashSet<long>(this.entries.Select(e => e.Id));
			this.comments = comments.Where(c => knownEntryIds.Contains(c.EntryId)).ToLookup(c => c.EntryId);
		}

		public void Write(string target)
		{
			var authors = GetAuthors();
			var channel = new XElement("channel",
							new XElement("title", "export"),
							new XElement("language", "hu-hu"),
							new XElement(Xmlns.Wp + "wxr_version", "1.2"),
							new XElement(Xmlns.Wp + "base_site_url", "http://lofasz.com"),
							new XElement(Xmlns.Wp + "base_blog_url", "http://lofasz.com"),
							new XElement("pubDate", DateTimeOffset.Now.ToString("r", CultureInfo.InvariantCulture))
						);

			channel.AddSection("AUTHOR LIST", GetAuthors());
			channel.AddSection("CATEGORY LIST", GetCategories());
			channel.AddSection("TAG LIST", GetTags());

			channel.Add(GetEntries());

			var doc = new XDocument(
				new XElement("rss",
					new XAttribute(XNamespace.Xmlns + "excerpt", Xmlns.Excerpt),
					new XAttribute(XNamespace.Xmlns + "content", Xmlns.Content),
					new XAttribute(XNamespace.Xmlns + "wfw", Xmlns.Wfw),
					new XAttribute(XNamespace.Xmlns + "dc", Xmlns.Dc),
					new XAttribute(XNamespace.Xmlns + "wp", Xmlns.Wp),

					channel
					));

			//Console.WriteLine(doc);

			doc.Save(target, SaveOptions.DisableFormatting);
		}

		private XElement[] GetAuthors()
		{
			var authors = entries.Select(e => e.Author).Distinct();

			return authors.Select((a, i) =>
						WpElement("author",
							//WpElement("author_id", i),
							WpElement("author_login", a),
							WpElement("author_email", "PUT YOUR EMAIL HERE")
						)).ToArray();
		}

		private XElement[] GetCategories()
		{
			var categories = entries.SelectMany(e => e.Categories).Distinct();

			return categories.Select((c, i) =>
						WpElement("category",
							WpElement("term_id", i),
							WpElement("category_nicename", c.Alias),
							WpElement("category_parent", String.Empty),
							WpElement("cat_name", new XCData(c.Title))
						)).ToArray();
		}

		private XElement[] GetTags()
		{
			var tags = entries.SelectMany(e => e.Tags).Distinct();

			return tags.Select((t, i) =>
						WpElement("tag",
							WpElement("term_id", i),
							WpElement("tag_slug", t.Replace(" ", "-")),
							WpElement("tag_name", new XCData(t))
						)).ToArray();
		}

		private XElement[] GetEntries()
		{
			return entries.Select(EntryToXml).ToArray();
		}

		private XElement EntryToXml(Entry e, int index)
		{
			var retval = new XElement("item",
							new XElement("title", e.Title),
							new XElement("link", "http://localhost/?p=" + index),
							new XElement("guid", "http://localhost/?p=" + index, new XAttribute("isPermalink", false)),
							new XElement("pubDate", e.Timestamp.UtcDateTime.ToString("r", CultureInfo.InvariantCulture)),
							new XElement(Xmlns.Dc + "creator", e.Author),
							new XElement("description", String.Empty),
							new XElement(Xmlns.Content + "encoded", new XCData(e.Content)),
							new XElement(Xmlns.Excerpt + "encoded", new XCData(e.Excerpt ?? String.Empty)),
							WpElement("post_id", index),
							WpElement("post_date", e.Timestamp.DateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
							WpElement("post_date_gmt", e.Timestamp.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),

							WpElement("comment_status", "open"),
							WpElement("ping_status", "open"),
							WpElement("status", "publish"),
							WpElement("post_parent", 0),
							WpElement("menu_order", 0),
							WpElement("is_sticky", 0),
							WpElement("post_type", "post"),

							WpElement("post_name", MkAlias(e.Title)));

			retval.Add(e.Categories.Select(c =>
							new XElement("category",
								new XAttribute("domain", "category"),
								new XAttribute("nicename", c.Alias),
								new XCData(c.Title)))
						.ToArray());

			retval.Add(comments[e.Id].Where(c => !String.IsNullOrEmpty(c.Content)).Select(CommentToXml).ToArray());

			retval.Add(e.Tags.Select(t =>
						new XElement("category",
							new XAttribute("domain", "tag"),
							new XAttribute("nicename", MkAlias(t)),
							new XCData(t)))
					.ToArray());

			return retval;
		}

		private XElement CommentToXml(Comment c, int index)
		{
			int authorId;
			var author = c.Author;

			if (!String.IsNullOrEmpty(author.Name))
				knownAuthors.TryGetValue(author.Name, out authorId);
			else
				authorId = 0;

			return WpElement("comment",
						WpElement("comment_id", index),
						WpElement("comment_author", c.Author.Name),
						WpElement("comment_author_email", c.Author.Email),
						WpElement("comment_author_url", String.Empty),
						WpElement("comment_author_IP", c.Ip),
						WpElement("comment_date", c.Timestamp.DateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
						WpElement("comment_date_gmt", c.Timestamp.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
						WpElement("comment_content", new XCData(c.Content)),
						WpElement("comment_approved", 1),
						WpElement("comment_type", String.Empty),
						WpElement("comment_parent", 0),
						WpElement("comment_user_id", authorId)
					);
		}

		private static string MkAlias(string input)
		{
			return String.IsNullOrEmpty(input)
					? input
					: input.Replace(" ", "-");
		}

		private static XElement WpElement(string name, params object[] content)
		{
			return new XElement(Xmlns.Wp + name, content);
		}
	}

	static class XHelpers
	{
		public static void AddSection(this XElement element, string name, params object[] content)
		{
			element.Add(new XComment("\n\t" + name + "\n\t"));
			element.Add(content);
			element.Add(new XComment("\n\n\t"));
		}
	}
}
