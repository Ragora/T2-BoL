//Component: Lootage
//Description: You can loot corpses. (w00t)

//----------------------------------------------------------------------------
// DATABLOCKS
//----------------------------------------------------------------------------
datablock ItemData(Lootbag)
{
			className     = Weapon;
			catagory      = "Misc";
			shapeFile     = "moneybag.dts";
			mass          = 1;
			elasticity    = 0.2;
			friction      = 50;
			pickupRadius  = 2;
			pickUpPrefix  = "a bag of items";
			alwaysAmbient = true;
   	        description   = "lootbag_model";

			computeCRC = false;
			emap = true;
};

//----------------------------------------------------------------------------
// BOUND FUNCTIONS
//----------------------------------------------------------------------------

//Realized this isn't required..
//function LootBag::onAdd(%this,%obj) //Force a loot check on creation.
//{
//LootBagCheckForPickUp(%obj);
//parent::onAdd(%this,%obj);
//}

//----------------------------------------------------------------------------
// FUNCTIONS
//----------------------------------------------------------------------------

function LootBagCheckForPickUp(%this) //Not sure why, loot bags won't do a T2 system pickup..
{
	cancel(%this.loop);

	if (!IsObject(%this))
		return;

	%count = MissionCleanUp.getCount();

	for(%i = 0; %i < %count; %i++)
	{
		%test = MissionCleanUp.getObject(%i);

		if (%test.getClassName() $= "Player" && %test.getState() !$= "dead" && !%test.client.isAIControlled())
		{
			%dist = vectorDist(%this.getPosition(),%test.getPosition());

			if (%dist < %this.getDatablock().pickUpRadius)
			{
				LootBagPickedUp(%this,%test.client);
				return;
			}

			%this.loop = schedule(100,0,"LootBagCheckForPickup",%this);
		}
	}
}

function LootBagPickedUp(%this,%client)
{
	%db = %client.player.getDatablock(); //The player's datablock
	%money = %this.money; //Monies?
	%numItems = %this.numItems; //Giving out items.
	%numAmmo = %this.numAmmo; //..Ammo too!
	//Does the bag have money?
	if (%money $= "")
		%hasMoney = false;
	else
		%hasMoney = true;

	if (%money !$= "")
		%client.money = %client.money + %money; //Give some monies.

	for (%i = 0; %i < %numItems; %i++)
	{
		if (%db.max[%this.item[%i]] != 0) //Don't want people in light armor running around with mortars, do we?
		{
			%client.player.setInventory(%this.item[%i],1);
			%client.player.setInventory(%this.ammo[%i],%this.ammoNum[%i]);
		}
	}
	%this.delete(); //Delete the bag.

	//Let the player know.
	switch (%hasMoney)
	{
		case 0:
			if (%numItems > 1)
				messageClient(%client,'MsgClient','You picked up a bag of items that contained %1 items.',%numitems);
			else
				messageClient(%client,'MsgClient','You picked up a bag of items that contained 1 item.');
		case 1:
			if (%numItems > 1)
				messageClient(%client,'MsgClient','You picked up a bag of items that contained $%1 and %2 items.',%money,%numitems);
			else
				messageClient(%client,'MsgClient','You picked up a bag of items that contained $%1 and 1 item.',%money);
	}
}
