// Mightywill.Tools.Image.h

#pragma once

using namespace System;

namespace UC { namespace Image { namespace DevIL 
{
	public ref class Image
	{
		unsigned char * pointer;

		public:
			static Image()
			{
				/* First we initialize the library. */
				/*Do not forget that... */
				ilInit();
				/* We want all images to be loaded in a consistent manner */
				ilEnable(IL_ORIGIN_SET);
			}

			~Image()
			{
				 this->!Image();
			}

			!Image()
			{
				free(pointer);
			}

			property System::IntPtr Pointer
			{
				 System::IntPtr get() { return (System::IntPtr)pointer; }
			}

			property System::Int32 Width
			{
				 System::Int32 get() { return (System::Int32)ilGetInteger(IL_IMAGE_WIDTH); }
			}

			property System::Int32 Height
			{
				 System::Int32 get() { return (System::Int32)ilGetInteger(IL_IMAGE_HEIGHT); }
			}

/*
			static bool Supports(array<System::Byte> ^ data)
			{
				ILuint handle;
				/ * In the next section, we load one image * /
				ilGenImages(1, & handle);
				ilBindImage(handle);
				
				pin_ptr<unsigned char> p = &data[0];

				return ilIsValidL(IL_DDS, p, data->Length);
			}*/

			Image(array<System::Byte> ^ data)
			{
				ILuint handle;
				/* In the next section, we load one image */
				ilGenImages(1, & handle);
				ilBindImage(handle);

				pin_ptr<unsigned char> p = &data[0];

				ilLoadL(IL_TYPE_UNKNOWN, p, data->Length);

				auto f = ilGetInteger(IL_IMAGE_FORMAT);
				ilConvertImage(IL_RGBA, IL_UNSIGNED_BYTE);
								
				p = ilGetData();

				auto w = ilGetInteger(IL_IMAGE_WIDTH);
				auto h = ilGetInteger(IL_IMAGE_HEIGHT);

				pointer = (unsigned char *)malloc(w * h * 4);
				
				// convert lower-upper RGBA to upper-lower to ARGB(Format32bppArgb)
				for(int y = 0; y < h; y++)
				{
					for(int x = 0; x < w; x++)
					{
						*(pointer + (x + y*w)*4 +0) = *(p + (x + (h - 1 - y)*w)*4 +2);
						*(pointer + (x + y*w)*4 +1) = *(p + (x + (h - 1 - y)*w)*4 +1);
						*(pointer + (x + y*w)*4 +2) = *(p + (x + (h - 1 - y)*w)*4 +0);
						*(pointer + (x + y*w)*4 +3) = *(p + (x + (h - 1 - y)*w)*4 +3);
					}
				}
				



/*
				auto n = ilSaveL(IL_TGA, NULL, 0);
				p = (ILubyte *)malloc(n);
				
				ilSaveL(IL_PNG, p, n);

				data = gcnew array<System::Byte>(n);

				for(unsigned int i=0; i<n; i++)
				{
					data[i] = p[i];
				}

				free(p);

				return data;*/
			}
	};
}}}