#pragma once

namespace uc
{
	#undef GetClassName

	#define null										NULL
	#define byte										unsigned char
	#define HERE										__FUNCTIONW__,__LINE__

	typedef unsigned int								uint;
	#define PROJECT_DEVELOPMENT_STAGE					L"Prototype"

	#define UOS_OBJECT_PROTOCOL							L"unap"

	#define UOS_PROJECT_TARGET_PLATFORM_WIN32_X86		L"Winx86"
	#define UOS_PROJECT_TARGET_PLATFORM_WIN32_X64		L"Winx64"

	#define UOS_PROJECT_CONFIGURATION_DEBUG				L"Debug"
	#define UOS_PROJECT_CONFIGURATION_RELEASE			L"Release"

	#define UC_NAME										L"Ultranet Community"
	#define UC_NAMESPACE								L"UC"
	#define UC_COPYRIGHT								L"© Ultranet Community"
	#define UO_WEB_HOME									L"https://ultranet.org"

	#define Bit(a, n)		(((a >> n) & 0x1)==1)
		
	#define cleandelete(p)	if(p != null)\
							{\
								if(*((unsigned int *)p) == 0xfeeefeee)\
								{\
									throw CException(HERE, L"p==0xfeeefeee"); \
								}\
								delete p;\
								p=null;\
							}

	#define UOS_RTTI		static CString & GetClassName()\
							{\
								static CString name;\
								if(name.empty())\
								{\
									auto p = (wchar_t *)(__FUNCTIONW__ + wcslen(__FUNCTIONW__));\
									auto n = 0;\
									wchar_t * e = null;\
									wchar_t * b = null;\
									while(n < 3)\
									{\
										if(*p == L':')\
										{\
											n++;\
											if(n == 2)\
												e = p;\
											if(n == 3)\
											{\
												b = p+1;\
												if(*b == 'C')\
													b++;\
												break;\
											}\
										}\
										p--;\
									}\
									name.assign(b, e-b);\
								}\
								return name;\
							}\
							\
							virtual CString & GetInstanceName() override\
							{\
								static CString b;\
								if(b.empty())\
								{\
									auto p = typeid(*this).name();\
									auto name = strstr(p, "C") + 1; if(!name) name = p;\
									auto n = (size_t)MultiByteToWideChar(CP_ACP, 0, name, -1, null, 0);\
									b.resize(n - 1); /* ending \0 not needed */\
									MultiByteToWideChar(CP_ACP, 0, name, -1, (LPWSTR)b.data(), (int)b.size());\
								}\
								return b;\
							}\



	#define EnumBitwiseOperators(T)	inline constexpr T operator&(T x, T y)											\
									{																				\
										return static_cast<T>(static_cast<int>(x) & static_cast<int>(y));			\
									}																				\
																													\
									inline constexpr T operator|(T x, T y)											\
									{																				\
										return static_cast<T>(static_cast<int>(x) | static_cast<int>(y));			\
									}																				\
																													\
									inline constexpr T operator^(T x, T y)											\
									{																				\
										return static_cast<T>(static_cast<int>(x) ^ static_cast<int>(y));			\
									}																				\
																													\
									inline constexpr T operator~(T x)												\
									{																				\
										return static_cast<T>(~static_cast<int>(x));								\
									}																				\
																													\
									inline T & operator&=(T & x, T y)												\
									{																				\
										x = x & y;																	\
										return x;																	\
									}																				\
																													\
									inline T & operator|=(T & x, T y)												\
									{																				\
										x = x | y;																	\
										return x;																	\
									}																				\
																													\
									inline T & operator^=(T & x, T y)												\
									{																				\
										x = x ^ y;																	\
										return x;																	\
									}																				

}
