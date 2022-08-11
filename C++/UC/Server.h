#pragma once
#include "Core.h"
#include "NexusObject.h"
#include "ServerAddress.h"
#include "Manifest.h"

namespace uc
{
	class CNexus;

	struct CReleaseInfo
	{
		CManifest *			Manifest = null;

		~CReleaseInfo()
		{
			delete Manifest;
		}
	};

	struct CServerInfo
	{
		CString				Name;
		CServerAddress		Locator;
		bool				Installed = false;
		CUsl 				Url;
		HINSTANCE			HInstance;
		CXon *				Xon;
		CReleaseInfo *		Release;
		CXonDocument *		Registry = null;

		~CServerInfo()
		{
			delete Registry;
		}
	};

	enum class EStartMode
	{
		Initialization, Start
	};

	class UOS_LINKING CServer : public virtual IType
	{
		public:
			CNexus *									Nexus;
			CUsl										Url;
			CServerInfo *								Info;

			CList<CBaseNexusObject *>					Objects;
			
			CMap<CString, IProtocol *>					Protocols;
			CMap<CString, CList<IType *>>				Users;
			CEvent<CServer *, IProtocol *, CString &>	Disconnecting;
			
			UOS_RTTI
			CServer(CNexus * l, CServerInfo * info);
			~CServer();

			virtual void								Start(EStartMode sm){}
			virtual void								Execute(const CUrl & u, CExecutionParameters * ep){}

			virtual IProtocol *		 					Connect(CString const & pr)=0;
			virtual void								Disconnect(IProtocol * s)=0;

			virtual CBaseNexusObject *					CreateObject(CString const & name);
			virtual void								RegisterObject(CBaseNexusObject * o, bool shared);
			virtual void								DestroyObject(CBaseNexusObject * o);

			virtual CBaseNexusObject *					FindObject(CString const & name);
			CBaseNexusObject *							FindObject(CUol const & u);
			template<class T> T *						FindObject(CString const & name)
														{
															return FindObject(name)->As<T>();
														}

			CString										MapRelative(CString const & path);
			CString										MapUserLocalPath(CString const & path);
			CString										MapUserGlobalPath(CString const & path);
			CString										MapTmpPath(CString const & path);
			CString										MapPath(CString const & path);

	};

	typedef CServer *	(* FStartUosServer)(CNexus * l, CServerInfo * info);
	typedef void		(* FStopUosServer)();
}


