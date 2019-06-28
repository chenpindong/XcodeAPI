//
//  NUtil.h
//  Unity-iPhone
//
//  Created by HQ on 14-1-7.
//
//

#import <Foundation/Foundation.h>

@interface NUtil : NSObject {
    
}

+ (const char* )JsonWithData:(id)data;

+ (const char* )CopyString:(const char*)string;

+ (void)UnitySendMessage:(const char*)gameObject
                  Method:(const char*)method
                     Msg:(NSDictionary *)msg;

+ (NSString *)MacAddress;
+ (NSString *)IDFA;
+ (NSString *)IDFV;
+ (NSString *)IPAddress;

+ (NSString *)md5:(NSString *)data;
+ (NSString *)md5:(Byte *)data length:(int)length;
@end


@interface NSDictionary (DictionaryHelper)

+ (NSDictionary *)dictionaryWithString:(NSString *)string;
+ (NSDictionary *)dictionaryWithUTF8String :(const char*)string;

- (const char* )UTF8String;
- (NSString* )DictionaryToString;

- (BOOL)containsKey:(NSString *)key;
@end

