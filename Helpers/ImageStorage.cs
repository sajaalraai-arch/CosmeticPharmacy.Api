namespace CosmeticPharmacy.Api.Helpers
{

    public static class ImageStorage
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxBytes = 5 * 1024 * 1024;

        public static string? Validate(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return "Only image files (jpg, jpeg, png, webp, gif) are allowed.";
            if (file.Length > MaxBytes)
                return "Image size must not exceed 5 MB.";
            return null;
        }

        public static async Task<string?> SaveAsync(IFormFile? file, IWebHostEnvironment env)
        {
            if (file == null || file.Length == 0) return null;

            string webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
            string uploadDir = Path.Combine(webRoot, "images");
            Directory.CreateDirectory(uploadDir);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName).ToLowerInvariant();
            string fullPath = Path.Combine(uploadDir, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/images/" + fileName;
        }
    }
}
