//------------------------------
// Construction Mod save file to C&C Building file converter script.
// A lil sumthin by Alviss
// 24/06/09
//------------------------------

function ccConvertSave(%sender,%args)
{
// %name = "Conv_test";
 //%Fullname = "Convertion_test";
 
 %name = GetWord(%args, 0);
 %fullname = GetWords(%args, 1);

 %obj = %sender.player.SightObject(100);

 // this is our flag to set as the bottom-most piece
 %obj.TheFloor = 1;

 ConvertSave(%sender, %name, %Fullname, %obj);
}


//----------------------------------------------------
// function to convert all the pieces in a server into a new save format
function ConvertSave(%client, %name, %Fullname, %floor)
{
 // %floor is the bottom-msot piece

 %start = %floor.getPosition();
 %tHeight = getTerrainHeight(%start);
 %start = GetWords(%start, 0, 1) SPC 0;

 %nf = new fileobject() {};
 %nf.OpenforWrite(%Fullname@".cs");

 %nf.WriteLine("//------------------------------------------------------------------------------");
 %nf.WriteLine("// Saved By "@%client.namebase);
 %nf.WriteLine("");

 %dgroup = nametoId("deployables");

 %nf.WriteLine("function Build_"@%Fullname@"(%client, %center, %team)");
 %nf.WriteLine("{");
 %nf.WriteLine("  if (%team $= \"\")");
 %nf.WriteLine("   %team = 1;");
 //%nf.WriteLine("schedule(2000, 0, \"eval\", \"$Building = "\"\"";\");");
 //%nf.WriteLine("%datablock = (%team == 1) ? \"BuildingBlock0\" : \"BuildingBlock6\";");

 %nf.WriteLine("%offset = VectorSub(GetWords(%center, 0, 1) SPC GetWord(%center, 2), \""@%start@"\");");
 %nf.WriteLine("");

 for (%i = 0; %i < %dgroup.GetCount(); %i++)
 {
    %obj = %dGroup.getObject(%i);

    %db = %obj.getDataBlock().getname();

    if (%obj.team == %client.team && %db !$= "")
    {
         // BD mod

         %newline = "%building = new (StaticShape) () {datablock = "@%db@";";

         // position mod

         %pos = %obj.getPosition();

         %z = GetWord(%pos, 2) - %tHeight;

         %pos = GetWords(%pos, 0, 1) SPC %z;

         %newline = %newline @ "Position = VectorAdd(\""@%pos@"\", %offset);";

         // Rotation mod (modifed by Dark Dragon DX to fix rotation issue)

       // %rot = %obj.getRotation();
         %rot = getWords(%obj.getTransform(), 3, 6);

       //  %newline = %newline @ "Rotation = \""@%rot@"\";";

         // Scale mod

         %Scale = %obj.Scale;

         %newline = %newline @ "Scale = \""@%Scale@"\";";

         // Floor mod

         if (%obj.TheFloor)
         {
           %newline = %newline @ "TheFloor = \"true\";";

           %obj.TheFloor = "";
         }

         // type mod

         %newline = %newline @ "Type = \""@%name@"\";";

         // Team mod

         %newline = %newline @ "team = %team;};";

         // Unification

         %newline = %newline @ "addToDeployGroup(%obj);";

         // write

         %nf.WriteLine(%newline);
         %nf.WriteLine("%building.setRotation(\x22"@%rot@"\x22);");
     }

 }

 %nf.WriteLine("}");
 %nf.Close();
 %nf.Delete();

}

$CC_ConvTable["DeployedSpine"] = "%datablock"; // white/black pads are team based
$CC_ConvTable["DeployedSpine2"] = "%datablock"; // white/black pads are team based

$CC_ConvTable["DeployedSpine5"] = "\"BuildingBlock1\""; // brown pad

$CC_ConvTable["DeployedCrate8"] = "\"BuildingBlock5\""; // Recycle Unit
$CC_ConvTable["DeployedCrate4"] = "\"BuildingBlock2\""; // Quantum Battery
$CC_ConvTable["DeployedCrate3"] = "\"BuildingBlock3\""; // 4 tubes
$CC_ConvTable["DeployedCrate7"] = "\"BuildingBlock4\""; // Mag Cooler
$CC_ConvTable["DeployedCrate9"] = "\"BuildingBlock5\""; // Cylinder

