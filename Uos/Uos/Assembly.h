#ifndef VERSIONNO__H
#define VERSIONNO__H

#define VERSION_FULL           0.1.1024.0

#define VERSION_BASEYEAR       0
#define VERSION_DATE           "2023-02-01"
#define VERSION_TIME           "12:12:02"

#define VERSION_MAJOR          0
#define VERSION_MINOR          1
#define VERSION_BUILDNO        1024
#define VERSION_EXTEND         0

#define VERSION_FILE           0,1,1024,0
#define VERSION_PRODUCT        0,1,1024,0
#define VERSION_FILESTR        "0,1,1024,0"
#define VERSION_PRODUCTSTR     "0,1,1024,0"

namespace uc
{
	constexpr auto UOS_PRODUCT_NAME_INTERNAL	= L"Uos";
	constexpr auto UOS_PRODUCT_NAME_PUBLIC		= L"Uos";
	constexpr auto UOS_PRODUCT_DESCRIPTION		= L"UOS is the Ultranet client and a prototype of future desktop, mobile and VR adaptable user interface environment";
		
	constexpr auto SUPERVISOR_FOLDER	=	L"Supervisor";
	constexpr auto CORE_FOLDER			=	L".";
	
	constexpr auto WEB_PAGE_HOMEPAGE	=	L"http://www.ultranet.org";
	constexpr auto WEB_PAGE_CONTACT		=	L"http://www.ultranet.org/contacts";
}

#endif
