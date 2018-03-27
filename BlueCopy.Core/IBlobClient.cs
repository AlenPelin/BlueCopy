using System.Threading.Tasks;

namespace BlueCopy.Core
{
    public interface IBlobClient
    {
        Task UploadAsync(string id1, string id2, string content);
    }
}
