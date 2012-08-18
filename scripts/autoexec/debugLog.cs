// #autoload
// #name = Debug Log
// #version = 1.0
// #category = Utility
// #warrior = DarkDragonDX
// #description = Adds a debug log.

package debugLog
{
	function echo(%arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10)
	{
		if (!IsObject(DebugLogger))
		{
			new FileObject(DebugLogger);
			DebugLogger.lineCount = 0;
			DebugLogger.openForWrite("debugLog.txt");
		}
		if (DebugLogger.lineCount >= 10)
		{
			DebugLogger.close();
			DebugLogger.openForAppend("debugLog.txt");
		}
		DebugLogger.writeLine(%arg1 @ %arg2 @ %arg3 @ %arg4 @ %arg5 @ %arg6 @ %arg7 @ %arg8 @ %arg9 @ %arg10);
		DebugLogger.lineCount++;
		parent::echo(%arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10);
		return true;
	}
	
	function error(%arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10)
	{
		if (!IsObject(DebugLogger))
		{
			new FileObject(DebugLogger);
			DebugLogger.lineCount = 0;
			DebugLogger.openForWrite("debugLog.txt");
		}
		if (DebugLogger.lineCount >= 10)
		{
			DebugLogger.close();
			DebugLogger.openForAppend("debugLog.txt");
		}
		DebugLogger.writeLine("Error: " @ %arg1 @ %arg2 @ %arg3 @ %arg4 @ %arg5 @ %arg6 @ %arg7 @ %arg8 @ %arg9 @ %arg10);
		DebugLogger.lineCount++;
		parent::error(%arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10);
		return true;
	}
	
	function warning(%arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10)
	{
		if (!IsObject(DebugLogger))
		{
			new FileObject(DebugLogger);
			DebugLogger.lineCount = 0;
			DebugLogger.openForWrite("debugLog.txt");
		}
		if (DebugLogger.lineCount >= 10)
		{
			DebugLogger.close();
			DebugLogger.openForAppend("debugLog.txt");
		}
		DebugLogger.writeLine("Warning: " @ %arg1 @ %arg2 @ %arg3 @ %arg4 @ %arg5 @ %arg6 @ %arg7 @ %arg8 @ %arg9 @ %arg10);
		DebugLogger.lineCount++;
		parent::warning(%arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10);
		return true;
	}
};
activatePackage(debugLog);