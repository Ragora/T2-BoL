// --------------------------------------------------------
// A script that allows one to buy property.
// The script is in a BETA state, so it may have bugs.
//
// TODO:
// Make the script take rotation into consideration..
// Find a way to 'purchase' interiors
// --------------------------------------------------------

function InteriorInstance::buyObject(%this,%objectID,%client,%team)
{
	if (%this.generatorCount $= "")
		%this.generatorCount = 0;
	if (%this.inventoryCount $= "")
		%this.inventoryCount = 0;
	if (%this.sensorCount $= "")
		%this.sensorCount = 0;
	if (%this.sentryCount $= "")
		%this.sentryCount = 0;
	if (%this.bannerCount $= "")
		%this.bannerCount = 0;
	if (%this.turretBaseCount $= "")
		%this.turretBaseCount = 0;

	switch(%objectID)
	{
		case 0: //Generator
			if (%this.generatorCount == $Property::Max[%this.interiorFile,0])
				return false;

			%shape = new StaticShape()
			{
				DataBlock = GeneratorLarge;
				Position = vectorAdd($Property::Offset[%this.interiorFile,0,%this.generatorCount],%this.getPosition());
				Rotation = $Property::Rotation[%this.interiorFile,0,%this.generatorCount];
				Team = %team;
			};
	 
			GeneratorLarge.gainPower(%shape);
			%this.generatorCount++;
 
		case 1: //Inventory
			if (%this.generatorCount == 0 || %this.inventoryCount == $Property::Max[%this.interiorFile,1]) //Don't create if there's no generators
				return false;
 
			%shape = new StaticShape()
			{
				DataBlock = StationInventory;
				Position = vectorAdd($Property::Offset[%this.interiorFile,1,%this.inventoryCount],%this.getPosition());
				Rotation = $Property::Rotation[%this.interiorFile,1,%this.inventoryCount];
				Team = %team;
			};
 
			StationInventory.gainPower(%shape);
			%this.inventoryCount++;
 
		case 2: //Sensor (Medium)
			if (%this.generatorCount == 0 || %this.sensorCount == $Property::Max[%this.interiorFile,2])
				return false;

			%shape = new StaticShape()
			{
				DataBlock = SensorMediumPulse;
				Position = vectorAdd($Property::Offset[%this.interiorFile,2,%this.sensorCount],%this.getPosition());
				Rotation = $Property::Rotation[%this.interiorFile,2,%this.sensorCount];
				Team = %team;
			};
			SensorMediumPulse.gainPower(%shape);
			%this.sensorCount++;
 
		case 3: //Sensor (Large)
		 if (%this.generatorCount == 0 || %this.sensorCount == $Property::Max[%this.interiorFile,2])
			return false;

		%shape = new StaticShape()
		{
			DataBlock = SensorLargePulse;
			Position = vectorAdd($Property::Offset[%this.interiorFile,3,%this.sensorCount],%this.getPosition());
			Rotation = $Property::Rotation[%this.interiorFile,3,%this.sensorCount];
			Team = %team;
		};

		SensorLargePulse.gainPower(%shape);
		%this.sensorCount++;
 
		case 4: //Sentry Turrets
			if (%this.generatorCount == 0 || %this.sentryCount == $Property::Max[%this.interiorFile,4])
				return false;

			%shape = new StaticShape()
			{
				DataBlock = SentryTurret;
				Position = vectorAdd($Property::Offset[%this.interiorFile,4,%this.sentryCount],%this.getPosition());
				Rotation = $Property::Rotation[%this.interiorFile,4,%this.sentryCount];
				Team = %team;
			};
			SentryTurret.gainPower(%shape);
			%this.sentryCount++;
			
		case 5: //Banner (Strength)
			if (%this.bannerCount == $Property::Max[%this.interiorFile,5])
				return false;
			%shape = new StaticShape()
			{
				DataBlock = Banner_Strength;
				Position = vectorAdd($Property::Offset[%this.interiorFile,5,%this.bannerCount],%this.getPosition());
				Rotation = $Property::Rotation[%this.interiorFile,5,%this.bannerCount];
				Team = %team;
			};
			%this.bannerCount++;
 
		case 6: //Large Turret Base
		if (%this.generatorCount == 0 || %this.turretBaseCount == $Property::Max[%this.interiorFile,6])
			return false;

		%shape = new StaticShape()
		{
			DataBlock = TurretBaseLarge;
			Position = vectorAdd($Property::Offset[%this.interiorFile,6,%this.turretBaseCount],%this.getPosition());
			Rotation = $Property::Rotation[%this.interiorFile,6,%this.turretBaseCount];
			Team = %team;
		};

		%this.turretBaseCount++;
	}

	%this.getGroup().add(%shape);
	setTargetName(%shape.target,addTaggedString(%client.namebase @ "'s"));
	setTargetSensorGroup(%shape.getTarget(), %team);
	%shape.setSelfPowered();
	return %shape;
}

function staticShape::setPosition(%this,%pos)
{
	%this.setTransform(%pos);
	return %this;
}

function staticShape::getRotation(%this)
{
	%trans = %this.getTransform();
	return getWord(%trans, 3) SPC getWord(%trans, 4) SPC getWord(%trans,5);
}

function objectIDToDatablock(%objectID)
{
	switch(%objectID)
	{
		case 0: return "GeneratorLarge";
		case 1: return 0;
		default: return -1;
	}
	return -1;
}


//This the array that stores all the positions and rotations for purchases of objects. I'll eventually move this to be a part of the basicFileProcessing.
//Beagle Tower (bbunk2)
//Generators
$Property::Offset["bbunk2.dif",0,0] = "0.136109 6.92569 3.80877";
$Property::Rotation["bbunk2.dif",0,0] = "1 0 0 0";
//Inventory
$Property::Offset["bbunk2.dif",1,0] = "-13.5045 6.57603 -6.49712";
$Property::Rotation["bbunk2.dif",1,0] = "0 0 -1 88.8085";
$Property::Offset["bbunk2.dif",1,1] = "13.5045 6.57603 -6.49712";
$Property::Rotation["bbunk2.dif",1,1] = "0 0 1 88.8085";
//Medium Sensors
$Property::Offset["bbunk2.dif",2,0] = "-0.0187805 3.42132 30.8251";
$Property::Rotation["bbunk2.dif",2,0] = "1 0 0 0";
//Large Sensors
$Property::Offset["bbunk2.dif",3,0] = "-0.0187805 3.42132 30.8251";
$Property::Rotation["bbunk2.dif",3,0] = "1 0 0 0";
//Sentry Turrets
$Property::Offset["bbunk2.dif",4,0] = "0.018325 -0.513021 9.99179";
$Property::Rotation["bbunk2.dif",4,0] = "0.706825 0.707389 0.000371874 179.92";
$Property::Offset["bbunk2.dif",4,1] = "-0.092863 10.5404 -0.443447";
$Property::Rotation["bbunk2.dif",4,1] = "0.577209 -0.577449 -0.577392 119.938";
//Banners (Strength)
$Property::Offset["bbunk2.dif",5,0] = "-0.150952 9.53516 9.82968";
$Property::Rotation["bbunk2.dif",5,0] = "0 0 1 179.909";
//Large Turret Base
$Property::Offset["bbunk2.dif",6,0] = "0.0332212 11.5991 27.9961";
$Property::Rotation["bbunk2.dif",6,0] = "1 0 0 0";

//Max values for each interior
$Property::Max["bbunk2.dif",0] = 1; //Max generators
$Property::Max["bbunk2.dif",1] = 2; //Max Inventories
$Property::Max["bbunk2.dif",2] = 1; //Max Medium Sensors
$Property::Max["bbunk2.dif",3] = 1; //Max Large Sensors
$Property::Max["bbunk2.dif",4] = 2; //Max Sentry Turrets
$Property::Max["bbunk2.dif",5] = 1; //Max Banners (Strength)
$Property::Max["bbunk2.dif",6] = 1; //Max Turret Bases
