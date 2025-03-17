using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public static class HapticFeedback
{
	public static void PlayJoinHaptics(PlayerInput input, int playerIndex, float pulseDuration = 0.1f, float pauseDuration = 0.2f)
	{
		if (input.currentControlScheme != "Gamepad") return; // Skip if not using a gamepad

		Gamepad gamepad = input.devices[0] as Gamepad;
		if (gamepad != null)
		{
			CoroutineRunner.Instance.StartCoroutine(HapticPulseRoutine(gamepad, playerIndex, pulseDuration, pauseDuration));
		}
	}

	public static void PlayHapticFeedback(PlayerInput input, float lowFrequency, float highFrequency, float duration)
	{
		if (input.currentControlScheme != "Gamepad") return; // Skip if not using a gamepad

		Gamepad gamepad = input.devices[0] as Gamepad;
		if (gamepad != null)
		{
			CoroutineRunner.Instance.StartCoroutine(HapticFeedbackRoutine(gamepad, lowFrequency * SettingsManager.Instance.GetSettings().rumbleAmount, highFrequency * SettingsManager.Instance.GetSettings().rumbleAmount, duration));
		}
	}

	private static IEnumerator HapticFeedbackRoutine(Gamepad gamepad, float lowFrequency, float highFrequency, float duration)
	{
		gamepad.ResumeHaptics();
		gamepad.SetMotorSpeeds(lowFrequency, highFrequency); // Custom vibration values
		yield return new WaitForSecondsRealtime(duration);

		// Ensure vibration stops after the duration
		gamepad.ResetHaptics();
		gamepad.SetMotorSpeeds(0, 0);
	}

	private static IEnumerator HapticPulseRoutine(Gamepad gamepad, int pulseCount, float pulseDuration, float pauseDuration)
	{
		float rumbleAmount = Mathf.Min(0.1f, 0.75f * SettingsManager.Instance.GetSettings().rumbleAmount);
		for (int i = 0; i < pulseCount; i++)
		{
			gamepad.ResumeHaptics();
			gamepad.SetMotorSpeeds(rumbleAmount, rumbleAmount); // Start vibration
			yield return new WaitForSecondsRealtime(pulseDuration);

			gamepad.PauseHaptics();
			gamepad.SetMotorSpeeds(0, 0); // Stop vibration manually
			yield return new WaitForSecondsRealtime(0.05f); // **Small delay to ensure stop registers**

			if (i < pulseCount - 1) yield return new WaitForSecondsRealtime(pauseDuration);
		}

		// **Final failsafe to ensure it really stops**
		yield return new WaitForSecondsRealtime(0.1f);
		gamepad.ResetHaptics();
		gamepad.SetMotorSpeeds(0, 0);
	}
}
