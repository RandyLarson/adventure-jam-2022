using TMPro;

namespace Assets.Scripts.Extensions
{
	public static  class TextMeshProExtensions
	{
		public static void SafeSetText<T>(this TextMeshProUGUI target, T content)
		{
			if (target != null)
			{
				target.text = content?.ToString() ;
			}
		}
	}
}
