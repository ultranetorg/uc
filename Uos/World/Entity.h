#pragma once

namespace uc
{
	class CWorldEntity : public CPersistentObject
	{
		public:
			inline static const CString   	Scheme = L"worldentity";

			CString							DefaultInteractiveMasterTag;
			CString							Title;
			CEvent<CWorldEntity *>			Retitled;

			CWorldEntity(CServer * s, CString const & name) : CPersistentObject(Scheme, s, name)
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