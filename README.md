# Disk Space Plugin
This plugin allows to get information about disk space in Android / iOS / Windows 10 platforms.

## Installation
To use this plugin in a project, you have to do:

	cordova plugin add git@gitlab-toulouse.sqli.com:ah-techpub/sqli-cordova-disk-space-plugin.git

Add your platforms targeted:

	cordova platform add android
	cordova platform add ios
	cordova platform add windows

## Usage

1- You can get disk information by executing the info function:

	DiskSpacePlugin.info(options, successCallback, errorCallback);

Where options is a javascript object:

	location: 1 // To get information about external storage (For android only)
	location: 2 // To get information about internal storage (For android only)

The result object is like that:

	{
		"app": 29887, // Space occupied by the current application
		"total": 98717873000, // Total space of the device
		"free": 76556280  // Free space of the device
	}
