using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fb2Wp
{
	abstract class User
	{
		public abstract string Name { get; }
		public abstract string Email { get; }
	}

	class Category: IEquatable<Category>
	{
		public long Id;
		public string Title;
		public string Alias;

		public bool Equals(Category other)
		{
			return this.Id == other.Id;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}

	class Entry
	{
		public long Id;
		public string Title;
		public DateTimeOffset Timestamp;

		public string Content;
		public string Excerpt;
		public string Author;

		public List<Category> Categories;
		public List<string> Tags;
	}

	class Comment
	{
		public long Id;
		public long EntryId;
		public User Author;

		public string Content;
		public DateTimeOffset Timestamp;
		public string Ip;
	}
}
