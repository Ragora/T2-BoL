//Component: Destructable Props
//Description: Not much to describe.. stuff that blows up or can be broken.

//----------------------------------------------------------------------------
// DATABLOCKS
//----------------------------------------------------------------------------
datablock StaticShapeData(DetructableSecurityCamera) : StaticShapeDamageProfile
{
    className = "SecurityCamera";
    shapeFile = "SecurityCamera.dts";
    maxDamage = 2.0;
    destroyedLevel = 2.0;
    disabledLevel = 2.0;
    mass = 1.2;
    elasticity = 0.1;
    friction = 0.9;
    collideable = 1;
    pickupRadius = 1;
    sticky = false;

    explosion      = CameraGrenadeExplosion;
    expDmgRadius = 1.0;
    expDamage = 0.1;
    expImpulse = 200.0;
    dynamicType = $TypeMasks::StaticShapeObjectType;
    deployedObject = true;
    cmdCategory = "Misc";
    cmdIcon = CMDSensorIcon;

    targetNameTag = 'Security';
    targetTypeTag = 'Camera';
    deployAmbientThread = true;
    debrisShapeName = "debris_generic_small.dts";
    debris = SmallShapeDebris;
    heatSignature = 0;
   	needsPower = true;
};

datablock StaticShapeData(DeployedSpine) : StaticShapeDamageProfile {
	className = "spine";
	shapeFile = "Pmiscf.dts";

	maxDamage      = 0.5;
	destroyedLevel = 0.5;
	disabledLevel  = 0.3;

	isShielded = true;
	energyPerDamagePoint = 240;
	maxEnergy = 50;
	rechargeRate = 0.25;

	explosion    = HandGrenadeExplosion;
	expDmgRadius = 3.0;
	expDamage    = 0.1;
	expImpulse   = 200.0;

	dynamicType = $TypeMasks::StaticShapeObjectType;
	deployedObject = true;
	cmdCategory = "DSupport";
	cmdIcon = CMDSensorIcon;
	cmdMiniIconName = "commander/MiniIcons/com_deploymotionsensor";
	targetNameTag = 'Light Support Beam';
	deployAmbientThread = true;
	debrisShapeName = "debris_generic_small.dts";
	debris = DeployableDebris;
	heatSignature = 0;
	needsPower = true;
};

//----------------------------------------------------------------------------
// FUNCTIONS
//----------------------------------------------------------------------------
function DetructableSecurityCamera::onDestroyed(%this, %obj)
{
	schedule(1000,0,"delete",%obj);
}
