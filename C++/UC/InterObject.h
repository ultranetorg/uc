#pragma once
#include "Shared.h"
#include "Uxx.h"
#include "Guid.h"
#include "Interface.h"
#include "Event.h"
#include "Path.h"
#include "IType.h"

namespace uc
{
	class CServer;

	class UOS_LINKING CInterObject : public virtual CShared, public virtual IType//, public virtual IProtocol
	{
		public:
			CUol						Url;
			CServer	*					Server;

			CEvent<CInterObject *>		Destroying;
			bool						Shared = false;

			UOS_RTTI
			~CInterObject();

			CString						MapRelative(CString const & path);
			CString						MapGlobalPath(CString const & path);
			CString						MapLocalPath(CString const & path);

		protected:
			CInterObject(CString const & scheme, CServer * s, CString const & name);
	};

	template<class T> struct CObject
	{
		CUol		Url;
		T *			Object = null;

		CObject(){}
		CObject(CUol const & o) : Url(o) {}
		CObject(CInterObject * o) : Object(dynamic_cast<T *>(o))
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


}