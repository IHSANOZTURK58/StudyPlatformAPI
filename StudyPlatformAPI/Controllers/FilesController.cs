using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudyPlatformAPI.Services;
using System;
using System.Threading.Tasks;

namespace StudyPlatformAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                // Dosya hiç seçilmediyse veya boşsa istek yapan kişiye 400 Bad Request dönüyoruz.
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Lütfen geçerli bir dosya seçin.");
                }

                // İşi mutfağa (Service katmanına) devrediyoruz.
                var fileName = await _fileService.UploadFileAsync(file);

                // İşlem başarılıysa 200 OK ile MinIO'da oluşan yeni dosya adını geri döndürüyoruz.
                return Ok(new { FileName = fileName, Message = "Dosya başarıyla yüklendi!" });
            }
            catch (Exception ex)
            {
                // Beklenmedik bir sunucu veya bağlantı hatası olursa 500 dönüp hatayı gösteriyoruz.
                return StatusCode(500, $"Dosya yüklenirken bir hata oluştu: {ex.Message}");
            }
        }


        [HttpGet("url/{fileName}")]
        public async Task<IActionResult> GetFileUrl(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest("Dosya adı boş olamaz.");
                }

                // İşi mutfağa devredip 1 saat geçerli, şifreli MinIO linkini alıyoruz
                var url = await _fileService.GetFileUrlAsync(fileName);

                if (string.IsNullOrEmpty(url))
                {
                    return NotFound("Dosya bulunamadı veya link oluşturulamadı.");
                }

                // Oluşturulan geçici linki (Pre-Signed URL) JSON olarak dönüyoruz
                return Ok(new { Url = url, Message = "Geçici bağlantı başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Bağlantı oluşturulurken bir hata oluştu: {ex.Message}");
            }
        }
    }
}