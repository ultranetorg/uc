#pragma once
//#include "Core.h"
#include "PersistentObject.h"
#include "ApplicationRelease.h"
#include "Identity.h"

namespace uc
{
	class CNexus;
	class CServer;

	struct CServerInstance
	{
		CString												Name;
		CApplicationRelease *								Release;
		CServer	*											Instance = null;
		//bool												Initialized = false;
		CXon *												Registration;
		CXon *												Command = null;
		CIdentity *											Identity = null;
	};

	class UOS_LINKING CServer : public virtual IType
	{
		public:
			CServerInstance *							Instance;
			CNexus *									Nexus;

			CList<CInterObject *>						Objects;
						
			UOS_RTTI
			CServer(CNexus * l, CServerInstance * info);
			~CServer();

			virtual void								SystemStart(){}
			virtual void								UserStart(){}

			virtual IProtocol *							Accept(CString const & protocol)=0;
			virtual void								Break(IProtocol * protocol)=0;

			virtual CInterObject *						CreateObject(CString const & name);
			virtual void								RegisterObject(CInterObject * o, bool shared);
			virtual void								DestroyObject(CInterObject * o);

			virtual CInterObject *						FindObject(CString const & name);
			CInterObject *								FindObject(CUol const & u);
			template<class T> T *						FindObject(CString const & name)
														{
															return FindObject(name)->As<T>();
														}

			CString										MapReleasePath(CString const & path);
			CString										MapSystemPath(CString const & path);
			CString										MapSystemTmpPath(CString const & path);
			CString										MapUserLocalPath(CString const & path);
			CString										MapUserGlobalPath(CString const & path);
			CString										MapUserTmpPath(CString const & path);

	};
}


