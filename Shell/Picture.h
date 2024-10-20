#pragma once
#include "ShellLevel.h"

namespace uc
{
	struct CPicture : public CWorldEntity
	{
		public:
			CString				File;
			CShellLevel *		Level;

			UOS_RTTI
			CPicture(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName())) : CWorldEntity(l->Server, name)
			{	
				Level = l;
			
				SetDirectories(MapRelative(L""));
				SetDefaultInteractiveMaster(CArea::Main);
			}

			~CPicture()
			{
				Save();
			}
			
			void SetFile(CString & u)
			{
				File = u;
				SetTitle(CPath::GetName(u));
			}

			void SaveInstance()
			{
				CTonDocument d;
				d.Add(L"File")->Set(File);

				SaveGlobal(d, GetClassName() + L".xon");
			}

			void LoadInstance()
			{
				CTonDocument d;
				LoadGlobal(d, GetClassName() + L".xon");

				SetFile(d.Get<CString>(L"File"));
			}
	};
}