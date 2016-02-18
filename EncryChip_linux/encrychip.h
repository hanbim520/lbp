#ifndef _ENCRYCHIP_H
#define _ENCRYCHIP_H

typedef char			    BOOL;
typedef unsigned char       U8;
typedef unsigned short int  U16;
typedef unsigned long       U32;

#define  ABS(x)	 (x)<0?-(x):(x)
void  DWORDTOBUFF(unsigned char *buff, unsigned long vlaue);
unsigned long  BUFFTODWORD(unsigned char *buff);
extern "C"
{
// 获取四位的报账码校验码.
// 线号	机号		总利润	当前利润		算码次数
char* ReturnCheckCode(int LineID, int CilentID, int MaxProfit, int Profit, int CheckCount);
// 生成报账数据, 传给加密片.
// crc: 校验码
// pwstring_in: 用户输入的码(8个字节)
// return: 要传给加密片的数据(32个字节)
char* CreateReportBytes(int LineID, int CilentID, int  MaxProfit, int Profit, int CheckCount, int crc, int pwstring_in);
// 解析加密片传回的打码结果
// recv_buff: 从加密片得到的数据(有效字节12个——去掉指令头和尾部无用字节)
char* ParserCheckData(char *recv_buff);
// 解密从金手指传回来的数据
// Input: 61个字节数组
// Output: 59个字节数组
int DecryptIOData(unsigned char *Input, unsigned short int in_len, unsigned char **Output);
// 传给金手指的数据，先通过它来加密。
// Input: 61个字节数组
// Output: 63个字节数组
int EncryptIOData(U8 *Input, U16 in_len, U8 **Output);
// 释放byte数组指针
void FreeByteArray(unsigned char *ptr);
}


#endif
