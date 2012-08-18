// #autoload
// #name = Terrain UE Prevent
// #version = 1.0
// #date = Saturday, July 25, 2009
// #category = Fix
// #author = Dark Dragon DX
// #warrior = DarkDragonDX
// #email = DarkDragonDX@Hotmail.com
// #description = A "prevention" for when you open the command Circuit on a map without terrain and UE. (Like ANTS)

package TerrainFix
{
	function toggleCommanderMap(%val)
	{
		%count = ServerConnection.getCount();

		for (%i = 0; %i < %count; %i++) //Search for my terrain..
		{
			%obj = ServerConnection.getObject(%i); //Get the object

			if (%obj != -1) //No console spam
				if (%obj.getClassName() $= "TerrainBlock") //Is it terrain?
				{
					parent::toggleCommanderMap(%val); //Win
					return true;
				}
		}
		messageBoxOk("Error","Unable to open command circuit, no terrain exists."); //Tell me
		return false;
	}
};
activatePackage(TerrainFix);

