using UnityEngine;
using UnityEngine.UI;

namespace ShipEditor.UI
{
	public class NameValueItem : MonoBehaviour
	{
		public Text Label;
		public Text Value;

		public Color Color
		{
			set
			{
				//Label.color = new Color(value.r, value.g, value.b, Label.color.a);
				Value.color = new Color(value.r, value.g, value.b, Value.color.a);
			}
		}
	}
}
