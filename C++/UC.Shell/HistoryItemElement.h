#pragma once
#include "History.h"

namespace uc
{
	class CHistoryItemElement : public CAvatarCard
	{
		public:
			CShellLevel *								Level;
			CHistoryItem *								Entity = null;
			CFloat4										Color;

			using CAvatarCard::SetEntity;

			UOS_RTTI
			CHistoryItemElement(CShellLevel * l) : CAvatarCard(l->World, GetClassName())
			{
				Level = l;

				Active->IsPropagator = false;
				Active->MouseEvent[EListen::NormalAll]	+= ThisHandler(OnMouse);
				Active->TouchEvent[EListen::NormalAll]	+= ThisHandler(OnTouch);

				TextColor = Color = Level->Style->Get<CFloat4>(L"Text/Color/Disabled");
			}

			~CHistoryItemElement()
			{
			}

			void SetRecent()
			{
				Color = Level->Style->Get<CFloat4>(L"Text/Color/Normal");
				
				if(Text)
				{
					Text->SetColor(Color);
				}
			}

			void SetEnity(CHistoryItem * e)
			{
				if(e)
				{
					Entity = e;
					SetEntity(e->Object.Url);
				} 
				else
				{
					Entity = e;
					/// CAvatarCard will handle the rest 
				}
			}
			
			void OnMouse(CActive * r, CActive * s, CMouseArgs * arg)
			{
				if((arg->Control == EMouseControl::LeftButton || arg->Control == EMouseControl::MiddleButton) && arg->Event == EGraphEvent::Click)
				{
					Level->Nexus->Open(Entity->Object.Url, sh_new<CShowParameters>(arg, Level->Style));
					arg->StopPropagation = true;
				}

				if(s == Active)
				{
					if(arg->Event == EGraphEvent::Enter)
					{
						Text->SetColor(Level->Style->Get<CFloat4>(L"Selection/Border/Material"));
					}
	
					if(arg->Event == EGraphEvent::Leave)
					{
						Text->SetColor(Color);
					}
				} 

				if(arg->Event == EGraphEvent::Captured && arg->Capture->InputAs<CMouseInput>()->Control == EMouseControl::RightButton)
				{
					Level->World->Drag(CArray<CDragItem>{CDragItem(Entity->Object.Url, (CUrl)Entity->Object.Url)});
				}
			}

			void OnTouch(CActive * r, CActive * s, CTouchArgs * arg)
			{
				if(arg->Event == EGraphEvent::Click)
				{
					Level->Nexus->Open(Entity->Object.Url, sh_new<CShowParameters>(arg, Level->Style));
					arg->StopPropagation = true;
				}
			}

	};
}
