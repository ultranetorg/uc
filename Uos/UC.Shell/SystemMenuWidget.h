#pragma once
#include "MenuWidget.h"
#include "ApplicationsMenu.h"

namespace uc
{
	class CSystemMenuWidget : public CMenuWidget
	{
		using CElement::UpdateLayout;

		public:
			UOS_RTTI
			CSystemMenuWidget(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName())) : CMenuWidget(l, name)
			{
				//Level->Nexus->Initialized += ThisHandler(LoadSystemItems);
			}

			~CSystemMenuWidget()
			{
				//Level->Nexus->Initialized -= ThisHandler(LoadSystemItems);
			}

			void SetEntity(CUol & e) override
			{
				Entity = Server->FindObject(e);
				Entity->Destroying += ThisHandler(OnDependencyDestroying);

				LoadSystemItems();
			}

			void LoadSystemItems()
			{
				auto sys = AddMenuWidgetSectionItem(L"System");

				
				Level->AddSystemMenuItems(sys->Section);

				//auto friends = Level->Nexus->ConnectMany<IWorldFriend>(this, WORLD_FRIEND_PROTOCOL);
				//
				//for(auto i : friends)
				//{
				//	for(auto i : i->CreateActions())
				//	{
				//		//AddMenuWidgetSectionItem(i);
				//		Level->AddMenuItem(sys->Section, i);
				//	}
				//}
				//
				//Level->Nexus->Disconnect(friends);

				UpdateLayout();
			}
	};
}
