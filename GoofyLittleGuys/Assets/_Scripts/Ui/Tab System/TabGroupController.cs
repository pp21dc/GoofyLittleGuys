using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TabGroupController : MonoBehaviour
{
	[SerializeField] private List<TabObject> tabs;
	[SerializeField] private GameObject ribbonButton;

	[Header("Input")]
	[SerializeField] private InputActionReference navigateRibbon; // UI/NavigateRibbon input action

	private int currentTab = 0;

	private void OnEnable()
	{
		currentTab = 0;
		ActivateTab(currentTab);
		if (navigateRibbon != null)
		{
			navigateRibbon.action.performed += OnNavigateRibbon;
			navigateRibbon.action.Enable();
		}
	}

	private void OnDisable()
	{
		if (navigateRibbon != null)
		{
			navigateRibbon.action.performed -= OnNavigateRibbon;
			navigateRibbon.action.Disable();
		}
		DeactivateTab(currentTab);
	}

	private void OnNavigateRibbon(InputAction.CallbackContext ctx)
	{
		float dir = ctx.ReadValue<float>();

		if (dir > 0f)
			OnRB();
		else if (dir < 0f)
			OnLB();
	}

	public void OnRB()
	{
		Managers.UiManager.Instance.PlayButtonHighlightSfx();
		DeactivateTab(currentTab);
		currentTab = (currentTab + 1) % tabs.Count;
		ActivateTab(currentTab);

		Managers.DebugManager.Log($"Tab switched to: {currentTab}", Managers.DebugManager.DebugCategory.UI);
		
	}

	public void OnLB()
	{
		Managers.UiManager.Instance.PlayButtonHighlightSfx();
		DeactivateTab(currentTab);
		currentTab = (currentTab - 1 + tabs.Count) % tabs.Count;
		ActivateTab(currentTab);

		Managers.DebugManager.Log($"Tab switched to: {currentTab}", Managers.DebugManager.DebugCategory.UI);
	}

	private void ActivateTab(int index) => tabs[index].ActivateTab();
	private void DeactivateTab(int index) => tabs[index].DeactivateTab();
}
