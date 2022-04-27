#pragma once
#include "ShellLevel.h"

namespace uc
{
	class CHistoryItem
	{
		public:
			CString										Title;
			CEvent<CHistoryItem *>						TitleChanged;
			CString										Token;
			CObject<CWorldEntity>						Object;

			CHistoryItem(CShellLevel * l){}
			~CHistoryItem()
			{
				OnDependencyDestroying(Object);
			}

			void SetObject(CWorldEntity * e)
			{
				Object = e;

				Object->Destroying += ThisHandler(OnDependencyDestroying);
				Object->Retitled += ThisHandler(OnTitleChanged);

				Title = Object->Title;
			}

			void OnDependencyDestroying(CNexusObject * o)
			{
				if(Object)
				{
					Object->Destroying -= ThisHandler(OnDependencyDestroying);
					Object->Retitled -= ThisHandler(OnTitleChanged);
					Object.Clear();
				}
			}

			void Save(CXon * xon)
			{
				xon->Set(Object.Url);
				xon->Add(L"Title")->Set(Title);
				xon->Add(L"Token")->Set(Token);
			}

			void Load(CXon * xon)
			{
				Object.Url		= xon->Get<CUol>();
				Title			= xon->Get<CString>(L"Title");
				Token			= xon->Get<CString>(L"Token");
			}

			void OnTitleChanged(CWorldEntity * a)
			{
				Title = a->Title;
				TitleChanged(this);
			}
	};

	class CHistory : public CWorldEntity
	{
		public:
			CShellLevel *								Level;
			CList<CHistoryItem *>						Items;
			CEvent<CHistoryItem *>						Added;
			CEvent<CHistoryItem *>						Opened;
			
			UOS_RTTI
			CHistory(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName())) : CWorldEntity(l->Server, name)
			{
				Level = l;
				SetDirectories(MapRelative(L""));

				Level->World->UnitOpened += ThisHandler(OnModelOpened);
				Level->World->UnitClosed += ThisHandler(OnModelClosed);
			}

			~CHistory()
			{
				Save();

				for(auto i : Items)
				{
					delete i;
				}

				Level->World->UnitOpened -= ThisHandler(OnModelOpened);
				Level->World->UnitClosed -= ThisHandler(OnModelClosed);
			}
			
			void OnModelOpened(CUnit * a, CTransformation & t, CShowParameters * f)
			{
				if(	!Level->World->Starting &&
					(Level->World->MainArea->ContainsDescedant(a) ||
					Level->World->FieldArea->ContainsDescedant(a) || 
					Level->World->ThemeArea->ContainsDescedant(a)) ) // imporatant: skip Hud space - otherwise circular refs because we store ref to Server in a HistoryItem
				{
					auto hi = Items.Find([a](CHistoryItem * i){ return i->Object.Url == a->Entity.Url; });
					
					if(!hi)
					{
						auto hi = new CHistoryItem(Level);
						hi->SetObject(a->Entity);
						//hi->Token = hi->Allocation->AddGlobalReference(Url);
						
						Items.push_front(hi);

						Added(hi);
					}
					else
					{
						if(!hi->Object)
						{
							hi->SetObject(a->Entity);
						}

						Items.remove(hi);
						Items.push_front(hi);

						Opened(hi);
					}
				}
			}

			void OnModelClosed(CUnit *)
			{
			}

			void SaveInstance() override
			{
				CTonDocument d;

				for(auto i : Items)
				{
					i->Save(d.Add(L"Item"));
				}

				SaveGlobal(d, Level->World->Name + L".xon");
			}

			void LoadInstance() override
			{
				if(Level->Nexus->Storage->Exists(CPath::Join(GlobalDirectory, Level->World->Name + L".xon")))
				{
					CTonDocument d;
					LoadGlobal(d, Level->World->Name + L".xon");

					for(auto i : d.Many(L"Item"))
					{
						auto hi = new CHistoryItem(Level);
						hi->Load(i);
						Items.push_back(hi);
					}
				}
			}
	};
}
