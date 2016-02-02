var exec = require('cordova/exec');
function DiskSpace() {
	console.log("DiskSpacePlugin is created");
}

DiskSpace.prototype.info = function (options, success, error) {
    var execPromise = exec(function (result) {
        if (success) {
            success(result);
        }
    }, function (result) {
        if (error) {
            error(result);
        }
    }, "DiskSpacePlugin", "info", [options]);
};
var diskSpace = new DiskSpace();
module.exports = diskSpace;