// #autoload
// #name = Safe Mode
// #version = 1.0
// #date = Tuesday, January 12, 2010
// #author = Dark Dragon DX
// #warrior = DarkDragonDX
// #email = DarkDragonDX@Hotmail.com
// #web = http://www.the-Construct.net
// #description = Adds a new command-line to Tribes 2 for shortcuts: -safeMode
// #status = Release

package SafeMode{
function DispatchLaunchMode()
{
parent::DispatchLaunchMode();

    // check T2 command line arguments
    for(%i = 1; %i < $Game::argc ; %i++)
    {
        %arg = $Game::argv[%i];
        %nextArg = $Game::argv[%i+1];
        %hasNextArg = $Game::argc - %i > 1;

        if( !stricmp(%arg, "-Safemode")) //If enabled, cuts off all TCP & HTTP object connections, preventing UE's when running T2 without internet
        {
            exec("scripts/Safemode.cs");
        }
    }
}
};
activatePackage(SafeMode);

