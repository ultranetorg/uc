#pragma once
#include "AvatarCard.h"
#include "Sizer.h"
#include "World.h"
#include "Fieldable.h"
#include "RectangleMenu.h"

namespace uc
{
	class UOS_WORLD_LINKING CWidgetWindow : public CFieldableModel
	{
		public:
			CWorldProtocol *									World;
			CObject<CWorldEntity>						Entity;
			CStyle *									Style;
			CElement *									Face = null;
			CRectangleSizer 							Sizer;
			CRectangleMenu *							Menu = null;

			UOS_RTTI
			CWidgetWindow(CWorldProtocol * l, CServer * sys, CStyle * s, const CString & name = CGuid::Generate64(GetClassName()));
			~CWidgetWindow();

			void										SetFace(CElement * e);
			virtual void								SetEntity(CUol & o) override;
			void										OnDependencyDestroying(CInterObject *);

			virtual void								SaveInstance() override;
			virtual void								LoadInstance() override;

			virtual void								Place(CFieldWorld * f) override;
			
			virtual void								OnMouse(CActive *, CActive *, CMouseArgs *);

			void DetermineSize(CSize & smax, CSize & s) override;

	};
}
