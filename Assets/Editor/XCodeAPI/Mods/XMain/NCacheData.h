//
//  NCacheData.h
//  Unity-iPhone
//
//  Created by 陈品东 on 2019/2/21.
//
// 数据持久化

#import <Foundation/Foundation.h>
#import "NObject.h"
#import "NConfig.h"

@interface NCacheData: NObject

NSHARE_INSTANCE();

- (NSString *)getLocalData:(NSString *)key;
- (void)setLocalData:(NSString *)key data:(NSString *)data;
- (void)cleanLocalData:(NSString *)key;

@end

