#include "stdafx.h"
#include "Connection.h"
#include "Client.h"

using namespace uc;

bool CConnection::operator!() const
{
	return Client == null;
}

CConnection::operator bool() const
{
	return Client != null;
}

void CConnection::Clear()
{
	Who = null;
	Client = null;
	Protocol = null;
}

CConnection::CConnection(IType * who, CClient * client, CString const & pn)
{
	Who = who;
	Client = client;
	Protocol = client->Instance->Interfaces[pn];
	ProtocolName = pn;
}

CConnection::CConnection()
{
	Who = null;
	Client = null;
}
