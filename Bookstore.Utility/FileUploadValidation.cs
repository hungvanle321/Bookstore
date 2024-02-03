using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Bookstore.Utility
{
    public class FileImageUploadValidation
    {
        private static readonly Dictionary<string, List<byte[]>> _fileSignature = new()
        {
            { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF}
                }
            },
			{ ".jpg", new List<byte[]>
		        {
                    new byte[] { 0xFF, 0xD8, 0xFF}
                }
	        },
			{ ".png", new List<byte[]>
                {
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
                }
            },
            { ".bmp", new List<byte[]>
                {
                    new byte[] { 0x42, 0x4D }
                }
            },
            { ".tiff", new List<byte[]>
                {
                    new byte[] { 0x49, 0x49, 0x2A, 0x00 },
                    new byte[] { 0x4D, 0x4D, 0x00, 0x2A }
                }
            },
            { ".webp", new List<byte[]>
                {
                    new byte[] { 0x52, 0x49, 0x46, 0x46 },
                    new byte[] { 0x57, 0x45, 0x42, 0x50 }
                }
            },
            { ".svg", new List<byte[]>
                {
                    new byte[] { 0x3C, 0x73, 0x76, 0x67 }
                }
            }
        };

		public static bool IsFileExtensionValid(IFormFile file, out string ErrorMessage)
		{
			ErrorMessage = string.Empty;
			var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
			if (!string.IsNullOrEmpty(ext) && _fileSignature.ContainsKey(ext))
			{
				return true;
			}
			else
			{
				ErrorMessage = "Invalid file extension. Unable to determine file type.";
				return false;
			}
		}
		public static bool IsFileSignatureValid(IFormFile file, out string ErrorMessage)
		{
			ErrorMessage = string.Empty;
			var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

			if (_fileSignature.TryGetValue(ext, out var signatures))
			{
				using (var reader = new BinaryReader(file.OpenReadStream()))
				{
					var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

					if (signatures.Any(signature =>
						headerBytes.Take(signature.Length).SequenceEqual(signature)))
					{
						return true;
					}
					else
					{
						ErrorMessage = "File signature is not valid. It may not be an allowed file type.";
						return false;
					}
				}
			}
			else
			{
				ErrorMessage = "Invalid file extension. Unable to determine file type.";
				return false;
			}
		}

		public static bool IsFileSizeExceedLimit(IFormFile formFile, long FileLimit, out string ErrorMessage)
        {
            ErrorMessage = string.Empty;

            if (formFile.Length > FileLimit)
            {
                ErrorMessage = $"File size exceeds the allowed limit of {FileLimit} bytes.";
                return true;
            }

            return false;
        }

    }
}
