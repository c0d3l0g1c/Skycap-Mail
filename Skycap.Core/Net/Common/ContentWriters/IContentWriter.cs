namespace Skycap.Net.Common.ContentWriters
{
    using System;
    using System.Threading.Tasks;

    public interface IContentWriter
    {
        void Close();
        void Open();
        void Write(byte[] data);
    }
}

