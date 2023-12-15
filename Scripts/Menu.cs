using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
	public string menuName;
	public bool isActive;

	public void Show()
	{
		isActive = true;
		gameObject.SetActive(true);
	}

	public void Close()
	{
		isActive = false;
		gameObject.SetActive(false);
	}
}
