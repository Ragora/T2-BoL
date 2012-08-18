//------------------------------------------------------------------------------
// scripts/modScripts/server/serverNetworking.cs
//------------------------------------------------------------------------------

if (!IsObject(ServerNetwork))
new TCPObject(ServerNetwork);

function ServerNetwork::onConnect(%this)
{
}

function ServerNetwork::onConnectFailed(%this)
{
	if (%this.testingServer)
	{
		error("Error: Unable to verify connection to server "@%this.testIP@" at port "@%this.testPort@"!");
		%this.testIP = "";
		%this.testPort = "";
		%this.testingServer = false;
	}
}

function ServerNetwork::onDisconnect(%this)
{
}

function ServerNetwork::onDisconnectFailed(%this)
{
}

function ServerNetwork::listen(%this)
{
}

function ServerNetwork::send(%this)
{
}

function ServerNetwork::onLine(%this, %line)
{
}

function ServerNetwork::testServerIP(%this, %IP, %port)
{
	%this.testingServer = true;
	%this.testIP = %ip;
	%this.testPort = %port;
	%this.connect(%ip @ ":" @ %port);
}
