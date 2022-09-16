#include "stdafx.h"
#include "Connection.h"
#include "Client.h"

using namespace uc;

bool CClientConnection::operator!() const
{
	return Client == null;
}

CClientConnection::operator bool() const
{
	return Client != null;
}

void CClientConnection::Clear()
{
	Who = null;
	Client = null;
	Protocol = null;
}

CClientConnection::CClientConnection(CApplicationRelease * who, CClient * client, CString const & protocol, std::function<void()> ondisconnect)
{
	Who = who;
	Client = client;
	Protocol = client->Instance->Interfaces(protocol);
	ProtocolName = protocol;
	OnDisconnect = ondisconnect;
}

CClientConnection::CClientConnection()
{
	Who = null;
	Client = null;
	Protocol = null;
}
