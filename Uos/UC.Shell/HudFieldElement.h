#pragma once
#include "ShellLevel.h"
#include "FieldElement.h"
#include "ShellFriendProtocol.h"

namespace uc
{
	class CHudFieldElement : public CFieldElement
	{
		public:
			CFieldServer *								Entity = null;
			
			CSolidRectangleMesh *						AMesh;
			CGridRectangleMesh *						VMesh;
			CRectangleMenu *							Menu = null;
			CParagraph *								Info;
			CElement *									ProductInfo;

			UOS_RTTI
			CHudFieldElement(CShellLevel * l);
			~CHudFieldElement();

			using CElement::UpdateLayout;

			void										SetEntity(CFieldServer * f);

			void										OnProductInfoRetrieved(CUpdateData *);

			virtual void								UpdateLayout(CLimits const & l, bool apply) override;
			void										OnStateModified(CActive *, CActive *, CActiveStateArgs *);
			void										OnMouse(CActive * r, CActive * s, CMouseArgs * a);
			void										OnWorldInputAction(EWorldAction wa);

			void										OnAdded(CFieldItemElement * fie);
			void										OnPlacing(CFieldItemElement * fie);

			virtual CFieldItemElement *					AddItem(CFieldItem * fi) override;
	};
}
