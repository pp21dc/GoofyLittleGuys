using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
	public class GameManager : SingletonBase<GameManager>
	{
		[SerializeField] private const float maxGameTime = 10f;
		[SerializeField] private const float phaseOneStartTime = 7f;
		[SerializeField] private float currentGameTime = 0;
	}

}