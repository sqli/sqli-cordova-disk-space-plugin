using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace DiskSpaceLibrary
{
    public sealed class DiskSpace
    {
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

        // Manually compute total size of the given StorageFolder
        private async static Task<ulong> sizeFolder(StorageFolder folder)
        {
            ulong folderSize = 0;
            try
            {
                var queryOption = new Windows.Storage.Search.QueryOptions();
                queryOption.FolderDepth = Windows.Storage.Search.FolderDepth.Deep;
                var storageFileQueryResult = folder.CreateFileQueryWithOptions(queryOption);

                IReadOnlyCollection<StorageFile> files = await storageFileQueryResult.GetFilesAsync();

                List<Task<BasicProperties>> tasks = new List<Task<BasicProperties>>();
                foreach (StorageFile file in files)
                {
                    tasks.Add(file.GetBasicPropertiesAsync().AsTask());
                }

                Task.WaitAll(tasks.ToArray());

                // Once all file properties have been found, compute size
                foreach (Task<BasicProperties> task in tasks)
                {
                    folderSize += task.Result.Size;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return 0;
            }
            return folderSize;
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

            var tasks = new Task<ulong>[3];
            // Run folder discovery into another Thread to not block UI Thread
            await Task.Run(async () =>
            {
                tasks[0] = sizeFolder(ApplicationData.Current.LocalFolder);
                tasks[1] = sizeFolder(ApplicationData.Current.RoamingFolder);
                tasks[2] = sizeFolder(ApplicationData.Current.TemporaryFolder);

                await Task.WhenAll(tasks);
                foreach (var sizeTask in tasks)
                {
                    result.app += sizeTask.Result;
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
