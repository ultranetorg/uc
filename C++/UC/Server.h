#pragma once
#include "Core.h"
#include "StorableObject.h"
#include "ServerAddress.h"
#include "Manifest.h"

namespace uc
{
	class CNexus;
	class CServer;
	struct CServerRelease;

	typedef CServer *	(* FStartUosServer)(CNexus * l, CServerRelease * info, CXon * command);
	typedef void		(* FStopUosServer)(CServer *);

	struct CServerRelease
	{
		CServerAddress		Address;
		HINSTANCE			HInstance;
		FStartUosServer		StartUosServer;
		CManifest *			Manifest = null;
		CXonDocument *		Registry = null;

		~CServerRelease()
		{
			delete Registry;
		}
	};

	enum class EStartMode
	{
		Initialization, Start
	};

	class CIdentity
	{
	};

	class UOS_LINKING CServer : public virtual IType
	{
		public:
			CString												Instance;
			bool												Initialized = false;
			CXon *												Registration;
			CXon *												Command;
			CServerRelease *									Release;
			CNexus *											Nexus;
			CIdentity *											Identity = null;

			CList<CInterObject *>								Objects;
			
			CMap<CString, IInterface *>							Interfaces;
			CMap<CString, CMap<IType *, std::function<void()>>>	Users;
			
			UOS_RTTI
			CServer(CNexus * l, CServerRelease * info);
			~CServer();

			//virtual void								Execute(CXonValue * ep){}

			virtual IInterface *		 				Connect(CString const & pr)=0;
			virtual void								Disconnect(IInterface * s)=0;

			virtual CInterObject *						CreateObject(CString const & name);
			virtual void								RegisterObject(CInterObject * o, bool shared);
			virtual void								DestroyObject(CInterObject * o);

			virtual CInterObject *						FindObject(CString const & name);
			CInterObject * FindObject(CUol const & u);
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


