#include "stdafx.h"
#include "Email.h"

using namespace uc;

CEmail::CEmail(CExperimentalLevel * l, CString const & name) : CWorldEntity(l->Server, name)
{
	Level = l;

	SetDirectories(MapRelative(L""));
	SetDefaultInteractiveMaster(AREA_MAIN);

	SetTitle(GetClassName());

	CertificateVerifier = vmime::make_shared<CCertificateVerifier>();

	//MessageRecieved +=	[this](auto)
	//					{
	//						SetTitle(CString::Format(L"%d messages", Messages.size()));
	//					};
}

CEmail::~CEmail()
{
	Save();

	std::lock_guard<std::mutex> guard(MessagesLock);
	{
		for(auto i : Messages)
		{
			delete i;
		}
	}
}

void CEmail::SetEntity(CUol & e)
{
	Account = Server->FindObject(e);
	
	if(Account)
	{
		Account->Changed += [this]
							{
								Retrieve(); 
							};
	}
}

void CEmail::Retrieve()
{
	if(Connecting)
		return;
	else
		Connecting = true;

	SetTitle(CString::Format(L"Connecting to %s ...", Account->Server.ToString()));

	Level->Core->RunThread(	Account->Server.ToString(),
							[this]
							{
							
								try
								{
									// If no authenticator is given in argument to getStore(), a default one
									// is used. Its behaviour is to get the user credentials from the
									// session properties "auth.username" and "auth.password".
	
									auto st = Session->getStore(vmime::utility::url(Account->Server.ToString().ToAnsi()), vmime::make_shared<CInteractiveAuthenticator>(Account->User, Account->Password));
	
							#if VMIME_HAVE_TLS_SUPPORT
	
									// Enable TLS support if available
									st->setProperty("connection.tls", true);
	
									// Set the time out handler
									st->setTimeoutHandlerFactory(vmime::make_shared<CTimeoutHandlerFactory>());
	
									// Set the object responsible for verifying certificates, in the
									// case a secured connection is used (TLS/SSL)
									st->setCertificateVerifier(CertificateVerifier);
	
							#endif // VMIME_HAVE_TLS_SUPPORT
	
									// Trace communication between client and server
									auto traceStream = vmime::make_shared <std::ostringstream>();
									st->setTracerFactory(vmime::make_shared<CTracerFactory>(traceStream));
	
									// Connect to the mail store
									st->connect();
	
									// Display some information about the connection
									auto ci = st->getConnectionInfos();
										
									///Level->Log->ReportMessage(this, L"Connected to: %s : %hu - %s", CString::FromAnsi(ci->getHost()), ci->getPort(), st->isSecuredConnection() ? L"SECURED" : L"");
										
	
									// Open the default folder in this store
									auto f = st->getDefaultFolder();
									///		vmime::shared_ptr <vmime::net::folder> f = st->getFolder(vmime::utility::path("a"));
	
									f->open(vmime::net::folder::MODE_READ_WRITE);
	
									auto count = f->getMessageCount();
	
									//auto lastest = CDateTime::Min;
									auto news = CList<CEmailMessage *>();

									for(vmime::size_t i=count; i>=1; i--)
									{
										if(Level->Core->Exiting)
										{
											break;
										}
	
										auto msg = f->getMessage(i);
	
										vmime::net::fetchAttributes attr(vmime::net::fetchAttributes::ENVELOPE);
										// If you also want to fetch "Received: " fields:
										//attr.add("Received");
	
										f->fetchMessage(msg, attr);
	
										// Tue, 14 May 2019 23:19:34 +0000
										auto d = msg->getHeader()->Date()->getValue<vmime::datetime>();

										int year, mon, day, hour, minute, second, zone;
										d->getDate(year, mon, day);
										d->getTime(hour, minute, second, zone);

										auto date = CDateTime(year, mon - 1, day, hour, minute, second, zone);

										//vmime::datetime s;

										//auto r = msg->getHeader()->findAllFields("Received");  // Subject()->getValue<vmime::text>()->getConvertedText("utf-8"));
										//if(r)
										//{
										//	auto v = r->generate();
										//	//if()
										//	{
										//	}
										//}

										if(date > LastRecieved)
										{
											auto m = new CEmailMessage();
											m->From		= CString::FromUtf8(msg->getHeader()->From()->getValue<vmime::mailbox>()->getName().getConvertedText("utf-8"));
											m->To		= CString::FromAnsi(msg->getHeader()->To()->getValue<vmime::addressList>()->generate());
											m->Subject	= CString::FromUtf8(msg->getHeader()->Subject()->getValue<vmime::text>()->getConvertedText("utf-8"));
											m->Date		= date;/*date.ToString(L"%F %T")*/

											news.push_back(m);
											//auto id = CString::FromAnsi(msg->getHeader()->MessageId()->getValue<vmime::messageId>()->generate());
											
											Level->Core->DoUrgent(	this,
																	GetClassName() + L" MessageRecieved",
																	[this, m]
																	{
																		MessageRecieved(m);
																		return true;
																	});
										}
										else
											break;
									}

									if(!news.empty())
									{
										std::lock_guard<std::mutex> guard(MessagesLock);
										Messages.AddFrontMany(news);
										LastRecieved = news.front()->Date;
									}

									Level->Core->DoUrgent(this,
														GetClassName() + L" New Email",
														[this, news]
														{
															SetTitle(CString::Format(L"%d new emails", news.size()));
															return true;
														});


									f->close(true);
									st->disconnect();
								}
								catch(vmime::exception & e)
								{
									Level->Core->DoUrgent(this,
														GetClassName() + L" Connection failed",
														[this, e]
														{
															SetTitle(CString::Format(L"Connection failed: %s", CString::FromAnsi(e.what()) ));
															return true;
														});
								}
							
							},
							[this]
							{
								Connecting = false;
							}
	);
}

void CEmail::SaveInstance()
{
	CTonDocument d;

	d.Add(L"Account")->Set(Account.Url);
	d.Add(L"LastRecieved")->Set(LastRecieved);

	for(auto i : Messages)
	{
		i->Save(d.Add(L"Message"));
	}
	
	SaveGlobal(d, GetClassName() + L".xon");
}

void CEmail::LoadInstance()
{
	CTonDocument d; LoadGlobal(d, GetClassName() + L".xon");

	LastRecieved = d.Get<CDateTime>(L"LastRecieved");

	for(auto i : d.Many(L"Message"))
	{
		auto m = new CEmailMessage();
		m->Load(i);
		Messages.push_back(m);
	}

	SetEntity(d.Get<CUol>(L"Account"));

	if(Account)
	{
		Retrieve();
	}
}
