// #autoload
// #name = Static Waypoint Fix
// #version = 1.0
// #category = Fix
// #warrior = Jsut
// #description = Fixes static waypoints in the command circuit.

package JsutStaticWaypointFix
{

	function CommanderTree::processCommand(%this, %command, %target, %typeTag)

	{

		parent::processCommand(%this, %command, %target, %typeTag);

		// special case?

		if(%typeTag < 0)
		{
			switch$(getTaggedString(%command))
			{
				// waypoints: tree owns the waypoint targets
				case "CreateWayPoint":
				%target.settext(%this.currentWaypointID);
				return;
			}
		}
	}
};

activatePackage(JsutStaticWaypointFix);




 
