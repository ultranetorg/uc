#pragma once
#include "ExperimentalLevel.h"

namespace uc
{
	class CGeoMaterial
	{
		public:
			CExperimentalLevel * 	Level;
			CString					Name;
			CMaterial *				Material;
			CTexture *				Texture;
			CEvent<>				Loaded;
			CHttpRequest *			Request = null;
			bool					Ready = false;

			CGeoMaterial(CExperimentalLevel * l, CString const & name)
			{
				Level = l;
				Name = name;

				Texture = Level->Engine->TextureFactory->CreateTexture();
				Material = new CMaterial(Level->Engine->Level, Level->Engine->PipelineFactory->DiffuseTextureShader);
				Material->Textures[L"DiffuseTexture"] = Texture;
				Material->Samplers[L"DiffuseSampler"].SetAddressMode(ETextureAddressMode::Clamp, ETextureAddressMode::Clamp);
			}

			~CGeoMaterial()
			{
				delete Request;
		
				Material->Free();
				Texture->Free();
			}
	};

	class CGeoStore
	{
		public:
			CExperimentalLevel * 	Level;
			CList<CGeoMaterial *>	Materials;

			CGeoStore(CExperimentalLevel * l)
			{
				Level = l;
			}

			~CGeoStore()
			{
				for(auto i : Materials)
				{
					delete i;
				}
			}

			void RequestTile(int lod, int tx, int ty, CString const & style, std::function<void(CMaterial *)> ok)
			{
				auto name = Level->Server->MapSystemPath(CString::Format(L"Earth/Cache/%s-%d-%d-%d.jpg", style, lod, tx, ty));
					
				if(auto m = Materials.Find([name](auto i){ return i->Name == name; }))
				{
					if(m->Ready)
					{
						ok(m->Material);
					}
					else
					{
						m->Loaded += [ok, m]{ ok(m->Material); };
					}
				}
				else if(Level->Storage->Exists(name))
				{
					auto m = new CGeoMaterial(Level, name);
					Materials.push_back(m);

					auto s = Level->Storage->ReadFileAsync(name);

					s->ReadAsync(	[this, s, name, m, ok]
									{
										m->Texture->Load(s->Buffer.GetData(), s->Buffer.GetSize());
										Level->Storage->Close(s); // self destruction!

										m->Ready = true;
										m->Loaded();

										ok(m->Material);
									});
				}
				else
				{
					auto m = new CGeoMaterial(Level, name);
					Materials.push_back(m);

					m->Request = new CHttpRequest(Level->Core, CString::Format(L"https://api.mapbox.com/styles/v1/mapbox/" + style + L"/tiles/%d/%d/%d?"
																			 "access_token=pk.eyJ1IjoibWlnaHR5d2lsbCIsImEiOiJjanRoMDVodDcyMzlkNDNwOHBvZDlpeG93In0.ZpobQTwDvXWRHg3PwgzaVQ",
																			 lod, tx, ty));
					m->Request->Caching = true;
					m->Request->Recieved =	[this, name, m, ok]
											{
												m->Texture->Load(&m->Request->Stream);
												m->Request->Stream.ReadSeek(0);
	
												auto s = Level->Storage->WriteFile(name);
												s->Write(&m->Request->Stream);
												Level->Storage->Close(s);
	
												m->Ready = true;
												m->Loaded();

												ok(m->Material);
											};
					m->Request->Send();
				}
			}

	};
}