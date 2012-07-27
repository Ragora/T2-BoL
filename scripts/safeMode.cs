package SafeMode{ //Disables TCP and HTTPObjects, should only be used if you're totally running offline.
function TCPObject::onLine(){}
function TCPObject::connect(){}
function TCPObject::disconnect(){}
function TCPObject::listen(){}
function TCPObject::send(){}

function HTTPObject::onLine(){}
function HTTPObject::connect(){}
function HTTPObject::disconnect(){}
function HTTPObject::listen(){}
function HTTPObject::send(){}
function HTTPObject::post(){}

function SecureHTTPObject::onLine(){}
function SecureHTTPObject::connect(){}
function SecureHTTPObject::disconnect(){}
function SecureHTTPObject::listen(){}
function SecureHTTPObject::send(){}
function SecureHTTPObject::post(){}
};
activatePackage(SafeMode);
