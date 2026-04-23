namespace PROGPOE.Services
{
    public class FileStorageService
    {
        private readonly string _uploadPath;
        public FileStorageService(IWebHostEnvironment env)
        {
            _uploadPath = Path.Combine(env.ContentRootPath, "Uploads", "Agreements");
            Directory.CreateDirectory(_uploadPath);
        }
        public async Task<string> SaveFileAsync(int contractId, IFormFile file)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("No file");
            if (!file.ContentType.Equals("application/pdf")) throw new ArgumentException("PDF only");
            if (file.Length > 10_485_760) throw new ArgumentException("Max 10MB");
            var fn = $"Contract_{contractId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var fp = Path.Combine(_uploadPath, fn);
            using var s = new FileStream(fp, FileMode.Create);
            await file.CopyToAsync(s);
            return $"/Uploads/Agreements/{fn}";
        }
        public FileStream? GetFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var fp = Path.Combine(Directory.GetCurrentDirectory(), path.TrimStart('/'));
            return File.Exists(fp) ? new FileStream(fp, FileMode.Open, FileAccess.Read) : null;
        }
        public bool FileExists(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return File.Exists(Path.Combine(Directory.GetCurrentDirectory(), path.TrimStart('/')));
        }
    }
}