using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Interfaces;
using Microsoft.Extensions.Options;
using API.Helpers;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace API.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;
        public PhotoService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(
                config.Value.Cloudname,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file) {
            var uploadResult = new ImageUploadResult();

            if (file.Length > 0) {
                // we want to dispose of this stream as soon as we are done with this method
                using var stream = file.OpenReadStream();  // get our file as a stream of data
                // crop the photo and center on the face
                var uploadParams = new ImageUploadParams {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face")
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams); // this does the actual upload to cloudinary
            }

            return uploadResult;
        }

        public async Task<DeletionResult> DeletePhotoAsync(string publicId) {
          var deleteParams = new DeletionParams(publicId);

          var result = await _cloudinary.DestroyAsync(deleteParams);

          return result;
        }
    }
}