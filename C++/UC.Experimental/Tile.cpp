#include "stdafx.h"
#include "Tile.h"
#include "Globe.h"

using namespace uc;

CTile::CTile(CExperimentalLevel * l, CGlobe * g, CTile * parent, CMaterial * nl, double r, int lod, int ptx, int pty, CList<CString> * report)
{
	Level = l;
	Globe = g;
	Report = report;

	Notloaded = nl;
	Lod = lod;
	Parent = parent;
	TX = Parent ? 2 * Parent->TX + ptx : 0;
	TY = Parent ? 2 * Parent->TY + pty : 0;
	PTX = ptx;
	PTY = pty;
	R = r;
	T = int(pow(2, Lod));

	Ltb = O + 2 * E - (atan(sinh(M_PI - (double(TY) / T) * 2 * M_PI)) + E); // [o .. o+2e]
	Lte = O + 2 * E - (atan(sinh(M_PI - (double(TY + 1) / T) * 2 * M_PI)) + E);
	Ltd = Lte - Ltb;

	Lgb = 2 * M_PI * TX / T;
	Lge = 2 * M_PI * (TX + 1) / T;
	Lgd = 2 * M_PI / T;

	Apexes[0] = Sphere(Ltb, Lgb);
	Apexes[1] = Sphere(Ltb, Lge);
	Apexes[2] = Sphere(Lte, Lgb);
	Apexes[3] = Sphere(Lte, Lge);

	//Normals[0] = (Apexes[2] - Apexes[0]).Cross(Apexes[1] - Apexes[0]);
	//Normals[1] = (Apexes[3] - Apexes[2]).Cross(Apexes[1] - Apexes[2]);

	auto name = CString::Format(L"%d-%d-%d", Lod, TX, TY);

	Visual = Level->Engine->CreateVisual(name, null, Notloaded, CMatrix::Identity);
	//Active = Level->Engine->CreateActive(name, null, CMatrix::Identity);

	Visual->Clipping = EClipping::Apply;

	auto p = Parent;
	while(p && !p->Material)
	{
		p = p->Parent;
	}

	if(p)
	{
		Visual->SetMaterial(p->Visual->Material);
	}

	N = max(1, S / T); // segments per tile 
	auto m = N + 1; // points per tile

	Points.resize(m * m);
	UV.resize(m * m);

	for(int lt = 0; lt <= N; lt++) // spherical rectangle segment
	{
		for(int lg = 0; lg <= N; lg++)
		{
			auto lt_ = O + 2 * E - (atan(sinh(M_PI - (float(TY + float(lt) / N) / T) * 2 * M_PI)) + E); // [o .. o+2e]
			auto lg_ = Lgb + Lgd * float(lg) / N;

			Points[lt * m + lg] = Sphere(lt_, lg_);

			///points[lt * m + lg] = Sphere(float(lt)/N, float(lg)/N);

			if(Parent) // set map UV to nearest parent with loaded map ultil own is loaded
			{
				auto i = Parent;
				auto x = PTX * 0.5 + float(lg) / N * 0.5;
				auto y = PTY * 0.5 + float(lt) / N * 0.5;

				while(i != p)
				{
					x = i->PTX * 0.5 + x * 0.5;
					y = i->PTY * 0.5 + y * 0.5;

					i = i->Parent;
				}

				UV[lt * m + lg].x = float(x);
				UV[lt * m + lg].y = float(y);

				//uv[lt * m + lg].x = float(PTX * 0.5 + float(lg)/N * 0.5);
				//uv[lt * m + lg].y = float(PTY * 0.5 + float(lt)/N * 0.5);
			}
		}
	}

	for(int lt = 0; lt < N; lt++)
	{
		for(int lg = 0; lg < N; lg++)
		{
			Ix.push_back(lt * m + lg);
			Ix.push_back(lt * m + lg + 1);
			Ix.push_back((lt + 1) * m + lg);

			Ix.push_back((lt + 1) * m + lg);
			Ix.push_back(lt * m + lg + 1);
			Ix.push_back((lt + 1) * m + lg + 1);
		}
	}

}

CTile::~CTile()
{
	Visual->Free();
	//Active->Free();

	if(Mesh)
		Mesh->Free();

	for(auto i : Tiles)
	{
		delete i;
	}
}

CFloat3 CTile::Sphere(double lt, double lg)
{
	CFloat3 p;
	p.x = float(R * sin(lt) * cos(lg));
	p.z = float(R * sin(lt) * sin(lg));
	p.y = float(R * cos(lt));
	return p;
}

int CTile::Build(CCamera * c, int lod, CList<CMatrix> & wms)
{
	int count = 0;
	bool visible = false;

	if(!Parent || Lod <= 2)
	{
		visible = true;
	}
	else
	{
		bool infrustum = false;
		
		for(auto & fm : wms)
		{
			for(int i = 0; i < Ix.Count(); i += 3)
			{
				if(c->Contains(Points[Ix[i + 0]].VertexTransform(fm), Points[Ix[i + 1]].VertexTransform(fm), Points[Ix[i + 2]].VertexTransform(fm), true))
				{
					infrustum = true;
					break;
				}
			}

			if(infrustum)
				break;
		}

		if(infrustum)
		{
			visible = true;
		}

#ifdef _DEBUG
//		Report->push_back(CString::Format(L"%s   %s%d  %d-%d  %s %s", lod == Lod && visible ? L" V" : L"  ", CString(Lod, L' '), Lod, TX, TY, infront ? L"infront" : L" ", infrustum ? L"infrustum" : L" "));
		//Active->SetName(CString::Format(L"%d-%d-%d %s %s", LOD, TX, TY, infront ? L"infront" : L" ", infrustum ? L"infrustum" : L" "));
#endif
	}

	if(visible)
	{
		if(Parent && !Visual->Parent)
		{
			Parent->Visual->AddNode(Visual);
			//Parent->Active->AddNode(Active);
		}

		if(lod == Lod)
		{
			if(!Mesh) // null means not constructed
			{
				Mesh = Level->Engine->CreateMesh();
				Mesh->SetPrimitiveInfo(EPrimitiveType::TriangleList);
				Mesh->SetFillMode(EPrimitiveFill::Solid);
				
				Mesh->SetVertices(UOS_MESH_ELEMENT_POSITION, Points);
				Mesh->SetVertices(UOS_MESH_ELEMENT_UV, UV);
				Mesh->SetIndexes(Ix);
			}

			if(Status == ETileStatus::Empty)
			{
				count++;

				Status = ETileStatus::Scheduled;
				ShowTime = Level->Core->Timer.GetTime();

				Level->Core->DoUrgent(	this,
										L"Tile loading delay",
										[this]
										{
											if(Status == ETileStatus::Canceled) // canceled
											{
												Status = ETileStatus::Empty;
												return true;
											}
											else if(Level->Core->Timer.GetTime() - ShowTime > 0.5f)
											{
												RequestTile();
												return true;
											}
											else
												return false;
										});
			}

			Visual->SetMesh(Mesh);

			for(auto i : Tiles)
			{
				if(i && i->Visual->Parent)
				{
					i->CancelRequest();

					Visual->RemoveNode(i->Visual);
				}
			}
		}
		else if(lod > Lod)
		{
			Visual->SetMesh(null);

			if(Status == ETileStatus::Scheduled)
			{
				Status = ETileStatus::Canceled;
			}

			if(!Tiles[0])
			{
				Tiles[0] = new CTile(Level, Globe, this, Notloaded, R, Lod + 1, 0, 0, Report);
				Tiles[1] = new CTile(Level, Globe, this, Notloaded, R, Lod + 1, 1, 0, Report);
				Tiles[2] = new CTile(Level, Globe, this, Notloaded, R, Lod + 1, 0, 1, Report);
				Tiles[3] = new CTile(Level, Globe, this, Notloaded, R, Lod + 1, 1, 1, Report);
			}

			for(auto i : Tiles)
			{
				count += i->Build(c, lod, wms);
			}
		}
	}
	else
	{
		CancelRequest();

		if(Parent && Visual->Parent)
		{
			Visual->Parent->RemoveNode(Visual);
			//Active->Parent->RemoveNode(Active);
		}
	}

	return count;
}

void CTile::CancelRequest()
{
	if(Status == ETileStatus::Scheduled)
	{
		Status = ETileStatus::Canceled;
	}

	for(auto i : Tiles)
	{
		if(i)
			i->CancelRequest();
	}
}

void CTile::SetOwnUV()
{
	auto m = N + 1; // points per tile

	CArray<CFloat2> uv;

	uv.resize(m * m);

	for(int lt = 0; lt <= N; lt++) // spherical rectangle segment
	{
		for(int lg = 0; lg <= N; lg++)
		{
			uv[lt * m + lg].x = float(lg) / N;
			uv[lt * m + lg].y = float(lt) / N;
		}
	}

	Mesh->SetVertices(UOS_MESH_ELEMENT_UV, uv);
}

void CTile::RequestTile()
{
	if(Status != ETileStatus::Scheduled)
	{
		throw CException(HERE, L"Something wrong");
	}
	
	Status = ETileStatus::Requested;

	Globe->Store->RequestTile(Lod, TX, TY, Globe->MapStyle, [this](auto m)
															{
																Status = ETileStatus::Loaded;
																Material = m;
																SetOwnUV();
																Visual->SetMaterial(m);
															});

/*
	if(Globe->Materials.Contains(ufl))
	{
		auto t = Globe->Materials(ufl);

		if(t->Material)
		{
			Status = ETileStatus::Loaded;
			Visual->SetMaterial(t->Material);
		}
		else
		{
			Status = ETileStatus::Requested;
		
			t->Loaded +=	[this, t]
							{  
								Status = ETileStatus::Loaded;
								SetUV();
								Visual->SetMaterial(t->Material);
							};
		}

	}
	else if(Level->Storage->Exists(ufl))
	{
		Status = ETileStatus::Requested;

		Globe->Materials[ufl] = this;

		auto s = Level->Storage->OpenAsyncReadStream(ufl);

		s->ReadAsync(	[this, s, ufl]
						{
							SetUV();
							CreateMaterial()->Load(s->Buffer.GetData(), s->Buffer.GetSize());

							Level->Storage->Close(s); // seldestruction!

							Status = ETileStatus::Loaded;

							Loaded();
						});
	}
	else
	{
		Status = ETileStatus::Requested;

		Globe->Materials[ufl] = this;

		Request = new CHttpRequest(Level, CString::Format(L"https://api.mapbox.com/styles/v1/mapbox/" + Globe->MapStyle + L"/tiles/%d/%d/%d?"
														 "access_token=pk.eyJ1IjoibWlnaHR5d2lsbCIsImEiOiJjanRoMDVodDcyMzlkNDNwOHBvZDlpeG93In0.ZpobQTwDvXWRHg3PwgzaVQ",
														 Lod, TX, TY));
		Request->Caching = true;
		Request->Recieved = [this, ufl]()
							{
								SetUV();

								auto t = CreateMaterial();
								t->Load(&Request->Stream);
	
								auto s = Level->Storage->OpenWriteStream(ufl);
								t->Save(s);
								Level->Storage->Close(s);
	
								Status = ETileStatus::Loaded;

								Loaded();
							};
		Request->Send();
	}*/
}
