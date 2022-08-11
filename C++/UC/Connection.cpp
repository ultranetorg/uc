#include "stdafx.h"
#include "Connection.h"
#include "Server.h"

using namespace uc;

bool CConnection::operator!() const
{
	return Server == null;
}

CConnection::operator bool() const
{
	return Server != null;
}

void CConnection::Clear()
{
	Who = null;
	Server = null;
	Protocol = null;
}

CConnection::CConnection(IType * who, CServer * server, CString const & pn)
{
	Who = who;
	Server = server;
	Protocol = server->Protocols[pn];
	ProtocolName = pn;
}

CConnection::CConnection()
{
	Who = null;
	Server = null;
}
