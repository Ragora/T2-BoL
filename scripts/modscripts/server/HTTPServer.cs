//------------------------------------------------------------------------------
// HTTPServer.cs
// An experimental HTTP Server written in Torque!
// Copyright (c) 2012 Robert MacGregor
//------------------------------------------------------------------------------

// Replicate Code for Servers
$HTTPServer::ServerReplicate = "function *UNIQUESOCKET*::onConnectRequest(%this, %address, %socket)\n" @
"{\n" @
"%this.Parent.connectionCreated(%address, %socket);\n" @
"return true;\n" @
"}\n";
// Replicate Code for Clients
$HTTPServer::ClientReplicate = "function *UNIQUESOCKET*::onLine(%this, %line)\n" @
"{\n" @ 
"%this.Parent.onLine(%this, %line);\n" @
"return true;\n" @ 
"}\n" @
"function *UNIQUESOCKET*::onDisconnect(%this) { %this.Parent.connectionDropped(%this); return true; }\n" @
"function *UNIQUESOCKET*::sendPacket(%this,%packet)\n" @
"{\n" @
"%this.send(%packet.statusCode);\n" @
"for (%i = 0; %i < %packet.headerCount; %i++)\n" @
"{\n" @
"%this.send(%packet.headers[%packet.headerName[%i]]);\n" @
"}\n" @
"%this.send(\"\\n\");\n" @
"%this.send(%packet.payLoad);\n" @
"%this.disconnect();\n" @
"}\n";

function HTTPServer::listen(%this, %address, %port, %maxClients)
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
	%evalCode = $HTTPServer::ServerReplicate;
	%evalCode = strReplace(%evalCode, "*UNIQUESOCKET*", %uniqueString);
	eval(%evalCode);
	
	// Generate a list of unique names that this TCPServer will use (to keep down function def count)
	for (%i = 0; %i < %maxClients; %i++)
		while (true)
		{
			%uniqueName = generateString(%uniqueNameLength, %charList);
			if (!isObject(%uniqueName)) 
			{ 
				%eval = strReplace($HTTPServer::ClientReplicate, "*UNIQUESOCKET*", %uniqueName);
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
	
	%statusCodes = HTTPServerPrefs.get("StatusCodes",0);
	%this.Page[404] = %statusCodes.element("404");
	%this.Page[403] = %statusCodes.element("403");
	
	%generic = HTTPServerPrefs.get("Generic",0);
	%this.Root = %generic.element("Root");
	
	%this.Variables = Array.create();
	%this.MimeTypes = HTTPServerPrefs.get("MIME");
	
	logEcho("Server " @ %uniqueString SPC "is ready on " @ %address @ ":" @ %port);
	return true;
}

function HTTPServer::disconnect(%this)
{
}

function HTTPServer::connectionDropped(%this, %socket)
{
	if (!IsObject(%socket))
		return false;
	%socket.disconnect();
	if (!%this.allowMultiConnect)
		%this.connections[%socket.Address] = "";
	else
		%this.connections[%socket.Address, %socket.Port] = "";
	%this.uniqueNameInUse[%socket.getName()] = false;
	%this.connectionCount--;
	%this.onClientDisconnect(%socket);
}

function HTTPServer::connectionCreated(%this, %address, %socket)
{	
	%isReplicate = false;
	// Get the Port No. and Address respectively
	%address = strReplace(%address, "IP:","");
	%portPos = strStr(%address, ":");
	%port = getSubStr(%address, %portPos+1, strLen(%address));
	%address = getSubStr(%address, 0, %portPos);
		
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
	
	// If we were unable to find a good unique name
	%charList = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	%uniqueStringLen = 6; // Adjust this if needed, but there shouldn't be a reason to
	if (%uniqueName $= "")
	{
		while (true)
		{
			%uniqueName = generateString(%uniqueStringLen, %charList); 
			if (!isObject(%uniqueName)) break;
			else %uniqueName = "";
		}
		%eval = strReplace($HTTPServer::ClientReplicate, "*UNIQUESOCKET*", %uniqueName);
		eval(%eval);
		
		%this.uniqueName[%this.maximumClients] = %uniqueName;
		%this.uniqueNameInUse[%uniqueName] = true;
		%this.maximumClients++;
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
	
	%this.schedule(10000,"connectionDropped",%connection);
	
	return true;
}

// Callbacks -- make these do whatever you please!
function HTTPServer::onClientConnect(%this, %address, %socket) 
{
	echo("Received connection from " @ %address @ ". ID:" @ %socket);
	return true;
}
function HTTPServer::onClientRejected(%this, %socket, %reason) // %reason is always zero as of now.
{
	return true;
}
function HTTPServer::onClientDisconnect(%this, %socket)
{
	logEcho("Received Disconnect (" @ %socket @ ")");
	%socket.delete();
	return true;
}
function HTTPServer::onLine(%this, %socket, %line) 
{
	%reqType = getWord(%line, 0);
	if (%reqType $= "GET")
	{
		%req = getWord(%line, 1);
		%reqLen = strLen(%req);
		%socket.request = getSubStr(%req, 1, %reqLen);
		%socket.request = strReplace(%socket.request, "%20", " ");
		%socket.request = %this.Root @ %socket.request;
		%socket.requestType = "GET";
	}
	
	%data = "<HTML><header><title>404 - Not Found</title></header><body>Oops!<br>File not found.</body></HTML>";
	%forbiddenData = "<HTML><header><title>403- Forbidden</title></header><body>Oops!<br>You're not allowed to see this.</body></HTML>";
	
	%packet = new ScriptObject(){ class = "HTTPResponsePacket"; };
	//return;
	// We received the end-of-packet from a socket
	if (%line $= "")
	{
		//Shitty
		if (strStr(%socket.request,".") != -1)
		{
			if (!isFile(%socket.request))
				%packet.setStatusCode(404);
				
			else if (%socket.request $= "prefs/ClientPrefs.cs")
			{
				%packet.setStatusCode(403);
				%data = %forbiddenData;
			}
			else if (getFileExtensionFromString(%socket.request) $= "cs")
			{
				%packet.setStatusCode(200);
			
				// FIXME: INSECURE and gay
				exec(%socket.request);
				%class = strReplace(getFileNameFromString(%socket.request), ".cs", ""); // FIXME: Crappy
				%instance = new ScriptObject() { class = %class; };
				%data = %instance.contents();
				%instance.delete();
			}
			else
			{
				%packet.setStatusCode(403);
				%data = %forbiddenData;
			}
			
			// Check the file type
			%extension = getFileExtensionFromString(%socket.request);
			if (%extension $= "html" || %extension $= "htm") 
			{
				%script = strReplace(%socket.request,".html",".cs");
				if (isFile(%script))
				{
					exec(%script);
					%Object = new ScriptObject(){ class = "ServerApp"; };
					%data = %Object.execute(%data);
					%Object.delete();
				}
			}
		}
		else
		{
			%packet.setStatusCode(200);
			%data = "<HTML>\n<header>\n<title>\nDirectory\n</title>\n</header>\n<body><h1>Directory of " @ %socket.request @ "</h1>\n";
			for( %file = findFirstFile( %socket.request @ "*.*" ); %file !$= ""; %file = findNextFile( %socket.request @ "*.*" ) )
			{
				%file = strReplace(%file, %socket.request, "");
				if (strStr(%file, "/") != -1)
				{
					%dir = getSubStr(%file, 0, strStr(%file, "/")) @ "/";
					if (!%dirAdded[%dir])
					{
						%data = %data @ "<a href=\"" @ strReplace(%dir, " ","%20") @ "\">" @ %dir @ "</a><br>\n";
						%dirAdded[%dir] = true;
					}
				}
				else
					%data = %data @ "<a href=\"" @ strReplace(%file, " ", "%20") @ "\">" @ %file @ "</a><br>\n";
			}
			%data = %data @ "</body>\n</HTML>\n";
		}
		%packet.setHeader("Date",formatTimeString("DD, dd MM yy hh:nn:ss ") @ "Eastern");
		%packet.setHeader("Server","Tribes 2");
		%packet.setHeader("Content-Type","text/html");
		%packet.setPayload(%data);
		%socket.sendPacket(%packet);
		%packet.delete();
		%this.connectionDropped(%socket);
	}
	if (isObject(%packet))
		%packet.delete();
	return true;
}

// Packet Functions (used for packet generation/reading)
function HTTPResponsePacket::setHeader(%this, %name, %value)
{
	if (%this.headerCount == "")
		%this.headerCount = 0;
	
	if (%this.headers[%name] $= "")
	{
		%this.headerName[%this.headerCount] = %name;
		%this.headerCount++;
	}	
	%this.headers[%name] = %name @ ": " @ %value @ "\n";
	return true;
}

function HTTPResponsePacket::setStatusCode(%this, %code)
{
	%this.statusCode = "HTTP/1.1 " @ %code SPC "OK\n";
	return true;
}

function HTTPResponsePacket::setPayload(%this, %data)
{
	%this.payLoad = %data;
	%this.payloadSize = strLen(%data);
	%this.setHeader("Content-Length", %this.payloadSize);
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

// -- 
// BoL Specific Code -- automatically load the server if enabled
if (!IsObject(HTTPServerPrefs))
	new ScriptObject(HTTPServerPrefs) { class = "BasicDataParser"; };
HTTPServerPrefs.empty();
HTTPServerPrefs.load("prefs/WebServer.conf");
%generic = HTTPServerPrefs.get("Generic",0);
if(%generic.element("Enabled") $= "true")
{
	new ScriptObject(WebServer) { class = "HTTPServer"; };
	WebServer.listen(%generic.element("Host"),%generic.element("Port"),%generic.element("StartWorkers"));
}
// --