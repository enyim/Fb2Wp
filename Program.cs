using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Fb2Wp
{
	class Program
	{
		#region Services

		static Dictionary<string, Func<string, IImporter>> importers = new Dictionary<string, Func<string, IImporter>>
		{
			{ "fb", (path) => new FreeblogImporter(path) }
		};

		static Dictionary<string, Func<IEnumerable<Entry>, IEnumerable<Comment>, IExporter>> exporters = new Dictionary<string, Func<IEnumerable<Entry>, IEnumerable<Comment>, IExporter>>
		{
			{ "wp", (e, c) => new WpExporter(e, c) }
		};

		#endregion

		static void Help()
		{
			Console.WriteLine("Fb2Wp <fb export directory> <wp output path>");
			Console.WriteLine();
		}

		static void Main(string[] args)
		{
			if (args.Length != 2
				|| !Directory.Exists(args[0]))
			{
				Help();
				return;
			}

			var source = args[0];
			var target = args[1];

			var importer = importers["fb"](source);

			var entries = importer.GetEntries();
			var comments = importer.GetComments();

			var exporter = exporters["wp"](entries, comments);

			new WpExporter(entries, comments).Write(target);
		}

	}
}
