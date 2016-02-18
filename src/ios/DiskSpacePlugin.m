#import "DiskSpacePlugin.h"
#import <Cordova/CDV.h>

@implementation DiskSpacePlugin

- (void)info:(CDVInvokedUrlCommand*)command
{
    CDVPluginResult* pluginResult = nil;
    
    long long total = [[[[NSFileManager defaultManager] attributesOfFileSystemForPath:NSHomeDirectory() error:nil] objectForKey:NSFileSystemSize] longLongValue];
    long long free = [[[[NSFileManager defaultManager] attributesOfFileSystemForPath:NSHomeDirectory() error:nil] objectForKey:NSFileSystemFreeSize] longLongValue];

    
    NSArray *paths = [[NSFileManager defaultManager] URLsForDirectory:NSDocumentDirectory inDomains:NSUserDomainMask];
    NSURL *documentsURL = [paths lastObject];
    NSUInteger sizeDocuments = [self getDirectoryFileSize:documentsURL];
    
    paths = [[NSFileManager defaultManager] URLsForDirectory:NSLibraryDirectory inDomains:NSUserDomainMask];
    documentsURL = [paths lastObject];
    NSUInteger sizeLibrairies = [self getDirectoryFileSize:documentsURL];
    
    NSMutableDictionary* result = [[NSMutableDictionary alloc] init];
    [result setObject:[NSNumber numberWithDouble:total] forKey:@"total"];
    [result setObject:[NSNumber numberWithInt:(sizeDocuments+sizeLibrairies)] forKey:@"app"];
    [result setObject:[NSNumber numberWithDouble:free] forKey:@"free"];
    
    pluginResult = [CDVPluginResult resultWithStatus:CDVCommandStatus_OK messageAsDictionary:result];
    
    [self.commandDelegate sendPluginResult:pluginResult callbackId:command.callbackId];
}

-(NSUInteger)getDirectoryFileSize:(NSURL *)directoryUrl
{
    NSUInteger result = 0;
    NSArray *properties = [NSArray arrayWithObjects: NSURLLocalizedNameKey,
                           NSURLCreationDateKey, NSURLLocalizedTypeDescriptionKey, nil];
    
    NSArray *array = [[NSFileManager defaultManager]
                      contentsOfDirectoryAtURL:directoryUrl
                      includingPropertiesForKeys:properties
                      options:(NSDirectoryEnumerationSkipsHiddenFiles)
                      error:nil];
    
    for (NSURL *fileSystemItem in array) {
        BOOL directory = NO;
        [[NSFileManager defaultManager] fileExistsAtPath:[fileSystemItem path] isDirectory:&directory];
        if (!directory) {
            result += [[[[NSFileManager defaultManager] attributesOfItemAtPath:[fileSystemItem path] error:nil] objectForKey:NSFileSize] unsignedIntegerValue];
        }
        else {
            result += [self getDirectoryFileSize:fileSystemItem];
        }
    }
    
    return result;
}

@end