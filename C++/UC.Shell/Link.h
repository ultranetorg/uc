#pragma once
#include "ShellLevel.h"

namespace uc
{
	struct CLink : public CWorldEntity
	{
		public:
			CUrl				Target;
			CString				Executor;
			CUol				PropertiesAvatar;
			CShellLevel *		Level;

			UOS_RTTI
			CLink(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName())) : CWorldEntity(l->Server, name)
			{	
				Level = l;

				SetDirectories(MapRelative(L""));
				SetDefaultInteractiveMaster(CArea::Main);
			}

			~CLink()
			{
				Save();
			}
			
			void SaveInstance()
			{
				CTonDocument d;
				d.Add(L"Title")->Set(Title);
				d.Add(L"Target")->Set(Target);
				d.Add(L"Executor")->Set(Executor);
				d.Add(L"PropertiesAvatar")->Set(PropertiesAvatar);

				SaveGlobal(d, GetClassName() + L".xon");
			}

			void LoadInstance()
			{
				CTonDocument d; LoadGlobal(d, GetClassName() + L".xon");

				Title				= d.Get<CString>(L"Title");
				Target				= d.Get<CUrl>(L"Target");
				Executor			= d.Get<CString>(L"Executor");
				PropertiesAvatar	= d.Get<CUol>(L"PropertiesAvatar");
			}

			void SetTarget(CUrl & t)
			{
				Target = t;

				if(Title.empty())
				{
					if(Target.Scheme == CFileSystemEntry::Scheme)
						Title = CPath::GetNameBase(CUol(Target).GetObjectId());
					else
						Title = Target.ToString();
				}
			}
	};
}