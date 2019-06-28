//
//  NConfig.h
//  Unity-iPhone
//
//  Created by DreamTim on 5/23/14.
//
//

#ifndef Unity_iPhone_NConfig_h
#define Unity_iPhone_NConfig_h

#define ALog(format, ...) NSLog((@"%s [L%d] " format), __PRETTY_FUNCTION__, __LINE__, ##__VA_ARGS__)

//#ifndef RELEASE
//#define NLog(format, ...) ALog(format, ##__VA_ARGS__)
//#else
//#define NLog(...)
//#endif
#ifndef RELEASE
#define NLog(format, ...) 
#else
#define NLog(...)
#endif

#define PSTRING(str) [NSString stringWithUTF8String:str]
#define PSTRING2(value) [NSString stringWithFormat:@"%@", value]

//注册class
#define NREGIST_CLASS(ClassNameInstance)	\
@implementation NPlatform (NPlatformImpl)  \
- (void) initCreate  \
{ \
    ClassNameInstance; \
} \
@end \

//定义shareInstance
#define NSHARE_INSTANCE()  \
+ (instancetype)shareInstance;


//类单例的宏
#define NCLASS_INSTANCE(ClassName) \
+ (instancetype)shareInstance \
{ \
    static ClassName *_kshareInstance = nil; \
    static dispatch_once_t onceToken; \
    dispatch_once(&onceToken, ^{ \
        _kshareInstance = [[ClassName alloc] init]; \
    }); \
    return _kshareInstance; \
}

#define NTEST(ClassType) \
- (void)test \
{ \
    static ClassType * _test = nil; \
    [_test init]; \
}

#endif
