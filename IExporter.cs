using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fb2Wp
{
	interface IExporter
	{
		void Write(string target);
	}
}
