#pragma once
#include "Email.h"

namespace uc
{
	class CEmailWidget : public CWidgetWindow
	{
		public:
			CExperimentalLevel *						Level;
			CEmail *									Entity;
			CGrid *										Grid;
			
			UOS_RTTI
			CEmailWidget(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CEmailWidget();

			virtual void								Place(CFieldWorld * fo) override;

			void										SetEntity(CUol & e);
			void										OnDependencyDestroying(CInterObject *);

			void										OnMessageRecieved(CEmailMessage * o);
			void										OnMouse(CActive * r, CActive * s, CMouseArgs * a) override;
	};
}
