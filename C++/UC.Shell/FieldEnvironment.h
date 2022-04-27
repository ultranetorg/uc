#pragma once
#include "FieldAvatar.h"

using namespace uc;

namespace uc
{ 
	class CFieldEnvironment : public virtual IType
	{
		public:
			virtual	CFieldElement *						GetField()=0;

			UOS_RTTI
			CFieldEnvironment(){}
			virtual ~CFieldEnvironment(){}
	};
	
	class CFieldEnvironmentServer : public CFieldAvatar, public CFieldEnvironment
	{
		public:
			CRectangleSizer								Sizer;

			UOS_RTTI
			CFieldEnvironmentServer(CShellLevel * l, const CString & name = CGuid::Generate64(CFieldEnvironment::GetClassName()));

			using CElement::UpdateLayout;

			CFieldElement *								GetField() override { return FieldElement; };
	};
}

