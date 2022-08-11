#pragma once
#include "Protocol.h"

namespace uc
{
	class CServer;

	class UOS_LINKING CConnection
	{
		public:
			IType *				Who;
			CServer *			Server;
			IProtocol *			Protocol;
			CString				ProtocolName;

			CConnection();
			CConnection(IType * who, CServer * server, CString const & pn);

			void Clear();
			operator bool () const;
			bool operator! () const;

			template<class T> T * As()
			{
				return dynamic_cast<T *>(Protocol);
			}
	};

	template<class P> struct CProtocolConnection : public CConnection
	{
		CProtocolConnection(){}
		CProtocolConnection(CConnection & c) : CConnection(c){}

		P * operator->()
		{
			return dynamic_cast<P *>(Protocol);
		}

		operator P * () const
		{
			return dynamic_cast<P *>(Protocol);
		}
	};

}