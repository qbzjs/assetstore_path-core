namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Lightbox.
	/// Display modal image.
	/// </summary>
	[RequireComponent(typeof(Image))]
	[RequireComponent(typeof(RectTransform))]
	[DisallowMultipleComponent]
	[AddComponentMenu("UI/New UI Widgets/Lightbox")]
	public class Lightbox : MonoBehaviour
	{
		/// <summary>
		/// The modal ID.
		/// </summary>
		protected InstanceID? ModalKey;

		/// <summary>
		/// Hide on modal click.
		/// </summary>
		[SerializeField]
		public bool HideOnModalClick = true;

		/// <summary>
		/// Display specified image.
		/// </summary>
		/// <param name="sprite">Image to display.</param>
		/// <param name="modalSprite">Modal background sprite.</param>
		/// <param name="modalColor">Modal background color.</param>
		/// <param name="canvas">Canvas.</param>
		public virtual void Show(
			Sprite sprite,
			Sprite modalSprite = null,
			Color? modalColor = null,
			Canvas canvas = null)
		{
			var image = GetComponent<Image>();
			image.sprite = sprite;
			image.preserveAspect = true;

			var parent = (canvas != null) ? canvas.transform as RectTransform : UtilitiesUI.FindTopmostCanvas(gameObject.transform);
			if (parent != null)
			{
				transform.SetParent(parent, false);
			}

			if (modalColor == null)
			{
				modalColor = new Color(0, 0, 0, 0.8f);
			}

			ModalKey = ModalHelper.Open(this, modalSprite, modalColor, ProcessModalClick, parent);

			transform.SetAsLastSibling();

			gameObject.SetActive(true);
		}

		/// <summary>
		/// Process modal click.
		/// </summary>
		protected void ProcessModalClick()
		{
			if (HideOnModalClick)
			{
				Close();
			}
		}

		/// <summary>
		/// Close lightbox.
		/// </summary>
		public virtual void Close()
		{
			gameObject.SetActive(false);

			if (ModalKey.HasValue)
			{
				ModalHelper.Close(ModalKey.Value);
			}
		}
	}
}