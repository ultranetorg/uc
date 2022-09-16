#pragma once
#include "Interface.h"
#include "IType.h"

namespace uc
{
	class CClient;
	struct CApplicationRelease;

	class UOS_LINKING CClientConnection
	{
		public:
			CApplicationRelease *	Who;
			CClient *				Client;
			IProtocol *				Protocol;
			CString					ProtocolName;
			std::function<void()>	OnDisconnect;

			CClientConnection();
			CClientConnection(CApplicationRelease * who, CClient * client, CString const & protocol, std::function<void()> ondisconnect);

			void				Clear();
			bool				operator! () const;
			operator bool () const;

			template<class T> T * As()
			{
				return dynamic_cast<T *>(Protocol);
			}
	};

// 	class UOS_LINKING CServerConnection
// 	{
// 		public:
// 			CApplicationRelease *	Who;
// 			CClient *				Client;
// 			IProtocol *				Protocol;
// 			CString					ProtocolName;
// 
// 			CClientConnection();
// 			CClientConnection(CApplicationRelease * who, CClient * client, CString const & protocol);
// 
// 			void				Clear();
// 			bool				operator! () const;
// 			operator bool () const;
// 
// 			template<class T> T * As()
// 			{
// 				return dynamic_cast<T *>(Protocol);
// 			}
// 	};

	template<class P> struct CProtocolConnection
	{
		CClientConnection * Connection = null;

		CProtocolConnection(){}
		CProtocolConnection(CClientConnection * c)
		{
			Connection = c;
		}

		P * operator->()
		{
			return dynamic_cast<P *>(Connection->Protocol);
		}

		operator P * () const
		{
			return dynamic_cast<P *>(Connection->Protocol);
		}

		operator CClientConnection * () const
		{
			return Connection;
		}

		operator bool()
		{
			return Connection != null;
		}
	};

}