using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
	public static MenuManager Instance;

	[SerializeField] Menu[] menus;

	void Awake()
	{
		Instance = this;
	}

	public void OpenMenu(string menuName)
	{
		for (int i = 0; i < menus.Length; i++)
		{
            if (menus[i] != null)
            {
				if (menus[i].menuName == menuName)
				{
					menus[i].Show();
				}
				else
				{
					CloseMenu(menus[i]);
				}
			}
		}
	}

	public void OpenMenu(Menu menu)
	{
		if (menu != null)
		{
			for (int i = 0; i < menus.Length; i++)
			{
				if (menus[i].isActive)
				{
					CloseMenu(menus[i]);
				}
			}
			menu.Show();
		}
	}

	public void CloseMenu(Menu menu)
	{
        if (menu != null)
        {
			menu.Close();
		}
	}
}
