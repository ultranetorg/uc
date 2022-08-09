#pragma once
#include "Shared.h"
#include "Uxx.h"
#include "Guid.h"
#include "Protocol.h"
#include "Event.h"
#include "Path.h"

namespace uc
{
	class CServer;

	class UOS_LINKING CBaseNexusObject : public virtual CShared, public virtual IType//, public virtual IProtocol
	{
		public:
			CUol						Url;
			CServer	*					Server;

			CEvent<CBaseNexusObject *>	Destroying;
			bool						Shared = false;

			UOS_RTTI
			~CBaseNexusObject();

			CString						MapRelative(CString const & path);
			CString						MapGlobalPath(CString const & path);
			CString						MapLocalPath(CString const & path);

		protected:
			CBaseNexusObject(CServer * s, CString const & name);
	};

	template<class T> struct CObject
	{
		CUol		Url;
		T *			Object = null;

		CObject(){}
		CObject(CUol const & o) : Url(o) {}
		CObject(CBaseNexusObject * o) : Object(dynamic_cast<T *>(o))
		{
			if(o)
				Url = o->Url;
		}

		T * operator->()
		{
			return Object;
		}

	 	operator T * () const
		{
			return Object;
		}

		operator bool () const
		{
			return Object != null;
		}

		bool operator! () const
		{
			return Object == null;
		}

		void Clear()
		{
			Object = null;
		}
	};

	template<class T> struct CNameObject
	{
		CString		Name;
		T *			Object = null;

		CNameObject(){}
		CNameObject(T * o) : Object(o)
		{
			if(o)
				Name = o->Name;
		}

		T * operator->()
		{
			return dynamic_cast<T *>(Object);
		}

	 	operator T * () const
		{
			return dynamic_cast<T *>(Object);
		}

		operator bool () const
		{
			return Object != null;
		}

		bool operator! () const
		{
			return Object == null;
		}

		void Clear()
		{
			Object = null;
		}
	};

}