#pragma once
#include "ImageExtractor.h"

namespace uc
{
	struct CMergeMeta : public IType, public CShared
	{
		UOS_RTTI
		CList<CString>				Sources;
		CString						Object;

		~CMergeMeta()
		{
		}
	};

	struct CShellLevel
	{
		CProtocolConnection<CWorld>				World;
		CProtocolConnection<IFileSystem>		Storage;
		CEngine *								Engine;
		CStorableServer *						Server;
		CCore *									Core;
		CNexus *								Nexus;
		CImageExtractor *						ImageExtractor;
		CStyle *								Style;
		CLog *									Log;

		void AddModeSwitch(IMenuSection * s)
		{
			auto cmd = World.Server->Instance + L"{";

			#ifndef _DEBUG // copy World command options
				for(auto & i : Core->FindStartCommands(World->Server->Url))
				{
					for(auto & j : i.Query)
					{
						cmd.Query[j.first] = j.second;
					}
				}
			#endif

			auto dm = s->AddSectionItem(L"Switch to Desktop Mode");
			dm->GetSection()->AddItem(L"Default")->Clicked	=	[this, cmd](auto, auto) mutable
																{ 
																	cmd += L"Name=" + CWorldMode::DESKTOP;
																	cmd += L" Layout=Default";
																	Core->AddRestartCommand(cmd + L"}");
																	Core->Exit();
																};
			dm->GetSection()->AddItem(L"Duo")->Clicked	=		[this, cmd](auto, auto) mutable
																{ 
																	cmd += L"Name=" + CWorldMode::DESKTOP;
																	cmd += L" Layout=Duo";
																	Core->AddRestartCommand(cmd + L"}");
																	Core->Exit();
																};

			#ifdef _DEBUG
				dm->GetSection()->AddItem(L"MWS1")->Clicked	=		[this, cmd](auto, auto) mutable
																	{ 
																		cmd += L"Name=" + CWorldMode::DESKTOP;
																		cmd += L" Layout=MWS1";
																		Core->AddRestartCommand(cmd + L"}");
																		Core->Exit();
																	};
				dm->GetSection()->AddItem(L"MWS1.Split")->Clicked =	[this, cmd](auto, auto) mutable
																	{ 
																		cmd += L"Name=" + CWorldMode::DESKTOP;
																		cmd += L" Layout=MWS1.Duo.Split";
																		Core->AddRestartCommand(cmd + L"}");
																		Core->Exit();
																	};
				dm->GetSection()->AddItem(L"MWS7")->Clicked =		[this, cmd](auto, auto) mutable
																	{ 
																		cmd += L"Name=" + CWorldMode::DESKTOP;
																		cmd += L" Layout=MWS7";
																		Core->AddRestartCommand(cmd + L"}");
																		Core->Exit();
																	};
				dm->GetSection()->AddItem(L"MWS7 0.4")->Clicked =	[this, cmd](auto, auto) mutable
																	{ 
																		cmd += L"Name=" + CWorldMode::DESKTOP;
																		cmd += L" Layout=MWS7 ";
																		cmd += CScreenEngine::RENDER_SCALING + L"=0.4";
																		Core->AddRestartCommand(cmd + L"}");
																		Core->Exit();
																	};
			#endif

			auto mm = s->AddSectionItem(L"Switch to Mobile Emulation Mode");
			mm->GetSection()->AddItem(L"Default")->Clicked =	[this, cmd](auto, auto) mutable
																{ 
																	cmd += L"Name=" + CWorldMode::MOBILE_E;
																	cmd += L" Layout=Default";
																	Core->AddRestartCommand(cmd + L"}");
																	Core->Exit();
																};

			#ifdef _DEBUG
				mm->GetSection()->AddItem(L"MWS1")->Clicked =		[this, cmd](auto, auto) mutable
																	{ 
																		cmd += L"Name=" + CWorldMode::MOBILE_E;
																		cmd += L" Layout=MWS1";
																		Core->AddRestartCommand(cmd + L"}");
																		Core->Exit();
																	};
				mm->GetSection()->AddItem(L"MWS7")->Clicked =		[this, cmd](auto, auto) mutable
																	{ 
																		cmd += L"Name=" + CWorldMode::MOBILE_E;
																		cmd += L" Layout=MWS7";
																		Core->AddRestartCommand(cmd + L"}");
																		Core->Exit();
																	};
				mm->GetSection()->AddItem(L"MWS7-0.4")->Clicked =	[this, cmd](auto, auto) mutable
																	{ 
																		cmd += L"Name=" + CWorldMode::MOBILE_E;
																		cmd += L" Layout=MWS7 ";
																		cmd += CScreenEngine::RENDER_SCALING + L"=0.4";
																		Core->AddRestartCommand(cmd);
																		Core->Exit();
																	};
			#endif

			auto vm = s->AddSectionItem(L"Switch to VR Emulation Mode");
			vm->GetSection()->AddItem(L"Default")->Clicked =	[this, cmd](auto, auto) mutable
																{ 
																	cmd += L"Name=" + CWorldMode::VR_E;
																	cmd += L" Layout=Default";
																	Core->AddRestartCommand(cmd + L"}");
																	Core->Exit();
																};
			#ifdef _DEBUG
				vm->GetSection()->AddItem(L"MWS1")->Clicked =		[this, cmd](auto, auto) mutable
																	{ 
																		cmd += L"Name=" + CWorldMode::VR_E;
																		cmd += L" Layout=MWS1";
																		Core->AddRestartCommand(cmd + L"}");
																		Core->Exit();
																	};
				vm->GetSection()->AddItem(L"MWS7")->Clicked =		[this, cmd](auto, auto) mutable
																	{ 
																		cmd += L"Name=" + CWorldMode::VR_E;
																		cmd += L" Layout=MWS7";
																		Core->AddRestartCommand(cmd + L"}");
																		Core->Exit();
																	};
				vm->GetSection()->AddItem(L"MWS7-0.4")->Clicked =	[this, cmd](auto, auto) mutable
																	{ 
																		cmd += L"Name=" + CWorldMode::VR_E;
																		cmd += L" Layout=MWS7 ";
																		cmd += CScreenEngine::RENDER_SCALING + L"=0.4";
																		Core->AddRestartCommand(cmd);
																		Core->Exit();
																	};
			#endif
		}
		
		void AddSystemMenuItems(CRectangleMenuSection * ms)
		{
			auto syses = Nexus->ConnectMany<IWorldFriend>(Server);

			for(auto & i : syses)
			{
				for(auto i : i->CreateActions())
				{
					AddMenuItem(ms, i);
				}
			}

			Nexus->Disconnect(syses);
			
			ms->AddSeparator();
			
			AddModeSwitch(ms);

			ms->AddSeparator();

			if(Core->IsAdministrating)
			{
				ms->AddItem(L"Extact CRT dlls to C:\\")->Clicked =	[this](auto, auto)
																	{
																		HMODULE hMods[1024];
																	
																		DWORD cbNeeded;
																		unsigned int i;
	
																		// Get a handle to the process.

																		auto hProcess = OpenProcess(PROCESS_QUERY_INFORMATION |PROCESS_VM_READ, FALSE, GetCurrentProcessId());

																		if(EnumProcessModules(hProcess, hMods, sizeof(hMods), &cbNeeded))
																		{
																			for(i = 0; i < (cbNeeded / sizeof(HMODULE)); i++)
																			{
																				TCHAR szModName[MAX_PATH];

																				if(GetModuleFileNameEx(hProcess, hMods[i], szModName, sizeof(szModName) / sizeof(TCHAR)))
																				{
																					auto name = CNativePath::GetFileName(szModName).ToLower();
																					if(name.StartsWith(L"api-") || name.StartsWith(L"ucrtbase") || name.StartsWith(L"msvcp") || name.StartsWith(L"vcruntime"))
																					{
																						CopyFile(szModName, (L"C:\\" + name).data(), FALSE);
																					}
																				}
																			}
																		}
	    
																		// Release the handle to the process.
	
																		CloseHandle( hProcess );
																	};
	
				ms->AddItem(L"Exit Without Saving")->Clicked =	[this](auto, auto)
																{
																	Core->CommitDatabase = false;
																	Core->Exit();
																};

			}
				
			ms->AddItem(L"Exit")->Clicked =	[this](auto, auto)
													{ 
														Core->Exit();  
													};
		}

		CRectangleTextMenuItem * AddMenuItem(IMenuSection * ms, CMenuItem * item)
		{
			CRectangleTextMenuItem * mi;

			if(item->Execute)
			{
				mi = new CRectangleTextMenuItem(World, Style, nullptr, item->Label);

				mi->Clicked =	[item](auto args, auto mi)
								{
									item->Execute(args);
								};
			} 
			else
			{
				auto si = new CRectangleSectionMenuItem(World, Style, item->Label);
				
				auto ms = dynamic_cast<CRectangleMenuSection *>(si->Section);
				ms->Express(L"IW", [ms](){ return min(ms->C.W, 200); });
				
				si->Section->Opening +=	[this, item, si](auto)
										{
											if(item->Opening)
											{
										 		item->Opening();
											}
											
											si->Section->Clear();

											for(auto i : item->Items)
											{
												AddMenuItem(si->Section, i);
											}
										};

				si->Active->SetMeta(item);
				mi = si;
			}

			if(!item->IconEntity.IsEmpty())
			{
				ImageExtractor->GetIconMaterial(mi, (CUrl)item->IconEntity, 16)->Done = [mi](auto m){  mi->Icon->Visual->SetMaterial(m);  }; 
			}

			mi->Active->SetMeta(item);
			ms->AddItem(mi);
			mi->Free();

			return mi;
		}

		CRefList<CMenuItem *> LoadLinks(CList<CString> & sources)
		{
			CRefList<CMenuItem *> out;

			for(auto & o : sources)
			{
				for(auto & i : Storage->Enumerate(o, L".*"))
				{
					auto path = CPath::Join(o, i.Name);

					CMenuItem * existing = null;

					existing = out.Find([i](auto k)
										{
											return k->MetaAs<CMergeMeta>()->Sources.Has([i](auto j) mutable
																						{
																							return CPath::GetName(j) == i.Name; 
																						}); 
										});
					
					if(existing)
					{
						auto meta = existing->MetaAs<CMergeMeta>();
						meta->Sources.push_back(path);
					}
					else
					{
						auto meta = new CMergeMeta();
						meta->Sources.push_back(path);
						meta->Object = path;

						auto & name = CPath::GetNameBase(i.Name);

						if(!i.NameOverride.empty())
						{
							name = i.NameOverride; 
						}

						auto mi = new CMenuItem(name);
						mi->Meta		= meta;
						mi->IconEntity	= Storage->ToUol(path);

						if(i.Type == CFileSystemEntry::Directory)
						{
							mi->Opening =	[this, mi, meta]
											{
												mi->Items = LoadLinks(meta->Sources);
											};
						}
						else
						{
							mi->Execute =	[=](auto args)
											{
												Nexus->Execute(&CTonDocument(CXonTextReader(L"Open Url=" + Storage->ToUol(path).ToString())), sh_new<CShowParameters>(args, Style));
											};
						}
						
						out.Add(mi);
						mi->Free();
					}
				}
			}

			out.Sort(	[](auto a, auto b)
						{
							if(bool(a->Execute) != bool(b->Execute))
								return !bool(a->Execute) && bool(b->Execute);

							return a->Label < b->Label;
						});

			return out;
		}
	};
}
