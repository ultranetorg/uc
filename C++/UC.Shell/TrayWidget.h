#pragma once
#include "Tray.h"
#include "TrayItemElement.h"

namespace uc
{
	class CTrayWidget : public CFieldableModel
	{
		public:
			CShellLevel *								Level;
			CListbox *									Listbox;
			CRectangle *								Panel;
			CObject<CTray>								Entity;

			UOS_RTTI
			CTrayWidget(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CTrayWidget();

			void										OnDependencyDestroying(CNexusObject * o);

			void										SetEntity(CUol & o) override;

			virtual void								SaveInstance() override;
			virtual void								LoadInstance() override;

			virtual void								Open(CWorldCapabilities * caps, CUnit * a) override;

			void										OnItemAdded(CTrayItem * hi);
			void										OnItemRemoved(CTrayItem * hi);

			void										UpdateLayout(CLimits const & l, bool apply) override;
	};
}
