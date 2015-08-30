//------------------------------------------------------------------------------
// A lil sumthin by Alviss

// Datablock script with all the building datablocks.
//------------------------------------------------------------------------------
//      Datablock
//------------------------------------------------------------------------------
datablock StaticShapeData(BuildingBlock0) : StaticShapeDamageProfile
{
	className = "BuildingPiece";
	shapeFile = "Pmiscf.dts";

	maxDamage      = 20;
	destroyedLevel = 20;
	disabledLevel  = 19;

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
	targetNameTag = 'Building Piece';
	deployAmbientThread = true;
	debrisShapeName = "debris_generic_small.dts";
	debris = DeployableDebris;
	heatSignature = 0;
	needsPower = false;
};

datablock StaticShapeData(BuildingBlock1) : BuildingBlock0
{
 	shapeFile = "smiscf.dts";
};

datablock StaticShapeData(BuildingBlock2) : BuildingBlock0
{
 	shapeFile =  "stackable2l.dts";
};

datablock StaticShapeData(BuildingBlock3) : BuildingBlock0
{
   shapeFile = "stackable2m.dts";
};

datablock StaticShapeData(BuildingBlock4) : BuildingBlock0
{
   shapeFile = "stackable3l.dts";
};

datablock StaticShapeData(BuildingBlock5) : BuildingBlock0
{
	shapeFile = "stackable4m.dts";
};

datablock StaticShapeData(BuildingBlock6) : BuildingBlock0
{
	shapeFile = "Xmiscf.dts";
};

//--------------------
// Hack to turn item names into the Type names
// We won't need them once all the buildings are converted.
//

// Words like, Deployable, BasePack, Turret
// are all removed from the item name, and then referenced the this table.

//$CCItemToType[PowerPlant] = "PowerPlant";
//$CCItemToType[Barracks] = "Barracks";
//$CCItemToType[TiberiumRefinery] = "Tiberium";
//$CCItemToType[CommCenter] = "CommCenter";
//$CCItemToType[GTower] = "GuardTower";
//$CCItemToType[WeaponsFactory] = "WarFact";
//$CCItemToType[GunTurret] = "Gun"; // < lol
//$CCItemToType[NPA] = "LaserBatt";
//$CCItemToType[AdvPowerPlant] = "AdvPowerPlant";
//$CCItemToType[EnrichGenerator] = "Enrich";
//$CCItemToType[AdvGuard] = "AdvGuard";
//$CCItemToType[AABat] = "AABat";
//$CCItemToType[IonControl] = "IonControl";
//$CCItemToType[Obelisk] = "Obelisk";
//$CCItemToType[SAM] = "SAMSite";
//$CCItemToType[TempOfNod] = "TempleOfNod";
//$CCItemToType[PEC] = "ParticleEC";

$CCItemToType[PowerPlantDeployable] = "PowerPlant";
$CCItemToType[BarracksDeployable] = "Barracks";
$CCItemToType[TiberiumRefinery] = "Tiberium";
$CCItemToType[CommCenterDeployable] = "CommCenter";
$CCItemToType[GTowerTurretBasePack] = "GuardTower";
$CCItemToType[WeaponsFactoryDeployable] = "WarFact";
$CCItemToType[TurretBasePack] = "Gun"; // < lol
$CCItemToType[NPATurretBasePack] = "LaserBatt";
$CCItemToType[AdvPowerPlantDeployable] = "AdvPowerPlant";
$CCItemToType[EnrichGeneratorDeployable] = "Enrich";
$CCItemToType[AdvGuardTurretBasePack] = "AdvGuard";
$CCItemToType[AABatTurretBasePack] = "AABat";
$CCItemToType[IonControlDeployable] = "IonControl";
$CCItemToType[ObeliskTurretBasePack] = "Obelisk";
$CCItemToType[SAMTurretBasePack] = "SAMSite";
$CCItemToType[TempleOfNod] = "TempleOfNod";
$CCItemToType[PECTurretBasePack] = "ParticleEC";

$CCTypeToItem[PowerPlant] = "PowerPlantDeployable";
$CCTypeToItem[Barracks] = "BarracksDeployable";
$CCTypeToItem[Tiberium] = "TiberiumRefineryDeployable";
$CCTypeToItem[CommCenter] = "CommCenterDeployable";
$CCTypeToItem[GuardTower] = "GTowerTurretBasePack";
$CCTypeToItem[WarFact] = "WeaponsFactoryDeployable";
$CCTypeToItem[Gun] = "TurretBasePack"; // < lol
$CCTypeToItem[LaserBatt] = "NPATurretBasePack";
$CCTypeToItem[AdvPowerPlant] = "AdvPowerPlantDeployable";
$CCTypeToItem[Enrich] = "EnrichGeneratorDeployable";
$CCTypeToItem[AdvGuard] = "AdvGuardTurretBasePack";
$CCTypeToItem[AABat] = "AABatTurretBasePack";
$CCTypeToItem[IonControl] = "IonControlDeployable";
$CCTypeToItem[Obelisk] = "ObeliskTurretBasePack";
$CCTypeToItem[SAMSite] = "SAMTurretBasePack";
$CCTypeToItem[TempOfNod] = "TempleOfNod";
$CCTypeToItem[ParticleEC] = "PECTurretBasePack";



