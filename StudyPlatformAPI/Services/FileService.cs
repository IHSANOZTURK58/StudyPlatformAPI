using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StudyPlatformAPI.Services;

public class FileService : IFileService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;

    public FileService(IMinioClient minioClient, IConfiguration configuration)
    {
        _minioClient = minioClient;
        _bucketName = configuration["Minio:BucketName"]!;
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Geçersiz veya boş dosya.");

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";

        using var stream = file.OpenReadStream();

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(fileName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(file.ContentType);

        await _minioClient.PutObjectAsync(putObjectArgs);

        return fileName;
    }

    public async Task<string> GetFileUrlAsync(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return string.Empty;

        var args = new PresignedGetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(fileName)
            .WithExpiry(7200);

        var url = await _minioClient.PresignedGetObjectAsync(args);
        return url;
    }
}