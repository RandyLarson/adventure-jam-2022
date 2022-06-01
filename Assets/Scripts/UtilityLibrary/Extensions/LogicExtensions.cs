using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Extensions
{
	public static class LogicExtensions
	{
		public static void iff<T>(this T src, Action<T> fn)
		{
			if (src != null)
				fn(src);
		}
	}
}
