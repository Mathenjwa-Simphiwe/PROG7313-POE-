using Xunit;
using Moq;
using Microsoft.AspNetCore.Hosting;
using PROGPOE.Services;

namespace PROGPOE.Tests
{
    public class FileValidationTests
    {
        private readonly FileStorageService _service;

        public FileValidationTests()
        {
            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(e => e.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            _service = new FileStorageService(mockEnv.Object);
        }

        [Fact]
        public void FileExists_NullPath_ReturnsFalse()
        {
            var result = _service.FileExists(null!);
            Assert.False(result);
        }

        [Fact]
        public void FileExists_EmptyPath_ReturnsFalse()
        {
            var result = _service.FileExists("");
            Assert.False(result);
        }

        [Fact]
        public void GetFile_InvalidPath_ReturnsNull()
        {
            var result = _service.GetFile("/nonexistent/file.pdf");
            Assert.Null(result);
        }
    }
}