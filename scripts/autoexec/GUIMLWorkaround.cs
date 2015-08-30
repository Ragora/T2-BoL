// #autoload
// #name = GUIML Workaround
// #version = 3.0
// #date = November 22nd, 2012
// #category = Fix
// #author = Robert MacGregor
// #warrior = DarkDragonDX
// #email = DarkDragonDX@Hotmail.com
// #description = Repairs broken functionality with certain GUIML elements after the T2 mission editor has been initialised. And in some cases, where they misbehave for no known reason.

// Separate Package just to ensure the code is "injected" at the right time
package GUIMLInjection
{
	// Takes care of opening the F2 menu on certain server/client combinations not working properly
	function ScoreScreen::onWake(%this)
	{
		parent::onWake(%this);
		if (!isActivePackage(GUIMLWorkaround))
			activatePackage(GUIMLWorkaround);
	}
	
	// Takes care of if we just launch the editor but never use the F2 menu; clicking a link in say a server desc
	function toggleEditor(%make)
	{
		parent::toggleEditor(%make);
		if (!isActivePackage(GUIMLWorkaround))
			activatePackage(GUIMLWorkaround);
	}
};
activatePackage(GUIMLInjection);

//Seperate package to activate our new code
package GUIMLWorkaround
{
	function GuiMLTextCtrl::onURL(%this, %url)
	{
		if (getField(%url,0) $= "wwwlink")
			parent::onURL(%this, getField(%url, 1)); // Opens a web browser window as it should
		else
			commandToServer('ProcessGameLink',getField(%url, 1), getField(%url, 2), getField(%url, 3), getField(%url, 4), getField(%url, 5));
	}
};