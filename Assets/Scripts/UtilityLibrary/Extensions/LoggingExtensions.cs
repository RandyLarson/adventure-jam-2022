using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Extensions
{
	public static class LoggingExtensions
	{
		public static string LogValue(this string src)
		{
			return src ?? "(null)";
		}
	}
}
