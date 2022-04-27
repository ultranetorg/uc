#include "StdAfx.h"
#include "InputSystem.h"

using namespace uc;

CInputSystem::CInputSystem(CEngineLevel * l, CScreenEngine * se) : CEngineEntity(l)
{
	ScreenEngine = se;

	Devices.push_back(new CMouse(l, se, this));
	Devices.push_back(new CKeyboard(l, this));

	int caps = GetSystemMetrics(SM_DIGITIZER);

	se->ScreenAdded +=	[this, caps](auto s)
						{
							if(auto ws = s->As<CWindowScreen>())
							{
								if(caps & NID_READY)
								{
									auto ts = new CTouchScreen(Level, this, ws);
									Devices.push_back(ts);
								}

								ws->MessageRecieved +=	[this](auto & m)
														{
															ProcessMessage(&m);
														};
							}
						};
}

CInputSystem::~CInputSystem()
{
	for(auto i : Devices)
	{
		delete i;
	}
}

bool CInputSystem::ProcessMessage(MSG * msg)
{
    const LONG_PTR c_SIGNATURE_MASK = 0xFFFFFF00;
    const LONG_PTR c_MOUSEEVENTF_FROMTOUCH = 0xFF515700;

	auto touch = (GetMessageExtraInfo() & c_SIGNATURE_MASK) == c_MOUSEEVENTF_FROMTOUCH;

	if(WM_MOUSEFIRST <= msg->message && msg->message <= WM_MOUSELAST && !touch)
	{
		if(auto d = First<CMouse>())
		{
			d->ProcessMessage(msg);
		}
	}
	else if(WM_KEYFIRST <= msg->message && msg->message <= WM_KEYLAST)
	{
		if(auto d = First<CKeyboard>())
		{
			d->ProcessMessage(msg);
		}
	}
	else if(msg->message == WM_TOUCH)
	{
		for(auto i : Devices)
		{
			if(auto d = i->As<CTouchScreen>())
			{
				if(d->Screen->Hwnd == msg->hwnd)
				{
					d->ProcessMessage(msg);
					break;
				}
			}
		}
	}

	return false;
}

void CInputSystem::SendInput(CInputMessage * m)
{
	if(InSending)
	{
		m->Take();
		Level->Core->DoUrgent(this, L"Deferred Input", [this, m]
													 {
														SendInput(m); 
														m->Free();
														return true; 
													 });
	}
	else
	{
		InSending = true;
		Recieved(m);
		InSending = false;

	}
}

