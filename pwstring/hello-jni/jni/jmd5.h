
#ifndef  JMD5_H
#define  JMD5_H
#include <stdio.h>
#include <string.h>
#define   MD5A  0xf2a118d4L
#define   MD5B  0x6be15889L
#define   MD5C  0x5f54dc4eL
#define   MD5D  0x3b0b4233L

//typedef signed short int                   BOOL;
typedef signed char         S8;
typedef signed short int    S16;
typedef signed long         S32;
//typedef signed __int64         S64;

//typedef bool                 U1;
//typedef unsigned char       U8;
//typedef unsigned short int  U16;
//typedef unsigned long       U32;
//typedef unsigned __int64       U64;


/*
*轮函数
*/
//U32   Md5_F(U32 X,U32 Y,U32 Z)          { return (X&Y)|((~X)&Z); }
//U32   Md5_G(U32 X,U32 Y,U32 Z)           { return (X&Z)|(Y&(~Z));}
//U32   Md5_H(U32 X,U32 Y,U32 Z)        { return (X^Y^Z);}
//U32   Md5_I(U32 X,U32 Y,U32 Z)          { return (Y^(X|(~Z)));} 
//U32   ROTATE_LEFT(U32 x, U32 n)   { return  (((x) << (n)) | ((x) >> (32-(n)))); }
//
//U32  Md5_FF(  U32 a,  U32 b,  U32 c,  U32 d,  U32 x,  U32 s,  U32 ac) {(a) += Md5_F ((b), (c), (d)) ;( a) += (x) + ac;(a) = ROTATE_LEFT ((a), (s));(a) += (b); return a;}
//U32  Md5_GG( U32 a,  U32 b,  U32 c,  U32 d,  U32 x,  U32 s,  U32 ac)  {(a) += Md5_G ((b), (c), (d));a += (x) + ac;(a) = ROTATE_LEFT ((a), (s));  (a) += (b);return a;}
//U32  Md5_HH( U32  a, U32  b, U32  c, U32 d,  U32 x,  U32 s, U32 ac) { (a) +=Md5_H ((b), (c), (d));(a ) +=  (x) + ac;(a) = ROTATE_LEFT ((a), (s));(a) += (b);return a;}
//U32  Md5_II( U32 a, U32  b,  U32 c,  U32 d,  U32 x, U32 s,  U32 ac) {(a) += Md5_I ((b), (c), (d));(a) += (x) + (ac);(a) = ROTATE_LEFT ((a), (s));(a) += (b);return a;}

//修改链接值
//void ModifyMD5Value( U32  numberA, U32  numberB,U32  numberC,U32  numberD );
void   JMD5( U8 *buff , int len,  U32 *rA, U32 *rB, U32 *rC, U32 *rD  );
//void     MD5( char *buff, U8 *value );
#endif
