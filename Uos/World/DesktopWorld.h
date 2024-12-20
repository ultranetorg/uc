#pragma once
#include "WorldServer.h"

namespace uc
{
	class CDesktopWorld : public CWorldServer
	{
		public:
			CPolygonalPositioning *		InteractivePositioning = null;
			CPolygonalPositioning *		BackPositioning = null;

			EWorldAction				TouchStage = EWorldAction::Null;

			CDesktopWorld(CNexus * l, CServerInstance * si);
			~CDesktopWorld();

			void						InitializeViewports() override;
			void						InitializeGraphs() override;
			void						InitializeView() override;
			void						InitializeAreas() override;

			CList<CUnit *>				CollectHidings(CArea * a, CArea * master) override;
			virtual void				OnMouse(CActive * r, CActive * s, CMouseArgs * arg);
			virtual void				OnTouch(CActive * r, CActive * s, CTouchArgs * arg);
			void						OnKeyboard(CActive *, CActive * s, CKeyboardArgs * arg);
			void						OnStateChanged(CActive * r, CActive * s, CActiveStateArgs * arg);
			void						StartMovement(CPick & pk, CInputArgs * arg);
			void						ProcessMovement(CPick & pk, CInputArgs * arg);
			void						FinishMovement(CInputArgs * arg);
	};
}

