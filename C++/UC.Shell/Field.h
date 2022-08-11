#pragma once
#include "ShellLevel.h"

namespace uc
{
	struct CFieldItem : public CStorableObject
	{
		public:
			CUol				Object;
			CString				Complexity;

			UOS_RTTI
			CFieldItem(CServer * s, CString const & complexity, CString const & name = CGuid::Generate64(GetClassName())) : CStorableObject(s, name)
			{
				Complexity = complexity;
			}

			void Save(CXon * p)
			{
				p->Set(Url.Object);
				p->Add(L"Object")->Set(Object);
				p->Add(L"Complexity")->Set(Complexity);
			}

			void Load(CXon * p)
			{
				Object		= p->Get<CUol>(L"Object");
				Complexity	= p->Get<CString>(L"Complexity");
			}
	};

	class CField : public virtual CWorldEntity
	{
		public:
			UOS_RTTI
			CField(CShellLevel * l, CString const & name) : CWorldEntity(l->Server, name)
			{
			}

			virtual	CObject<CFieldItem>					Add(CUol & o, CString const & complexity) = 0;
			virtual CList<CFieldItem *>					Add(CList<CUol> & o, CString const & complexity) = 0;
			virtual	CObject<CFieldItem>					FindByObject(CUol & o) = 0;

	};

	class CFieldServer : public CField
	{
		public:
			CShellLevel *								Level;
			CList<CFieldItem *>							Items;
			CEvent<CFieldItem *>						Added;

			CFieldServer(CShellLevel * l, CString const & name = CGuid::Generate64(CField::GetClassName()));
			virtual ~CFieldServer();

			void										SetTitle(CString const & name);
			CObject<CFieldItem>							FindByObject(CUol & o) override;
			CObject<CFieldItem>							Add(CUol & o, CString const & complexity) override;
			virtual CList<CFieldItem *>					Add(CList<CUol> & o, CString const & complexity) override;
			CObject<CFieldItem>							Find(CUol & o);
			void										Remove(CFieldItem * fi);

			void										SaveInstance() override;
			void										LoadInstance() override;
	};

	class IFieldProtocol
	{
		public:
			virtual CObject<CFieldServer>		GetField(CUol & o)=0;

			virtual ~IFieldProtocol(){}
	};


}