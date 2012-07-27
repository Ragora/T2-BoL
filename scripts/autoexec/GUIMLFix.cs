// #autoload
// #name = GUIML Fix
// #version = 2.0
// #date = March 1st, 2010
// #category = Fix
// #author = Dark Dragon DX
// #warrior = DarkDragonDX
// #email = DarkDragonDX@Hotmail.com
// #description = Adds a failSafe for the <a:command> tag. Mainly for players that use mods with interactive GUIML elements.

package GUIMLFix
{
 function toggleEditor(%make)
 {
  parent::toggleEditor(%make); //Call parent function
  activatePackage(GUIMLPackage);
 }
};
activatePackage(GUIMLFix);

//Seperate package to activate our new code
package GUIMLPackage
{
 function GuiMLTextCtrl::onURL(%this, %url)
 {
 parent::onURL(%this, %url);
 commandToServer('ProcessGameLink',getField(%url,1));
 }
};
deactivatePackage(GUIMLPackage); // Just to be sure


