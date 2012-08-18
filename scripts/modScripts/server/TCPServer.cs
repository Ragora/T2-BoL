//------------------------------------------------------------------------------
// TCPServer.cs
// Needed this type of thing for communication between servers in the game,
// so after some googling -- this is what I put out.
// forum.blockland.us/index.php?topic=105360
// Copyright (c) 2012 The DarkDragonDX
//------------------------------------------------------------------------------

// Replicate Code for Servers
$TCPServer::ServerReplicate = "function *UNIQUESOCKET*::onConnectRequest(%this, %address, %socket)" @
"{" @
"%this.Parent.connectionCreated(%address, %socket); " @
"return true;" @
"}";
// Replicate Code for Clients
$TCPServer::ClientReplicate = "function *UNIQUESOCKET*::onLine(%this, %line)" @
"{" @ 
"%this.Parent.onLine(%this, %line);" @
"return true;" @ 
"}" @
"function *UNIQUESOCKET*::onDisconnect(%this) { %this.Parent.connectionDropped(%this); return true; }";

function TCPServer::listen(%this, %address, %port, %maxClients)
{
	%uniqueNameLength = 6; // Adjust this if need be, but there should never be a reason to
	%this.allowMultiConnect = false; // If false, when a client connects twice, its old connection is killed and replaced with a new one.
	if (%this.isListening)
		return false;
	if (%maxClients < 1 || %maxClients == 0)
		%maxClients = 8;
	%oldAddr = $Host::BindAddress;
	%address = strlwr(%address);
	if (%address $= "local" || %address $="localhost" ) %address = "127.0.0.1";
	else if (%address $= "any") %address = "0.0.0.0";
	
	%charList = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	// Generate A name for a TCPObject (and make sure it's unique)
	%uniqueStringLen = 6; // Adjust this if needed, but there shouldn't be a reason to
	%uniqueString = "";
	while (true)
	{
		%uniqueString = generateString(%uniqueNameLength, %charList);
		if (!isObject(%uniqueString)) break;
		else %uniqueString = "";
	}
	%evalCode = $TCPServer::ServerReplicate;
	%evalCode = strReplace(%evalCode, "*UNIQUESOCKET*", %uniqueString);
	eval(%evalCode);
	
	// Generate a list of unique names that this TCPServer will use (to keep down function def count)
	for (%i = 0; %i < %maxClients; %i++)
		while (true)
		{
			%uniqueName = generateString(%uniqueNameLength, %charList);
			if (!isObject(%uniqueName)) 
			{ 
				%eval = strReplace($TCPServer::ClientReplicate, "*UNIQUESOCKET*", %uniqueName);
				eval(%eval);
				%this.uniqueName[%i] = %uniqueName; 
				%this.uniqueNameInUse[%uniqueName] = false;
				break; 
			}
		}
	
	// Create the Socket and we'll rock n' roll
	$Host::BindAddress = %address;
	%this.Server = new TCPObject(%uniqueString);
	%this.Server.listen(%port);
	%this.Server.Parent = %this;
	%this.connectionCount = 0;
	%this.maximumClients = %maxClients;
	$Host::BindAddress = %oldAddr;
	%this.isListening = true;
	
	logEcho("Server " @ %uniqueString SPC "is ready on " @ %address @ ":" @ %port);
	return true;
}

function TCPServer::disconnect(%this)
{
}

function TCPServer::connectionDropped(%this, %socket)
{
	if (!%this.allowMultiConnect)
		%this.connections[%socket.Address] = "";
	else
		%this.connections[%socket.Address, %socket.Port] = "";
	%this.connectionCount--;
	%this.onClientDisconnect(%socket);
}

function TCPServer::connectionCreated(%this, %address, %socket)
{	
	%isReplicate = false;
	// Get the Port No. and Address respectively
	%address = strReplace(%address, "IP:","");
	%portPos = strStr(%address, ":");
	%port = getSubStr(%address, %portPos+1, strLen(%address));
	%address = getSubStr(%address, 0, %portPos);
	if (!%this.allowMultiConnect && %this.connections[%address] != 0)
	{ 
		%this.connections[%address].disconnect();
		%this.connections[%address].delete();
		%isReplicate = true;
	}
	
	if (%this.connectionCount >= %this.maximumClients)
	{
		// Create the connection so we can disconnect it *lol*
		%connection = new TCPObject(%uniqueName,%socket) { class = ConnectionTCP; parent = %this; Address = %address; Port = %port; };
		%this.onClientRejected(%connection, 0);
		logEcho("Unable to accept connection from " @ %address SPC " -- already at maximum client count! (" @ %this.maximumClients @ ")");
		%connection.disconnect();
		%connection.delete();
		return false;
	}
	
	// Pick a unique name
	%uniqueName = "";
	for (%i = 0; %i < %this.maximumClients; %i++)
	{
		%name = %this.uniqueName[%i];
		if (!%this.uniqueNameInUse[%name])
		{
			%uniqueName = %name;
			%this.uniqueNameInUse[%name] = true;
			break;
		}
	}	
	// Create The Client Socket
	%connection = new TCPObject(%uniqueName,%socket) { class = ConnectionTCP; parent = %this; Address = %address; Port = %port; };
	
	if (!%this.allowMultiConnect)
		%this.connections[%address] = %connection;
	else
		%this.connections[%address, %port] = %connection;
		
	%this.connectionCount++;
	%this.onClientConnect(%address, %connection); 
	logEcho("Received client connection from " @ %address); 
	
	return true;
}

// Callbacks -- make these do whatever you please!
function TCPServer::onClientConnect(%this, %address, %socket) 
{
	echo("Received connection from " @ %address @ ". ID:" @ %socket);
	return true;
}
function TCPServer::onClientRejected(%this, %socket, %reason) // %reason is always zero as of now.
{
	return true;
}
function TCPServer::onClientDisconnect(%this, %socket)
{
	error("Received Disconnect (" @ %socket @ ")");
	return true;
}
function TCPServer::onLine(%this, %socket, %line) 
{
	echo(%socket SPC "says: " @ %line);
	return true;
}

// Lib Functions
function generateString(%length, %alpha)
{
	%len = strLen(%alpha);
	%result = "";
	for (%i = 0; %i < %length; %i++)
		%result = %result @ getSubStr(%alpha, getRandom(0, %len), 1);
	return %result;
}