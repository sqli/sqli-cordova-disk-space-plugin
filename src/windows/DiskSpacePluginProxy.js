var cordova = require('cordova');
var DiskSpacePlugin = require('./DiskSpacePlugin');


module.exports = {
	
	info:function(successCallback, errorCallback) {
        DiskSpacePluginProxy.DiskSpacePlugin.info('').then(function (success) {
			if (success && typeof success === 'string') {
				success = JSON.parse(success);
			}
			successCallback(success);
		}, function (error) {
			errorCallback(error);
		});
	}
}

require("cordova/exec/proxy").add("DiskSpacePlugin", module.exports);