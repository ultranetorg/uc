#include "StdAfx.h"
#include "ImageExtractor.h"

using namespace uc;
using namespace std;

typedef BOOL	(* FFileIconInit)(BOOL);

CImageExtractor::CImageExtractor(CWorldLevel * l, CServer * server)
{
	Level = l;
	Level->Core->AddWorker(this);
	Level->Core->ExitRequested += ThisHandler(OnCoreExitQueried);

	Diagnostics	= Level->Core->Supervisor->CreateDiagnostics(GetClassName());
	Diagnostics->Updating += ThisHandler(OnDiagnosticsUpdate);


	Notfound24 = Level->Engine->TextureFactory->CreateTexture();
	auto s = Level->Storage->OpenReadStream(server->MapPath(L"Notfound-24x24.png"));
	Notfound24->Load(s);
	Level->Storage->Close(s);

	Notfound48 = Level->Engine->TextureFactory->CreateTexture();
	s = Level->Storage->OpenReadStream(server->MapPath(L"Notfound-48x48.png"));
	Notfound48->Load(s);
	Level->Storage->Close(s);

	//IImageList * ImageList48;
	//HRESULT r = SHGetImageList(SHIL_EXTRALARGE, IID_IImageList, (void**)&ImageList48);

	//if(r != S_OK) 
	//	throw CException(HERE, L"Can't get system image list");

	//SetEnvironmentVariable(L"windir", L"c:\\\\Windows");

	Thread = Level->Core->RunThread(L"Icon extraction",	[this]
														{
															CoInitialize(NULL);
								
															while(!Level->Core->Exiting)
															{

																decltype(Fetching) jobs;
																{
																	lock_guard<mutex> guard(Lock);
																	jobs = Fetching;
																}
	
																for(auto j : jobs)
																{
																	auto l = Resolve(j->Path);

																	SHGetFileInfo(j->Path.c_str(), 0, &j->Sfi, sizeof(j->Sfi), SHGFI_SYSICONINDEX);
																}
									
																bool empty;
																{
																	lock_guard<mutex> guard(Lock);

																	for(auto j : jobs)
																	{
																		Ready.push_back(j);
																		Fetching.remove(j);
																	}

																	empty = Fetching.empty();
																}

																if(empty)
																{
																	FetcherWakeuper.wait(unique_lock<mutex>(FetcherMutex));
																}
															}
														},
														[]{}
														);
}
	
CImageExtractor::~CImageExtractor()
{
	DoIdle();
	
	for(auto i : Ready)
		delete i;

	for(auto i : Fetching)
		delete i;

	for(auto i : Textures)
		i.second->Free();

	for(auto i : Materials)
		i.second->Free();

	for(auto & i : Images)
		for(auto & j : i.second)
			delete j.second;

	Notfound24->Free();
	Notfound48->Free();

	Level->Core->RemoveWorker(this);
	Level->Core->ExitRequested -= ThisHandler(OnCoreExitQueried);

	Diagnostics->Updating -= ThisHandler(OnDiagnosticsUpdate);
}

void CImageExtractor::OnDiagnosticsUpdate(CDiagnosticUpdate & a)
{
	Diagnostics->Add(L"Fetching : %d", Fetching.size());
	Diagnostics->Add(L"Ready    : %d", Ready.size());
}

void CImageExtractor::OnCoreExitQueried()
{
	FetcherWakeuper.notify_all();
}

CTexture * CImageExtractor::GetNotfound(int wh)
{
	if(0 < wh && wh <= 24)
		return Notfound24;
	else
		return Notfound48;
}

void CImageExtractor::DoIdle()
{
	decltype(Ready) jobs;
	{
		lock_guard<mutex> guard(Lock);
		jobs = Ready;
		for(auto i : jobs)
		{
			Ready.remove(i);
		}
	}

	for(auto i : jobs)
	{
		if(i->Image)
			i->ImageReady();
		else
			i->IconReady();

		delete i;
	}
}

void CImageExtractor::FetchIcon(CUrl & f, int wh, CGetIconMaterialJob * j)
{
	if(CUol::IsValid(f))
		j->Path = Level->Nexus->UniversalToNative(CUol(f).GetId());
	else
		throw CException(HERE, L"Not implemented");

	auto ext = CPath::GetExtension(CUol(f).GetId());

	bool custom = ext == L"exe" || ext == L"msi" || ext == L"lnk" || CNativeDirectory::Exists(j->Path);

	if(!custom && Types[wh].Contains(ext))
	{
		lock_guard<mutex> guard(Lock);
		j->Image = Types[wh][ext];
		Ready.push_back(j);
		return;
	}
	
	j->IconReady =	[this, wh, j, ext, custom]
					{
						if(j->Sfi.iIcon == -1)
						{
							goto end;
						}

						if(Images[wh].Contains(j->Sfi.iIcon))
						{
							j->Image = Images[wh][j->Sfi.iIcon];
							goto end;
						}

						IShellItemImageFactory * iif; 

						HRESULT r = SHCreateItemFromParsingName(j->Path.c_str(), NULL, IID_PPV_ARGS(&iif));
						if(!iif)
							SHCreateItemFromParsingName(j->Path.Replace(L" (x86)", L"").c_str(), NULL, IID_PPV_ARGS(&iif));

						if(!iif)
						{
							Level->Log->ReportWarning(this, L"Can't extract icon for %s", j->Path.c_str());
							goto end;
						}
						
						auto swh = int(float(wh) * Level->Engine->ScreenEngine->Scaling.x); 

						HBITMAP b;
						iif->GetImage(SIZE{swh, swh}, SIIGBF_ICONONLY, &b);
						iif->Release();

						if(!b)
						{
							Level->Log->ReportWarning(this, L"Can't extract icon for %s", j->Path.c_str());
							goto end;
						}
							
						/*
						BITMAP bm;
						GetObject(b, sizeof(bm), &bm);
						*/

						//ImageList48


						BITMAP bmp;
						ZeroMemory(&bmp, sizeof(bmp));
						::GetObject(b, sizeof(bmp), &bmp);


						BITMAPV5HEADER bi;
						ZeroMemory(&bi, sizeof(BITMAPV5HEADER));
						bi.bV5Size = sizeof(BITMAPV5HEADER);
						bi.bV5Width = bmp.bmWidth;
						bi.bV5Height = -bmp.bmHeight;
						bi.bV5Planes = 1;
						bi.bV5BitCount = 32;
						bi.bV5Compression = BI_BITFIELDS;
						bi.bV5RedMask   = 0x00FF0000;
						bi.bV5GreenMask = 0x0000FF00;
						bi.bV5BlueMask  = 0x000000FF;
						bi.bV5AlphaMask = 0xFF000000; 

						PUCHAR bitdata = 0;

						HDC screenDC = GetDC(null);
						HDC tmpDC = CreateCompatibleDC(screenDC);
						HBITMAP tmpBMP = CreateDIBSection(screenDC, (BITMAPINFO *)&bi, DIB_RGB_COLORS, (void **)&bitdata, 0, 0);
						HGDIOBJ oldBMP = SelectObject(tmpDC, tmpBMP);

						/*
						IMAGELISTDRAWPARAMS ildp;
						ildp.cbSize = sizeof(IMAGELISTDRAWPARAMS);
						ildp.himl = *((HIMAGELIST *)ImageList);
						ildp.i = sfi.iIcon;
						ildp.hdcDst = tmpDC;
						ildp.x = 0;
						ildp.y = 0;
						ildp.cx = w;
						ildp.cy = h;
						ildp.xBitmap = 0;
						ildp.yBitmap = 0;
						ildp.rgbBk = CLR_NONE;
						ildp.rgbFg = CLR_NONE;
						ildp.fStyle = ILD_TRANSPARENT;
						ildp.dwRop = 0;
						ildp.fState = ILS_NORMAL;
						ildp.Frame = 0;
						ildp.crEffect = 0;

						ImageList->Draw(&ildp);*/

						HDC iconDC = CreateCompatibleDC(screenDC);
						HGDIOBJ oldiconBMP = SelectObject(iconDC, b);

						BitBlt(tmpDC, 0, 0, bmp.bmWidth, bmp.bmHeight, iconDC, 0, 0, SRCCOPY);

						CImage * i = new CImage();
						i->Pixels.Set(bitdata, bmp.bmWidth * bmp.bmHeight * bi.bV5BitCount/8);
						i->Size.W = float(bmp.bmWidth);
						i->Size.H = float(bmp.bmHeight);
						i->Size.D = 0;
						i->BytesPerPixel = bi.bV5BitCount/8;


						SelectObject(iconDC, oldiconBMP);
						DeleteDC(iconDC);
						DeleteObject(b);

						SelectObject(tmpDC, oldBMP);
						DeleteObject(tmpBMP);
						DeleteDC(tmpDC);
						ReleaseDC(NULL, screenDC);  

						Images[wh][j->Sfi.iIcon] = i;
						j->Image = i;

						if(!custom && !Types[wh].Contains(ext))
						{
							Types[wh][ext] = i;
						}

					end:
						j->ImageReady();
					};


	lock_guard<mutex> guard(Lock);
	Fetching.push_back(j);
	FetcherWakeuper.notify_all();
}

CGetIconMaterialJob * CImageExtractor::GetIconMaterial(IType * r, CUrl & f, int wh)
{
	auto j = new CGetIconMaterialJob();
	
	j->Requester = r;

	j->ImageReady =	[this, j]
					{
						if(j->Image)
						{
							if(Materials.Contains(j->Image))
							{
								j->Material = Materials[j->Image];
							}
							else
							{
								auto t = GetIconTexture(j->Image);

								if(t)
								{
									auto m = new CMaterial(Level->Engine->Level, Level->Engine->PipelineFactory->DiffuseTextureShader);
									m->AlphaBlending = true;
									m->Textures[L"DiffuseTexture"] = t;

									Materials[j->Image] = m;
									j->Material = m;
								}
							}
						}

						j->Done(j->Material);

						//lock_guard<mutex> guard(Lock);
						//ReadyJobs.push_back(j);
					};

	FetchIcon(f, wh, j);

	return j;
}

CTexture * CImageExtractor::GetIconTexture(CImage * i)
{
	if(Textures.Contains(i))
	{
		return Textures[i];
	}

	auto t = Level->Engine->TextureFactory->CreateTexture();
	t->Create(int(i->Size.W), int(i->Size.H), 0, 1, ETextureFeature::Load, DXGI_FORMAT_B8G8R8A8_UNORM);
	t->LoadPixels(i->Pixels.GetData(), int(i->BytesPerPixel * i->Size.W));

	//t->SaveToFile(Level->Core->SuperVisor->GetPathToDataFolder(L"Icon-" + CPath::Normalize(CPath::GetFileName(lnk)) + L".dds"));
	Textures[i] = t;

	return t;
}

CSolidRectangleMesh * CImageExtractor::GetMesh(float w, float h)
{
	auto m = Meshes.Find([w, h](auto i){ return i->BBox.GetWidth() == w && i->BBox.GetHeight() == h; });

	if(!m)
	{
		m = new CSolidRectangleMesh(Level->Engine->Level);
		m->Generate(0, 0, w, h);
		Meshes.AddNew(m);
	} 
	return m;
}

CLnk CImageExtractor::Resolve(CString const & lp)
{
	CLnk o;

	if(CNativePath::GetFileExtension(lp).EqualsInsensitive(L".lnk"))
	{
		HRESULT hres;
		IShellLink* psl;
		WCHAR tmp[MAX_PATH];
		//WCHAR szDescription[MAX_PATH];
		WIN32_FIND_DATA wfd;

		o.Name = CNativePath::GetFileNameBase(lp);

		// Get a pointer to the IShellLink interface. It is assumed that CoInitialize
		// has already been called. 
		hres = CoCreateInstance(CLSID_ShellLink, NULL, CLSCTX_INPROC_SERVER, IID_IShellLink, (LPVOID*)&psl);
		if(SUCCEEDED(hres))
		{
			IPersistFile* ppf;

			// Get a pointer to the IPersistFile interface. 
			hres = psl->QueryInterface(IID_IPersistFile, (void**)&ppf);

			if(SUCCEEDED(hres))
			{

				// Load the shortcut. 
				hres = ppf->Load(lp.data(), STGM_READ);

				if(SUCCEEDED(hres))
				{
					// Resolve the link. 
					hres = psl->Resolve(null, SLR_NO_UI);


					if(SUCCEEDED(hres))
					{
						
						// Get the path to the link target. 
						hres = psl->GetPath(tmp, MAX_PATH, (WIN32_FIND_DATA*)&wfd, 0);

						if(SUCCEEDED(hres))
						{
							o.Target = tmp;
							
							
							// Get the description of the target. 
							//hres = psl->GetDescription(szDescription, MAX_PATH);

						}

						hres = psl->GetIconLocation(tmp, MAX_PATH, &o.IconIndex);
						if(SUCCEEDED(hres))
						{
							o.IconSource = tmp;
						}
								
					}
				}

				// Release the pointer to the IPersistFile interface. 
				ppf->Release();
			}

			// Release the pointer to the IShellLink interface. 
			psl->Release();
		}

		return o;
	} 
	else
	{
		o.Target = lp;
		o.IconSource = L"";
		o.IconIndex = 0;
	}

	return o;
}

bool CImageExtractor::IsRequested(IType * r)
{
	lock_guard<mutex> guard(Lock);

	return Ready.Has([r](auto i){ return i->Requester == r; }) || Fetching.Has([r](auto i){ return i->Requester == r; });
}