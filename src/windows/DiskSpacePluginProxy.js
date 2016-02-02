module.exports = {
    info: function (successCallback, errorCallback) {
        var result = {
            app: 0,
            total: 0,
            free: 0
        }

        var sizeFolder = function (folder) {
            return new WinJS.Promise(function (completeDispatch, errorDispatch, progressDispatch) {
                var promises = [];
                var storageFileQueryResult = folder.createFileQuery(Windows.Storage.Search.CommonFileQuery.orderByName);
                storageFileQueryResult.getFilesAsync().then(function (res) {
                    for (var i = 0; i < res.length; i++) {
                        promises.push(res[i].getBasicPropertiesAsync());
                    }
                    WinJS.Promise.join(promises).then(function (res) {
                        var app = 0;
                        for (var i = 0; i < res.length; i++) {
                            app += res[i].size;
                        }
                        completeDispatch(app);
                    }, function (error) {
                        errorDispatch(error);
                    });
                });
            });
        }

        var promises = [];
        promises.push(sizeFolder(Windows.Storage.ApplicationData.current.localFolder));
        promises.push(sizeFolder(Windows.Storage.ApplicationData.current.roamingFolder));
        promises.push(sizeFolder(Windows.Storage.ApplicationData.current.temporaryFolder));

        WinJS.Promise.join(promises).then(function (res) {
            for (var i = 0; i < res.length; i++) {
                result.app += res[i];
            }
            
            Windows.Storage.ApplicationData.current.localFolder.getBasicPropertiesAsync().then(function (basicProperties) {
                // Get extra properties
                return basicProperties.retrievePropertiesAsync(["System.FreeSpace", "System.Capacity"]);
            }).done(function (extraProperties) {
                result.free = extraProperties["System.FreeSpace"];
                result.total = extraProperties["System.Capacity"];
                successCallback(result);
            }, function (error) {
                errorCallback(error);
            });
        }, function (error) {
            errorCallback(error);
        });




        
        
        
    }
};

require("cordova/exec/proxy").add("DiskSpacePlugin", module.exports);