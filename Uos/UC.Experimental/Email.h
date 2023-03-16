#pragma once
#include "EmailAccount.h"
#include "Vmime.h"

namespace uc
{
	class CEmailMessage
	{
		public:
			CString				From;
			CString				To;
			CString				Subject;
			CDateTime			Date;
			bool				IsRead;

			void Save(CXon * n)
			{
				n->Add(L"From")->Set(From);
				n->Add(L"To")->Set(To);
				n->Add(L"Subject")->Set(Subject);
				n->Add(L"Date")->Set(Date);
				n->Add(L"IsRead")->Set(IsRead);
			}
			void Load(CXon * n)
			{
				To		= n->One(L"To")->Get<CString>();
				From	= n->One(L"From")->Get<CString>();
				Subject	= n->One(L"Subject")->Get<CString>();
				Date	= n->One(L"Date")->Get<CDateTime>();
				IsRead	= n->One(L"IsRead")->Get<CBool>();
			}
	};

	class CEmail : public virtual CWorldEntity
	{
		public:
			CObject<CEmailAccount>						Account;

			CList<CEmailMessage *>						Messages;
			CExperimentalLevel *						Level;
			vmime::shared_ptr<vmime::net::session>		Session = vmime::net::session::create();
			vmime::shared_ptr<CCertificateVerifier>		CertificateVerifier;
			CEvent<CEmailMessage *>						MessageRecieved;
			std::mutex									MessagesLock;

			CDateTime									LastRecieved = CDateTime::Min;
			bool										Connecting = false;

			UOS_RTTI
			CEmail(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CEmail();

			void										SetEntity(CUol & e);

			void										SaveInstance() override;
			void										LoadInstance() override;

			void										Retrieve();

///			static const std::string FindAvailableProtocols(const vmime::net::service::Type type)
///			{
///				auto sf = vmime::net::serviceFactory::getInstance();
///
///				std::ostringstream res;
///				size_t count = 0;
///
///				for(size_t i = 0 ; i < sf->getServiceCount() ; ++i)
///				{
///					auto & serv = *sf->getServiceAt(i);
///
///					if(serv.getType() == type)
///					{
///						if (count != 0)
///						{
///							res << ", ";
///						}
///
///						res << serv.getName();
///						++count;
///					}
///				}
///
///				return res.str();
///			}

	};

}