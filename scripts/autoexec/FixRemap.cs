// #autoload
// #name = Fix Remap
// #version = 1.0
// #category = Fix
// #warrior = WegBert
// #status = beta

function redoBrokenMapping( %actionMap, %device, %action, %cmd, %newIndex )
{
	%actionMap.bind( %device, %action, %cmd );
	//OP_RemapList.setRowById( %oldIndex, buildFullMapString( %oldIndex ) );
	OP_RemapList.setRowById( %newIndex, buildFullMapString( %newIndex ) );
}

package FixRemapLoad {

function RemapInputCtrl::onInputEvent( %this, %device, %action )
{
	Parent::onInputEvent( %this, %device, %action );

	// prevMapIndex would equal -1 under the following circumstances:
	//
	// 1. User installed a third-party script which added a remap entry to the list.
	//    The user removed that script and is trying to rebind a key that was
	//    previously bound to that script. With the remap entry missing, the system
	//    fails, because the function bound to the key has no $RemapCmd.
	//
	// 2. User installed a third-party script which did not add a remap entry to the
	//    list, but instead replaced an existing command with a third-party one
	//    (for example, the HappyThrow script upon being run for the first time
	//     rebinds the user's grenade key from the default function, throwGrenade, to
	//     throwGrenadeNew). The script may still be installed, but the user is trying
	//    to rebind the key bound to the third-party function. This third-party function,
	//    although perfectly valid and existing, has no $RemapCmd.
	//
	// To counter these problems, the script will warn the user that the function may or
	// may not exist. It will allow the user to choose whether they still want to rebind
	// the key or not. If they do, the script will proceed with the remapping that the
	// original fon complained about.

	if (%this.mode !$= "consoleKey")
	{
		switch$ ( OP_ControlsPane.group )
		{
			case "Observer":
				%actionMap = observerMap;
				%cmd  = $ObsRemapCmd[%this.index];

			default:
				%actionMap = moveMap;
				%cmd  = $RemapCmd[%this.index];
		}

		%prevMap = %actionMap.getCommand( %device, %action );
		if (%prevMap !$= %cmd && %prevMap !$= "")
		{
			%mapName = getMapDisplayName( %device, %action );
			%prevMapIndex = findRemapCmdIndex( %prevMap );
			if (%prevMapIndex == -1)
			{
				// The OK dialog has probably popped up now, so turn it off. We've got the situation
				// under control.
				if (MessageBoxOKDlg.isAwake())
					Canvas.popDialog(MessageBoxOKDlg);

				MessageBoxYesNo( "FIXREMAP WARNING",
				                 "\"" @ %mapName @ "\" is bound to the function \"" @ %prevMap @ "\"! The function may exist in a user script. See FixRemap.txt in your T2 autoexec dir for more details. Do you still want to undo this mapping?", 
				                 "redoBrokenMapping(" @ %actionMap @ ", " @ %device @ ", \"" @ %action @ "\", \"" @ %cmd @ "\", " @ %this.index @ ");", "" );
			}
		}
	}
}

};

activatePackage(FixRemapLoad);
