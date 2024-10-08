#pragma once
#include "ShellLevel.h"

namespace uc
{
	class CTrayItem
	{
		public:
			auto static constexpr	Scheme = L"trayitem";

			CString					Title;
			CTexture *				Icon;
			CEvent<CTrayItem *>		TitleChanged;
			CEvent<CTrayItem *>		IconChanged;
			CUrl					Url;
			CEvent<CTrayItem *>		MarkedOld;
			bool					Old = false;

			CTrayItem(CShellLevel * l,CUol & u)
			{
				Url = u;
			}

			~CTrayItem()
			{
			}

			void SetIcon(CTexture * icon)
			{
				Icon = icon;
				IconChanged(this);
			}

			void SetTitle(CString const & t)
			{
				Title = t;
				TitleChanged(this);
			}

			void MarkOld()
			{
				Old = true;
				MarkedOld(this);
			}
	};


	class CTrayProtocol : public IProtocol
	{
		public:
			auto static constexpr	InterfaceName = L"Tray1";

			virtual	CTrayItem *		AddItem(CUol & u)=0;
			virtual	void			RemoveItem(CTrayItem *)=0;

			virtual ~CTrayProtocol(){}
	};


	class CTray : public CWorldEntity, public CTrayProtocol
	{
		public:
			CShellLevel *			Level;
			CList<CTrayItem *>		Items;
			CEvent<CTrayItem *>		Added;
			CEvent<CTrayItem *>		Removed;

			CTrayItem *				UpdateItem;
			
			UOS_RTTI
			CTray(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName())) : CWorldEntity(l->Server, name)
			{
				Level = l;
			
				SetDirectories(MapRelative(L""));

				auto i = AddItem(CUol(CTrayItem::Scheme, Url.Server, CGuid::Generate64(L"Info")));
				i->SetTitle(CString::Format(L"Profile: %s", Level->Core->Unid));
				i->MarkOld();

				/// UpdateItem = AddItem(CUol(Url, CGuid::Generate64(L"Update")));
				/// UpdateItem->SetTitle(Level->Nexus->UpdateStatus);
				///
				/// Level->Nexus->UpdateStatusChanged += ThisHandler(UpdateStatusChanged);
			}

			~CTray()
			{
				///Level->Nexus->UpdateStatusChanged -= ThisHandler(UpdateStatusChanged);

				Save();

				for(auto i : Items)
				{
					delete i;
				}
			}

			///	void UpdateStatusChanged()
			///	{
			///		UpdateItem->SetTitle(CString::Format(L"Updates found: %d", Level->Nexus->NewReleases.size()));
			///
			///		if(Level->Nexus->NewReleases.empty())
			///		{
			///			UpdateItem->MarkOld();
			///		}
			///	}

			CTrayItem * AddItem(CUol & u) override
			{
				auto ti = new CTrayItem(Level, u);
				Items.push_back(ti);
				Added(ti);
				
				return ti;
			}

			void RemoveItem(CTrayItem * ti) override
			{
				Removed(ti);
				Items.Remove(ti);
				delete ti;
			}

	};
}
