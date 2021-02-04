using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;

namespace UploadAzure.Server.Controllers
{
	[Route("api/upload")]
	[ApiController]
	public class UploadController : ControllerBase
	{
		private readonly string _azureConnectionString;

		public UploadController(IConfiguration configuration)
		{
			_azureConnectionString = configuration.GetConnectionString("AzureConnectionString");
		}

		[HttpPost]
		public async Task<IActionResult> Upload()
		{
			try
			{
				var formCollection = await Request.ReadFormAsync();
				var file = formCollection.Files.First();

				if (file.Length > 0)
				{
					var container = new BlobContainerClient(_azureConnectionString, "upload-container");
					var createResponse = await container.CreateIfNotExistsAsync();
					if (createResponse != null && createResponse.GetRawResponse().Status == 201)
						await container.SetAccessPolicyAsync(PublicAccessType.Blob);

					var blob = container.GetBlobClient(file.FileName);
					await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

					using (var fileStream = file.OpenReadStream())
					{
						await blob.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = file.ContentType });
					}

					return Ok(blob.Uri.ToString());
				}

				return BadRequest();
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex}");
			}
		}
	}
}
