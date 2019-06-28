//
//  NCacheData.m
//  Unity-iPhone
//
//  Created by 陈品东 on 2019/2/21.
//
// 数据持久化

#import "NCacheData.h"
#import "NUtil.h"
#import "KpKeychainItemWrapper.h"
#import <sys/utsname.h>

@implementation NCacheData

NCLASS_INSTANCE(NCacheData);

static NSString *myAppid = nil;
static NSString *identifier = nil;

- (void)setAppId:(NSString *)appId
{
    myAppid = [appId copy];
    identifier = [NSString stringWithFormat:@"xp-%@", myAppid];
}

- (NSString *)getLocalData:(NSString *)key
{
    [self setAppId:key];
    KpKeychainItemWrapper *keychainItem = [[KpKeychainItemWrapper alloc] initWithIdentifier:identifier accessGroup:nil];
    NSString *str = [keychainItem objectForKey:(id)kSecValueData];
    return str;
}

- (void)setLocalData:(NSString *)key data:(NSString *)data
{
    [self setAppId:key];
    KpKeychainItemWrapper *keychainItem = [[KpKeychainItemWrapper alloc] initWithIdentifier:identifier accessGroup:nil];
    [keychainItem setObject:data forKey:(id)kSecValueData];
    [keychainItem release];
}

- (void)cleanLocalData:(NSString *)key
{
    [self setAppId:key];
    KpKeychainItemWrapper *keychainItem = [[KpKeychainItemWrapper alloc] initWithIdentifier:identifier accessGroup:nil];
    [keychainItem setObject:@"" forKey:(id)kSecValueData];
    [keychainItem release];
}

extern "C"
{
    const char* XP_getLocalData(const char* key)
    {
        NSString *temp = [[NCacheData shareInstance] getLocalData:PSTRING(key)];
        return [NUtil CopyString:[temp UTF8String]];
    }
    
    void XP_setLocalData(const char* key, const char* data)
    {
        [[NCacheData shareInstance] setLocalData:PSTRING(key) data:PSTRING(data)];
    }
    
    void XP_cleanLocalData(const char* key)
    {
        [[NCacheData shareInstance] cleanLocalData:PSTRING(key)];
    }
}

@end
