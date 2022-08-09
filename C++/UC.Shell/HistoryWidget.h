#pragma once
#include "HistoryItemElement.h"

namespace uc
{
	class CHistoryWidget : public CFieldableModel
	{
		public:
			using CElement::UpdateLayout;

			CObject<CHistory>							Entity;
			CShellLevel *								Level;
			CListbox *									Listbox;

			UOS_RTTI
			CHistoryWidget(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			virtual ~CHistoryWidget();

			void										OnDependencyDestroying(CBaseNexusObject * o);

			void										SetEntity(CUol & o) override;

			CHistoryItemElement *						AddHistory(CHistoryItem * hi);
			void										RemoveHistory(CHistoryItemElement * h);
			CHistoryItemElement *						CreateItem(CHistoryItem * hi);

			void										OnNodeMouseButton(void *, CActive * r, CActive * s, CMouseArgs * a);

			void										OnItemAdded(CHistoryItem * hi);
			void										OnItemOpened(CHistoryItem * hi);

			virtual void								SaveInstance() override;
			virtual void								LoadInstance() override;

			virtual void								Open(CWorldCapabilities * caps, CUnit * a) override;

			void Place(CFieldWorld * f) override;

			void Adapt(CHistoryItemElement * e);
	};
}
