using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace DiskSpaceLibrary
{
    public sealed class DiskSpace
    {

        internal static readonly StorageFolder[] APP_FOLDERS = {
            ApplicationData.Current.LocalFolder,
            ApplicationData.Current.RoamingFolder,
            ApplicationData.Current.TemporaryFolder
        };

        [DataContract]
        internal class Result
        {
            [DataMember]
            internal ulong app = 0;
            [DataMember]
            internal ulong total = 0;
            [DataMember]
            internal ulong free = 0;
        }

        // Return the system FreeSpace and Capacity properties
        private async static Task<IDictionary<string, object>> getExtraProperties()
        {
            var basicProperties = await Windows.Storage.ApplicationData.Current.LocalFolder.GetBasicPropertiesAsync();
            return await basicProperties.RetrievePropertiesAsync(new string[] { "System.FreeSpace", "System.Capacity" });
        }

        // Class Entry point 
        public static IAsyncOperation<string> info(string args)
        {
            // Entry point in WinRT can not return Task<T> - so here is a trick to convert IAsyncOperation (WinRT) into classic C# async
            return infoTask().AsAsyncOperation();
        }

        // The real disk space work is done here within some asynchronous stuff
        private async static Task<string> infoTask()
        {
            Result result = new Result();

            // Run folder discovery into another Thread to not block UI Thread
            await Task.Run(async () =>
            {

                foreach (var folder in APP_FOLDERS)
                {
                    var query = folder.CreateFileQuery();
                    var files = await query.GetFilesAsync();
                    ulong folderSize = 0;
                    foreach (Windows.Storage.StorageFile file in files)
                    {
                        // Get file's basic properties.
                        Windows.Storage.FileProperties.BasicProperties basicProperties =
                            await file.GetBasicPropertiesAsync();

                        folderSize += (ulong)basicProperties.Size;
                    }

                    result.app += folderSize;
                }
            });

            await getExtraProperties().ContinueWith(propertiesTask =>
            {
                result.free = (ulong)propertiesTask.Result["System.FreeSpace"];
                result.total = (ulong)propertiesTask.Result["System.Capacity"];
            });

            // Return JSON Result
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Result));
            MemoryStream outputMs = new MemoryStream();
            serializer.WriteObject(outputMs, result);

            outputMs.Position = 0;
            StreamReader sr = new StreamReader(outputMs);

            return sr.ReadToEnd();

        }
    }
}
