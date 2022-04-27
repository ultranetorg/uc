#pragma once
#include "ExperimentalLevel.h"

namespace uc
{
	struct CEarth : public CWorldEntity
	{
		public:
			CExperimentalLevel *		Level;

			UOS_RTTI
			CEarth(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName())) : CWorldEntity(l->Server, name)
			{	
				Level = l;
			
				SetDirectories(MapRelative(L""));
				SetDefaultInteractiveMaster(AREA_MAIN);

				SetTitle(Url.GetType());
			}

			~CEarth()
			{
				Save();
			}
			
			void SaveInstance()
			{
				//CXonDocument d;
				//SaveGlobal(d, GetClassName() + L".xon");
			}

			void LoadInstance()
			{
				//CTonDocument d; LoadGlobal(d, GetClassName() + L".xon");
			}
	};
}