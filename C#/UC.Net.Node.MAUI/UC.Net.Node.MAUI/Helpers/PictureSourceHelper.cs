using CommunityToolkit.Diagnostics;

namespace UC.Net.Node.MAUI.Helpers;

public static class PictureSourceHelper
{
    private static FileResult photo;

    public static async Task<string> GetPicture()
    {
		var result = string.Empty;
        try 
        {
            var sheet = await App.Current.MainPage.DisplayActionSheet(
				"ChoosePictureSource", string.Empty, null, new string[] { "Camera","Gallery" });

            if (sheet != null)
			{
				photo = sheet == "Camera" && MediaPicker.IsCaptureSupported
					? await MediaPicker.CapturePhotoAsync()
					: await MediaPicker.PickPhotoAsync();

				if (photo != null)
				{
					var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
					using (var stream = await photo.OpenReadAsync())
					using (var newStream = File.OpenWrite(newFile))
						await stream.CopyToAsync(newStream);
					result = newFile;
				}
			}
        }
        catch
        {
            await ToastHelper.ShowMessageAsync("Capturing pictures isn't supported");
        }
        return result;
    }

    public static async Task<byte[]> PathToBytes(string path)
    {
        try
        {
            using (var stream = File.OpenWrite(path))
            using (var newStream = new MemoryStream())
            {
                await stream.CopyToAsync(newStream);
                return newStream.ToArray();
            }
        }
        catch(Exception ex)
        {
            await ToastHelper.ShowMessageAsync("Loading error");
			ThrowHelper.ThrowInvalidOperationException("PathToBytes: Loading error", ex);
        }
        return null;
    }
}
