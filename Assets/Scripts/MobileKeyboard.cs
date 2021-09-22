#if !(UNITY_STANDALONE || UNITY_WEBPLAYER)
using UnityEngine;

public static class MobileKeyboard 
{	
	private static string keyBoardText = "";
	
	public static TouchScreenKeyboard keyboard;
	private static bool isOpen = false;
	private static bool isDone = false;

	public static void Open()
	{
		if (!isOpen) 
		{
			TouchScreenKeyboard.hideInput = false;
			keyBoardText = "";
			keyboard = TouchScreenKeyboard.Open("", 
			                                    TouchScreenKeyboardType.Default, 
			                                    false, //autocorrect
			                                    false, //multiline
			                                    false, //secure 
			                                    false, //alert
			                                    "" //text placeholder
			                                    );
			isOpen = true;
			isDone = false;
		}
	}

	public static void update()
	{
		if(isOpen)
		{
			if(keyboard.wasCanceled)
			{
				isDone = true;
				keyBoardText = "";
				isOpen = false;
			}
			else if(keyboard.done) 
			{
				isDone = true;
				keyBoardText = keyboard.text;
				isOpen = false;
			}
			else
			{
				keyBoardText = keyboard.text;
			}
		}
	}

	/*
	public delegate void CharacterReturn(char c);
	public static void updateCharacterMode(Filter filterKeyboard, CharacterReturn characterReturn)
	{
		if(isOpen)
		{
			if(keyboard.wasCanceled)
			{
				isDone = true;
				keyBoardText = "";
				isOpen = false;
			}
			else if(keyboard.done) 
			{
				isDone = true;
				isOpen = false;
			}
			else
			{

				filterKeyboard();
				if(keyboard.text != keyBoardText)
				{
					if(keyboard.text.Length < 1)
					{
						//delete occured
						characterReturn('\b');
					}
					else
					{
						//last letter is the one added
						characterReturn(keyboard.text.ToCharArray()[0]);
					}
					keyBoardText = "";
					keyboard.text = "";
				}
			}
		}
	}
	*/
	public static string GetText()
	{
		return keyBoardText;
	}

	public static bool IsDone()
	{
		return isDone;
	}

	public static bool IsOpen()
	{
		return isOpen;
	}
}
#endif