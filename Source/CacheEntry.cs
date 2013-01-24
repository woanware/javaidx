using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using woanware;

namespace javaidx
{
    /// <summary>
    /// Encapsulates a Java IDX cache entry
    /// </summary>
    public class CacheEntry
    {
        #region Member Variables/Properties
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public bool Busy { get; set; }
        public bool Incomplete { get; set; }
        public int CacheVersion { get; set; }
        public bool IsShortcutImage { get; set; }
        public int ContentLength { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime ValidationTimestamp { get; set; }
        public bool KnownToBeSigned { get; set; }
        public int Section2Length { get; set;}
        public int Section3Length { get; set; }
        public int Section4Length { get; set; }
        public int Section5Length { get; set; }
        public DateTime BlackListValidationTime { get; set; }
        public DateTime CertExpirationDate { get; set; }
        public bool ClassVerficationStatus { get; set; }
        public int ReducedManifestLength { get; set; }
        public int Section4Pre15Length { get; set; }
        public bool HasOnlySignedEntries { get; set;}
        public bool HasSingleCodeSource { get; set; }
        public int Section4CertsLength { get; set; }
        public int Section4SignersLength { get; set; }
        public bool HasMissingSignedEntries { get; set; }
        public DateTime TrustedLibrariesValidationTime { get; set; }
        public int ReducedManifest2Length { get; set; }
        public bool IsProxied { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
        public string NamespaceId { get; set; }
        public string CodebaseIp { get; set; }
        public List<NameValue> Headers { get; set; }
        public bool Error { get; private set; }
        #endregion

        #region Constants
        private const int SECTION1_HEADER_LENGTH = 128;
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public CacheEntry(string path)
        {
            Headers = new List<NameValue>();

            Parse(path);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        private void Parse(string path)
        {
            try
            {
                FilePath = path;
                FileName = System.IO.Path.GetFileName(FilePath);

                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    this.Busy = StreamReaderHelper.ReadBoolBigEndian(fileStream);
                    this.Incomplete = StreamReaderHelper.ReadBoolBigEndian(fileStream);
                    this.CacheVersion = StreamReaderHelper.ReadInt32BigEndian(fileStream);

                    switch (this.CacheVersion)
                    {
                        case 602:
                            StreamReaderHelper.ReadByteBigEndian(fileStream);
                            StreamReaderHelper.ReadByteBigEndian(fileStream);
                            this.IsShortcutImage = StreamReaderHelper.ReadBoolBigEndian(fileStream);
                            this.ContentLength = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.LastModified = StreamReaderHelper.ReadJavaEpochBigEndian(fileStream);
                            this.ExpirationDate = StreamReaderHelper.ReadJavaEpochBigEndian(fileStream);
                            UInt16 length = StreamReaderHelper.ReadUInt16BigEndian(fileStream);
                            this.Version = StreamReaderHelper.ReadString(fileStream, (int)length);
                            length = StreamReaderHelper.ReadUInt16BigEndian(fileStream);
                            this.Url = StreamReaderHelper.ReadString(fileStream, (int)length);
                            length = StreamReaderHelper.ReadUInt16BigEndian(fileStream);
                            this.NamespaceId = StreamReaderHelper.ReadString(fileStream, (int)length);

                            ReadHeaders602(fileStream);
                            break;
                        case 603:
                        case 604:
                            StreamReaderHelper.ReadByteBigEndian(fileStream);
                            StreamReaderHelper.ReadByteBigEndian(fileStream);
                            this.IsShortcutImage = StreamReaderHelper.ReadBoolBigEndian(fileStream);
                            this.ContentLength = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.LastModified = StreamReaderHelper.ReadJavaEpochBigEndian(fileStream);
                            this.ExpirationDate = StreamReaderHelper.ReadJavaEpochBigEndian(fileStream);
                            this.ValidationTimestamp = StreamReaderHelper.ReadJavaEpochBigEndian(fileStream);
                            this.KnownToBeSigned = StreamReaderHelper.ReadBoolBigEndian(fileStream);
                            this.Section2Length = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.Section3Length = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.Section4Length = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.Section5Length = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.BlackListValidationTime = StreamReaderHelper.ReadJavaEpochBigEndian(fileStream);
                            this.CertExpirationDate = StreamReaderHelper.ReadJavaEpochBigEndian(fileStream);
                            this.ClassVerficationStatus = StreamReaderHelper.ReadBoolBigEndian(fileStream);
                            this.ReducedManifestLength = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.Section4Pre15Length = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.HasOnlySignedEntries = StreamReaderHelper.ReadBoolBigEndian(fileStream);
                            this.HasSingleCodeSource = StreamReaderHelper.ReadBoolBigEndian(fileStream);
                            this.Section4CertsLength = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.Section4SignersLength = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.HasMissingSignedEntries = StreamReaderHelper.ReadBoolBigEndian(fileStream);
                            this.TrustedLibrariesValidationTime = StreamReaderHelper.ReadJavaEpochBigEndian(fileStream);
                            this.ReducedManifest2Length = StreamReaderHelper.ReadInt32BigEndian(fileStream);

                            if (this.Section2Length > 0)
                            {
                                ReadSection2(fileStream);
                            }
                            break;
                        case 605:
                            this.IsShortcutImage = StreamReaderHelper.ReadBoolBigEndian(fileStream);
                            this.ContentLength = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.LastModified = StreamReaderHelper.ReadJavaEpochBigEndian(fileStream);
                            this.ExpirationDate = StreamReaderHelper.ReadJavaEpochBigEndian(fileStream);
                            this.ValidationTimestamp = StreamReaderHelper.ReadJavaEpochBigEndian(fileStream);
                            this.KnownToBeSigned = StreamReaderHelper.ReadBoolBigEndian(fileStream);
                            this.Section2Length = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.Section3Length = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.Section4Length = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.Section5Length = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.BlackListValidationTime = StreamReaderHelper.ReadJavaEpochBigEndian(fileStream);
                            this.CertExpirationDate = StreamReaderHelper.ReadJavaEpochBigEndian(fileStream);
                            this.ClassVerficationStatus = StreamReaderHelper.ReadBoolBigEndian(fileStream);
                            this.ReducedManifestLength = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.Section4Pre15Length = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.HasOnlySignedEntries = StreamReaderHelper.ReadBoolBigEndian(fileStream);
                            this.HasSingleCodeSource = StreamReaderHelper.ReadBoolBigEndian(fileStream);
                            this.Section4CertsLength = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.Section4SignersLength = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.HasMissingSignedEntries = StreamReaderHelper.ReadBoolBigEndian(fileStream);
                            this.TrustedLibrariesValidationTime = StreamReaderHelper.ReadJavaEpochBigEndian(fileStream);
                            this.ReducedManifest2Length = StreamReaderHelper.ReadInt32BigEndian(fileStream);
                            this.IsProxied = StreamReaderHelper.ReadBoolBigEndian(fileStream);

                            if (this.Section2Length > 0)
                            {
                                ReadSection2(fileStream);
                            }
                            break;
                        default:
                            Console.WriteLine("Unsupport cache version (" + this.CacheVersion + ") " + path);
                            return;
                    }
                }
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileStream"></param>
        private void ReadSection2(FileStream fileStream)
        {
            try
            {
                // Move to the start of the mandatory section 2 (128/0x80)
                fileStream.Seek(SECTION1_HEADER_LENGTH, SeekOrigin.Begin);
                UInt16 length = StreamReaderHelper.ReadUInt16BigEndian(fileStream);
                this.Version = StreamReaderHelper.ReadString(fileStream, (int)length);
                length = StreamReaderHelper.ReadUInt16BigEndian(fileStream);
                this.Url = StreamReaderHelper.ReadString(fileStream, (int)length);
                length = StreamReaderHelper.ReadUInt16BigEndian(fileStream);
                this.NamespaceId = StreamReaderHelper.ReadString(fileStream, (int)length);
                length = StreamReaderHelper.ReadUInt16BigEndian(fileStream);
                this.CodebaseIp = StreamReaderHelper.ReadString(fileStream, (int)length);

                ReadHeaders(fileStream);
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        /// <summary>
        /// Parses out the headers from the HTTP response
        /// </summary>
        /// <param name="fileStream"></param>
        private void ReadHeaders(FileStream fileStream)
        {
            try
            {
                for (int index = StreamReaderHelper.ReadInt32BigEndian(fileStream); index > 0; index--)
                {
                    UInt16 length = StreamReaderHelper.ReadUInt16BigEndian(fileStream);
                    string headerName = StreamReaderHelper.ReadString(fileStream, length);
                    if (headerName == "<null>")
                    {
                        headerName = string.Empty;
                    }
                    length = StreamReaderHelper.ReadUInt16BigEndian(fileStream);
                    string headerValue = StreamReaderHelper.ReadString(fileStream, length);

                    Headers.Add(new NameValue(headerName, headerValue));
                }
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        /// <summary>
        /// Parses out the headers from the HTTP response (602 cache version)
        /// </summary>
        /// <param name="fileStream"></param>
        private void ReadHeaders602(FileStream fileStream)
        {
            try
            {
                for (int index = StreamReaderHelper.ReadInt32BigEndian(fileStream); index > 0; index--)
                {
                    UInt16 length = StreamReaderHelper.ReadUInt16BigEndian(fileStream);
                    string headerName = StreamReaderHelper.ReadString(fileStream, length);
                    if (headerName == "deploy_resource_codebase_ip")
                    {
                        length = StreamReaderHelper.ReadUInt16BigEndian(fileStream);
                        string ip = StreamReaderHelper.ReadString(fileStream, length);
                        this.CodebaseIp = ip; 
                    }
                    else
                    {
                        length = StreamReaderHelper.ReadUInt16BigEndian(fileStream);
                        string headerValue = StreamReaderHelper.ReadString(fileStream, length);

                        Headers.Add(new NameValue(headerName, headerValue));
                    }
                }
            }
            catch (Exception)
            {
                Error = true;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        public string HeadersText
        {
            get
            {
                List<string> headers = new List<string>();
                foreach (NameValue nameValue in Headers)
                {
                    if (nameValue.Name.Trim().Length > 0)
                    {
                        headers.Add(string.Format(" {0}: {1}", nameValue.Name, nameValue.Value));
                    }
                    else
                    {
                        headers.Add(string.Format(" {0}", nameValue.Value));
                    }
                }

                return string.Join(",", headers.ToArray());
            }
        }
        #endregion
    }
}
