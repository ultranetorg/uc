#pragma once

namespace uc
{
	class CWorldEntity : public CStorableObject
	{
		public:
			CString										DefaultInteractiveMasterTag;
			CString										Title;
			CEvent<CWorldEntity *>						Retitled;

			CWorldEntity(CServer * s, CString const & name) : CStorableObject(s, name)
			{
			}

			~CWorldEntity()
			{
			}

			void SetDefaultInteractiveMaster(CString const & s)
			{
				DefaultInteractiveMasterTag = s;
			}

			void SetTitle(CString const & t)
			{
				if(t != Title)
				{
					Title = t;
					Retitled(this);
				}
			}

	};
}