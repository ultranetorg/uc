#pragma once
#include "Interface.h"
#include "IType.h"

namespace uc
{
	class CClient;

	class UOS_LINKING CConnection
	{
		public:
			IType *				Who;
			CClient *			Client;
			IInterface *		Protocol;
			CString				ProtocolName;

			CConnection();
			CConnection(IType * who, CClient * server, CString const & pn);

			void				Clear();
			bool				operator! () const;
			operator bool () const;

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