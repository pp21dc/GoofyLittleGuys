using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class ScrollableDropdown : TMP_Dropdown
{
	private ScrollRect scrollRect;
	private RectTransform activeContent;
	private RectTransform activeViewport;


	protected override GameObject CreateBlocker(Canvas rootCanvas)
	{
		var blocker = base.CreateBlocker(rootCanvas);
		StartCoroutine(ScrollToSelectedInClonedList());
		return blocker;
	}

	private IEnumerator ScrollToSelectedInClonedList()
	{
		yield return null;
		yield return null;

		// Find the runtime-generated Dropdown List
		GameObject dropdownList = GameObject.Find("Dropdown List");
		if (dropdownList == null)
		{
			//Debug.LogWarning("[Dropdown] Couldn't find Dropdown List.");
			yield break;
		}

		scrollRect = dropdownList.GetComponentInChildren<ScrollRect>();
		if (scrollRect != null)
		{
			activeContent = scrollRect.content;
			activeViewport = scrollRect.viewport;
		}
		else
		{
			//Debug.LogWarning("[Dropdown] No ScrollRect or content found in instantiated Dropdown.");
			yield break;
		}


		RectTransform content = scrollRect.content;

		if (value < 0 || value >= content.childCount)
		{
			//Debug.LogWarning("[Dropdown] Selected index out of range.");
			yield break;
		}

		Transform selected = content.GetChild(value);

		Canvas.ForceUpdateCanvases();
		LayoutRebuilder.ForceRebuildLayoutImmediate(content);

		float viewportHeight = scrollRect.viewport.rect.height;
		float selectedPosY = Mathf.Abs(selected.localPosition.y);
		float contentHeight = content.rect.height;

		float normalizedY = Mathf.Clamp01(selectedPosY / (contentHeight - viewportHeight));
		scrollRect.verticalNormalizedPosition = 1f - normalizedY;

		//Debug.Log($"[Dropdown] Scrolled to index {value} in runtime Dropdown List.");
	}

	private void LateUpdate()
	{
		if (scrollRect == null || activeContent == null || activeViewport == null)
			return;

		GameObject selectedObj = EventSystem.current.currentSelectedGameObject;
		if (selectedObj == null || selectedObj.transform.parent != activeContent)
			return;

		RectTransform selected = selectedObj.transform as RectTransform;

		// Convert bounds of selected into viewport space
		Vector3[] itemWorldCorners = new Vector3[4];
		Vector3[] viewportCorners = new Vector3[4];

		selected.GetWorldCorners(itemWorldCorners);
		activeViewport.GetWorldCorners(viewportCorners);

		float itemTop = itemWorldCorners[1].y;
		float itemBottom = itemWorldCorners[0].y;
		float viewTop = viewportCorners[1].y;
		float viewBottom = viewportCorners[0].y;

		// Scroll up if item is above viewport
		if (itemTop > viewTop)
		{
			scrollRect.verticalNormalizedPosition += 0.1f;
		}
		// Scroll down if item is below viewport
		else if (itemBottom < viewBottom)
		{
			scrollRect.verticalNormalizedPosition -= 0.1f;
		}
	}

}
