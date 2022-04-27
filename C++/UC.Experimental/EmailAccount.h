#pragma once
#include "ExperimentalLevel.h"
#include "Vmime.h"

namespace uc
{
	class CEmailAccount : public CWorldEntity
	{
		public:
			CExperimentalLevel *		Level;
			CUrl						Server;
			CString						User;
			CString						Password;
			CEvent<>					Changed;

			UOS_RTTI
			CEmailAccount(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName())) : CWorldEntity(l->Server, name)
			{	
				Level = l;

				Server	= CUrl(L"<server>");
				User	= L"<user>";

				#ifdef _DEBUG
				Server = CUrl(L"pop3s://pop.gmail.com:995");
				User = L"mightywill";
				Password = L"miggooteo-3";
				#endif

				SetTitle(L"Email Account - " + User);
				SetDirectories(MapRelative(L""));
				SetDefaultInteractiveMaster(AREA_MAIN);
			}

			~CEmailAccount()
			{
				Save();
			}
			
			void SaveInstance() override
			{
				CTonDocument d;
				d.Add(L"Title")->Set(Title);
				d.Add(L"Server")->Set(Server);
				d.Add(L"User")->Set(User);
				d.Add(L"Password")->Set(Password);

				SaveGlobal(d, GetClassName() + L".xon");
			}

			void LoadInstance() override
			{
				CTonDocument d; LoadGlobal(d, GetClassName() + L".xon");
				
				Title		= d.Get<CString>(L"Title");
				Server		= d.Get<CUrl>(L"Server");
				User		= d.Get<CString>(L"User");
				Password	= d.Get<CString>(L"Password");

				SetTitle(L"Email Account - " + User);
			}
	};
}