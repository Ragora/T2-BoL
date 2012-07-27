//---------------------------------------------------------
// Deployable Spine, Code by Mostlikely, Prettied by JackTL
//---------------------------------------------------------

$TeamDeployableMax[constructorDeployable] = 1;

datablock ShapeBaseImageData(constructorDeployableImage) {
 mass = 1;
	emap = true;
	shapeFile = "ammo_plasma.dts";
	item = constructorDeployable;
	mountPoint = 1;
	offset = "0 -0.2 -0.55";
	deployed = DeployedSpine;
	heatSignature = 0;

	stateName[0] = "Idle";
	stateTransitionOnTriggerDown[0] = "Activate";

	stateName[1] = "Activate";
	stateScript[1] = "onActivate";
	stateTransitionOnTriggerUp[1] = "Idle";

	isLarge = false;
	maxDepSlope = 360;
	deploySound = ItemPickupSound;

	minDeployDis = 0.1;
	maxDeployDis = 50.0;
};

datablock ItemData(constructorDeployable) {
	className = Pack;
	catagory = "Deployables";
	shapeFile = "stackable1s.dts";
     mass = 1;
	elasticity = 0.2;
	friction = 0.6;
	pickupRadius = 1;
	rotate = true;
	image = "constructorDeployableImage";
	pickUpName = "a light support beam pack";
	heatSignature = 0;
	emap = true;
};

function constructorDeployableImage::testObjectTooClose(%item) {
	return "";
}

function constructorDeployableImage::testNoTerrainFound(%item) {
	// don't check this for non-Landspike turret deployables
}

function constructorDeployable::onPickup(%this, %obj, %shape, %amount) {
	// created to prevent console errors
}

function constructorDeployableImage::onDeploy(%item, %plyr, %slot) {
	//Object
	%className = "StaticShape";

	%grounded = 0;
	if (%item.surface.getClassName() $= TerrainBlock)
		%grounded = 1;

	%playerVector = vectorNormalize(-1 * getWord(%plyr.getEyeVector(),1) SPC getWord(%plyr.getEyeVector(),0) SPC "0");

	if (%item.surfaceinher == 0) {
		if (vAbs(floorVec(%item.surfaceNrm,100)) $= "0 0 1")
			%item.surfaceNrm2 = %playerVector;
		else
			%item.surfaceNrm2 = vectorNormalize(vectorCross(%item.surfaceNrm,"0 0 1"));
	}

	%item.surfaceNrm3 = vectorCross(%item.surfaceNrm,%item.surfaceNrm2);

	%rot = fullRot(%item.surfaceNrm,%item.surfaceNrm2);
	%scale = getWords($packSetting["spine",%plyr.packSet],0,2);
	%dataBlock = %item.deployed;

	if (%plyr.packSet == 5) {
		%space = rayDist(%item.surfacePt SPC %item.surfaceNrm,%scale,$AllObjMask);
		if (%space != getWord(%scale,1))
			%type  = true;
		%scale = getWord(%scale,0) SPC getWord(%scale,0) SPC %space;
	}
	else if (%plyr.packSet == 6 || %plyr.packSet == 7) {
		%mCenter = "0 0 -0.5";
		%pad = pad(%item.surfacePt SPC %item.surfaceNrm SPC %item.surfaceNrm2,%scale,%mCenter);
		%scale = getWords(%pad,0,2);
		%item.surfacePt = getWords(%pad,3,5);
		%rot = getWords(%pad,6,9);
		if (%plyr.packSet == 7) {
			%scale = vectorMultiply(%scale,1.845 SPC 2 SPC 1.744); // Update dfunctions.cs if changed!
			%scaleIsSet = 1;
			%item.surfacePt = vectorAdd(%item.surfacePt,vectorScale(%item.surfaceNrm3,0.5));
			%rot = rotAdd(%rot,"1 0 0" SPC $Pi);
			%dataBlock = "DeployedWoodSpine";
		}
	}

	if (!%scaleIsSet)
		%scale = vectorMultiply(%scale,1/4 SPC 1/3 SPC 2);

    //Test pack -- make sure our structure is loaded
    exec("data/Game/Structures/SMLCabin.cs");
    build_SMLCabin(%plyr.client,%item.surfacePt,%plyr.client.team);

	// play the deploy sound
	serverPlay3D(%item.deploySound, %deplObj.getTransform());
 
    %plyr.setInventory("constructorDeployable",0,1);

	return %deplObj;
}

/////////////////////////////////////

function constructorDeployableImage::onMount(%data, %obj, %node) {
	%obj.hasSpine = true; // set for spinecheck
	%obj.packSet = 0;
	displayPowerFreq(%obj);
}

function constructorDeployableImage::onUnmount(%data, %obj, %node) {
	%obj.hasSpine = "";
	%obj.packSet = 0;
}
