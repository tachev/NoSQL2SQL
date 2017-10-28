using System;

namespace Geo.Data
{
    public interface IDocument
    {
        string Id { get; set; }//TODO:Remove set

        string DocumentType { get; }
    }

    public static class IDocumentExtensions {

        private static DateTime startOfTime = new DateTime(1970, 1, 1);

        public static long GenerateTimeStamp(this IDocument document)
        {
            return (long)(DateTime.UtcNow.Subtract(startOfTime)).TotalSeconds;
        }
    }
}
