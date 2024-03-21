#pragma once
#include "Tray.h"

namespace uc
{
	class CTrayItemElement : public CCard
	{
		public:
			CShellLevel *								Level;
			CTrayItem *									Entity = null;

			UOS_RTTI
			CTrayItemElement(CShellLevel * l) : CCard(l->World, GetClassName())
			{
				Level = l;

				Type = AVATAR_ICON2D;

				SetTitleMode(ECardTitleMode::Right);

				Active->MouseEvent[EListen::NormalAll] += ThisHandler(OnMouse);
			}

			~CTrayItemElement()
			{
			}

			void SetEnity(CTrayItem * e)
			{
				if(e && !Entity)
				{
					Entity = e;
					Entity->IconChanged += ThisHandler(OnIconChanged);
					Entity->TitleChanged += ThisHandler(OnTitleChanged);
					Entity->MarkedOld += ThisHandler(OnMarkedOld);
	
					Text->SetText(e->Title);
					
					if(e->Old)
					{
						Text->SetColor(Level->Style->Get<CFloat4>(L"Text/Color/Disabled"));
					}
				} 
				else if(!e && Entity)
				{
					Entity->IconChanged -= ThisHandler(OnIconChanged);
					Entity->TitleChanged -= ThisHandler(OnTitleChanged);
					Entity->MarkedOld -= ThisHandler(OnMarkedOld);
					Entity = null;
				}
			}

			void OnIconChanged(CTrayItem * ti)
			{
				PropagateLayoutChanges(this);
			}

			void OnTitleChanged(CTrayItem * ti)
			{
				if(Text)
				{
					Text->SetText(ti->Title);
					PropagateLayoutChanges(Text);
				}
			}

			void OnMarkedOld(CTrayItem * ti)
			{
				if(Text)
				{
					Text->SetColor(Level->Style->Get<CFloat4>(L"Text/Color/Disabled"));
				}
			}
			
			void OnMouse(CActive * r, CActive * s, CMouseArgs * arg)
			{
				if(arg->Control == EMouseControl::LeftButton && arg->Event == EGraphEvent::Click)
				{
					arg->StopPropagation = true;
				}


				if(s == Active)
				{
					if(arg->Event == EGraphEvent::Enter)
					{
					}
	
					if(arg->Event == EGraphEvent::Leave)
					{
					}
				} 

				///if(a.Type == EMouseEventType::Captured && a.Capture.Message.Sender == EInputSender::RightButton)
				///{
				///	Level->World->Drag(CArray<CDragItem>{CDragItem(Entity->Object.Url, (CUrl)Entity->Object.Url)});
				///}
			}

	};
}
