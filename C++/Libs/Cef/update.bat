echo off

set cef=..\..\..\..\cef\chromium_git\chromium\src\cef\binary_distrib\cef_binary_3.3578.1870.gc974488_windows32
set debug=..\..\..\Bin-Uos-Win32.x86.Debug\una.ultranet.org_Experimental\Browser
set release=..\..\..\Bin-Uos-Win32.x86.Release\una.ultranet.org_Experimental\Browser

rem lib
rd include\
rd Resources\
rd win32.x86.Debug\
rd win32.x86.Release\

robocopy %cef%\Include\ Include\ /s
robocopy %cef%\Resources\ Resources\ /s
robocopy %cef%\Debug\ win32.x86.Debug\ /s
robocopy %cef%\Release\ win32.x86.Release\ /s
copy %cef%\libcef_dll_wrapper\Debug\libcef_dll_wrapper.lib win32.x86.Debug\
copy %cef%\libcef_dll_wrapper\Debug\libcef_dll_wrapper.pdb win32.x86.Debug\
copy %cef%\libcef_dll_wrapper\Release\libcef_dll_wrapper.lib win32.x86.Release\

rem bin
rd %debug%\locales /s /q
rd %debug%\swiftshader /s /q
del /q %debug%\*.pak %debug%\..\*.dat %debug%\..\*.bin

rd %release%\locales /s /q
rd %release%\swiftshader /s /q
del /q %release%\*.pak %release%\..\*.dat %release%\..\*.bin

robocopy %cef%\Resources\	%debug% /s 
robocopy %cef%\Debug\ 		%debug%/.. /s /xf *.lib
move %debug%\icudtl.dat %debug%\..

robocopy %cef%\Resources\	%release% /s
robocopy %cef%\Release\		%release%/.. /s /xf *.lib
move %release%\icudtl.dat %release%\..

