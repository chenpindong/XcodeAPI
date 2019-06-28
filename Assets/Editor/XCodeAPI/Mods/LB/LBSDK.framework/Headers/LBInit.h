//
//  LBInit.h
//  LBSDK
//
//  Created by xunjiangtao on 2018/6/6.
//  Copyright © 2018年 杨冰冰. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface LBInit : NSObject

/**
 实例
 
 @return 返回实例
 */
+ (instancetype)sharedInstance;

/**
 SDK启动入口
 
 @param launchOptions 传入APP启动参数
 @return 返回为YES或者NO。
 */
- (BOOL)LBSDKShouldInitWithLaunchOptions:(NSDictionary *)launchOptions;

#pragma mark - 一些可能需要用到的接口
// 老用户下载完整资源接口
+ (void)downloadFullResource;
// 热更请求接口
+ (void)queryUpdate;
// bwbx是否是小包
+ (bool)isSplitPackage;
// bwbx资源是否下载完成
+ (bool)isDownloadFinished;
// bwbx后台下载进度
+ (int)backgroundDownloadProgress;


@end
