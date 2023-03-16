#include "StdAfx.h"
#include "NativeDirectory.h"
#include "Int32.h"

using namespace uc;

void CNativeDirectory::Create(CString const & src)
{
	if(CreateDirectory((L"\\\\?\\" + src).data(), null) == FALSE)
	{
		auto  e = GetLastError();
		//Sleep(1);
		//n++;
		//
		//	break;
		//
		//if(n > 1000)

		if(e != 183) // already exists
		{
			throw CException(HERE, CString::Format(L"Directory %s", src));
		}
	}

}

void CNativeDirectory::Delete(CString const & src, bool premove) 
{
	WIN32_FIND_DATA ffd;

	DWORD e;
	int n = 0;
	
	auto s = src;

	if(premove)
	{
		auto rnd = s + CGuid::Generate64();

		if(MoveFile((L"\\\\?\\" + s).data(), (L"\\\\?\\" + rnd).data()) == FALSE)
		{
			//throw CException(HERE, L"Unable to pre move: %s" + s);
		}
		
		s = rnd;
	}

	HANDLE h = FindFirstFile((L"\\\\?\\" + s + L"\\*.*").c_str(), &ffd);
	BOOL r = (h != INVALID_HANDLE_VALUE);

	while(r)
	{
		if(wcscmp(ffd.cFileName, L".") == 0 || wcscmp(ffd.cFileName, L"..") == 0)
		{
		}
		else if(ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
		{	
			Delete(s + L"\\" + ffd.cFileName, false);
		}
		else
		{
			n = 0;

			while(DeleteFile((L"\\\\?\\" + s + L"\\" + ffd.cFileName).data()) == FALSE)
			{
				Sleep(1);
				e = GetLastError();
				n++;

				if(n > 1000)
				{
					throw CException(HERE, L"Unable to DeleteFile after 1000 attempts");
				}
			}
		}	

		r = FindNextFile(h, &ffd);
	}
	FindClose(h);

	while(RemoveDirectory((L"\\\\?\\" + s).data()) == FALSE)
	{
		Sleep(1);
		e = GetLastError();
		n++;

		if(e == 2 || e == 3) // already removed
			break;

		if(n > 1000)
		{
			throw CException(HERE, L"Unable to RemoveDirectory after 1000 attempts");
		}
	}
}

void CNativeDirectory::Copy(CString const & src, CString const & dst)
{
	WIN32_FIND_DATA ffd;

	auto s = L"\\\\?\\" + src;
	auto d = L"\\\\?\\" + dst;

	int n = 0;

	CNativeDirectory::Create(dst);
	
	HANDLE h = FindFirstFile(CNativePath::Join(s, L"*.*").c_str(), &ffd);
	BOOL r = (h != INVALID_HANDLE_VALUE);

	while(r)
	{
		if(wcscmp(ffd.cFileName, L".") == 0 || wcscmp(ffd.cFileName, L"..") == 0);
		else if(ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
		{	
			Copy(src + L"\\" + ffd.cFileName, dst + L"\\" + ffd.cFileName);
		}
		else
		{
			//n = 0;

			if(CopyFile((s + L"\\" + ffd.cFileName).data(), (d + L"\\" + ffd.cFileName).data(), FALSE) == FALSE)
			{
				//Sleep(1);
				//e = GetLastError();
				//n++;
				//
				//if(n > 1000)
				//{
					throw CException(HERE, CString::Format(L"Copy failed from %s to %s", s + L"\\" + ffd.cFileName, d + L"\\" + ffd.cFileName));
				//}
			}
		}	

		r = FindNextFile(h, &ffd);
	}
	FindClose(h);
}

CList<CFileSystemEntry> CNativeDirectory::Enumerate(CString const & dir, const CString & regex, EDirectoryFlag f)
{
	CList<CFileSystemEntry> files;

	std::wregex rx(regex);

	if(dir == L"\\")
	{
		PWSTR name;

		IShellItem * m_currentBrowseLocationItem;
		auto hr = ::SHGetKnownFolderItem(FOLDERID_ComputerFolder, static_cast<KNOWN_FOLDER_FLAG>(0), nullptr, IID_PPV_ARGS(&m_currentBrowseLocationItem));

		IShellFolder * searchFolder;
		hr = m_currentBrowseLocationItem->BindToHandler(nullptr, BHID_SFObject, IID_PPV_ARGS(&searchFolder));

		IEnumIDList * fileList;

		if(S_OK == searchFolder->EnumObjects(nullptr, SHCONTF_FOLDERS|SHCONTF_FASTITEMS, &fileList))
		{
			ITEMID_CHILD * idList = nullptr;
			unsigned long fetched;

			while(S_OK == fileList->Next(1, &idList, &fetched))
			{

				IShellItem2 * shellItem;
				hr = SHCreateItemWithParent(nullptr, searchFolder, idList, IID_PPV_ARGS(&shellItem));

				if(SUCCEEDED(hr))
				{
					hr = shellItem->GetDisplayName(SIGDN_FILESYSPATH, &name);

					if(name)
					{
						if(std::regex_match(CString(name), rx))
						{
							files.push_back(CFileSystemEntry(name, CFileSystemEntry::Directory));
						}

						CoTaskMemFree(name);
					}
				}

				ILFree(idList);
			}
		}
	} 
	else
	{
		WIN32_FIND_DATA ffd;

		auto h = FindFirstFile(CNativePath::Join(L"\\\\?\\" + dir, L"*").c_str(), &ffd);
		auto r = (h != INVALID_HANDLE_VALUE);

		auto ini = CNativePath::Join(L"\\\\?\\" + dir, L"desktop.ini");
		auto hasini = CNativePath::IsFile(ini);

		while(r)
		{
			if(wcscmp(ffd.cFileName, L".") == 0 || wcscmp(ffd.cFileName, L"..") == 0);
			else if((f & SkipHidden)		&& ((ffd.dwFileAttributes & FILE_ATTRIBUTE_HIDDEN) != 0));
			else if((f & FilesOnly)			&& ((ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0));
			else if((f & DirectoriesOnly)	&& ((ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) == 0));
			else if(std::regex_match(CString(ffd.cFileName), rx))
			{
				CFileSystemEntry e(ffd.cFileName, ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY ? CFileSystemEntry::Directory : CFileSystemEntry::File);

				if(hasini && e.Name.EndsWith(L".lnk"))
				{
					wchar_t b[1024];
					GetPrivateProfileString(L"LocalizedFileNames", e.Name.data(), null, b, _countof(b), ini.data());

					if(wcslen(b) > 0)
					{
						auto a = CString(b).Split(L",");

						ExpandEnvironmentStrings(a[0].data()+1, b, _countof(b));

						auto h = LoadLibraryEx(b, null, LOAD_LIBRARY_AS_IMAGE_RESOURCE);

						if(!h)
						{
							h = LoadLibraryEx(CString(b).Replace(L"Program Files (x86)", L"Program Files").data(), null, LOAD_LIBRARY_AS_IMAGE_RESOURCE); // stupid workaround
						}

						if(h)
						{
							int n = LoadString(h, abs(CInt32::Parse(a[1])), b, _countof(b));

							if(n > 0)
							{
								e.NameOverride = b;
							}

							FreeLibrary(h);
						}
					}
				}

				files.push_back(e);
			}

			r = FindNextFile(h, &ffd);
		}
		FindClose(h);
	}

	return files;
}
		
bool CNativeDirectory::Exists(CString const & l)
{
	DWORD dwAttrib = GetFileAttributes((L"\\\\?\\" + l).data());

	return l == L"\\" || (dwAttrib != INVALID_FILE_ATTRIBUTES) && (dwAttrib & FILE_ATTRIBUTE_DIRECTORY);	
}

void CNativeDirectory::CreateAll(const CString & path)
{
	auto parts = path.Split(L"\\");

	CString s;

	for(auto & i : parts)
	{
		s += i + L"\\";

		if(CNativePath::IsRoot(s) || CNativePath::IsUNCServer(s) || CNativePath::IsUNCServerShare(s))
		{
			continue;
		}

		if(!Exists(s))
		{
			Create(s);
		}
	}
}

void CNativeDirectory::Clear(CString const & src)
{
	throw CException(HERE, L"Not tested");

	WIN32_FIND_DATA ffd;

	HANDLE h = FindFirstFile(CNativePath::Join(L"\\\\?\\" + src, L"\\*").c_str(), &ffd);
	BOOL r = (h != INVALID_HANDLE_VALUE);

	while(r)
	{
		if(wcscmp(ffd.cFileName, L".") == 0 || wcscmp(ffd.cFileName, L"..") == 0)
			;
		else if(ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
		{	
			Delete(src + L"\\" + ffd.cFileName);
		}
		else
		{
			DeleteFile((src + L"\\" + ffd.cFileName).data());
		}	

		r = FindNextFile(h, &ffd);
	}

	FindClose(h);
}

