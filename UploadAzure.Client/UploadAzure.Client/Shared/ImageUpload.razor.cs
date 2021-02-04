using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace UploadAzure.Client.Shared
{
	public partial class ImageUpload
	{
		[Inject]
		public HttpClient HttpClient { get; set; }

		public string ImgUrl { get; set; }

		private async Task HandleSelected(InputFileChangeEventArgs e)
		{
			var imageFile = e.File;

			if (imageFile == null)
				return;

			var resizedFile = await imageFile.RequestImageFileAsync("image/png", 300, 500);

			using (var ms = resizedFile.OpenReadStream(resizedFile.Size))
			{
				var content = new MultipartFormDataContent();
				content.Headers.ContentDisposition =
					new ContentDispositionHeaderValue("form-data");
				content.Add(new StreamContent(ms, Convert.ToInt32(resizedFile.Size)),
					"image", imageFile.Name);

				var response = await HttpClient.PostAsync("upload", content);
				ImgUrl = await response.Content.ReadAsStringAsync();
			}
		}
	}
}
