#pragma once
#include "ShellLevel.h"
#include "Field.h"

namespace uc
{
	class CFieldItemElement : public CAvatarCard
	{
		public:

			CObject<CFieldItem>							Entity;

			CShellLevel *								Level;
			CFieldWorld *								Operations;
			CString										Path;
			CString										VisualPath;
			CVisual *									Selection;
			CPositioningCapture							Capture;

			UOS_RTTI
			CFieldItemElement(CShellLevel * l, CFieldWorld * fo, CString const & dir);
			virtual ~CFieldItemElement();

			using CAvatarCard::UpdateLayout;
			using CAvatarCard::SetEntity;
			using CAvatarCard::Load;
			using CAvatarCard::Save;


			void										ResizeSelection(float w, float h);

			void										SetItem(CString const & type, CFieldItem * fi);
			void										Save(IMeshStore * mhs, IMaterialStore * mts);
			void										Load(IMeshStore * mhs, IMaterialStore * mts, CFieldServer * f);
			void										Revive();
			void										Delete();

			virtual void								UpdateLayout(CLimits const & l, bool apply) override;
			virtual void								PropagateLayoutChanges(CElement * s) override;

			void										OnDependencyDestroying(CNexusObject *);

			bool										IsSelected();
			void										Select(bool e);

	};
}