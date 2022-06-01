using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Extensions
{
	public static  class StringExtensions
	{
		public static bool EqualsIgnoreCase(this string lhs, string rhs)
		{
			if (lhs == null)
				return false;

			return string.Equals(lhs, rhs, StringComparison.OrdinalIgnoreCase);
		}

        public static string OrEmpty(this string src)
        {
            return src ?? string.Empty;
        }
        public static bool HasContent(this string src)
        {
            return string.IsNullOrWhiteSpace(src) == false;
        }
        public static bool HasNoContent(this string src) => !src.HasContent();

    }
}
