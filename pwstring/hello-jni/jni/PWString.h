#ifndef PW_H
#define PW_H
//#include "JCrc32.c"
typedef signed short int                   BOOL;
//typedef signed char         S8;
//typedef signed short int    S16;
//typedef signed long         S32;
////typedef signed __int64         S64;
//
//typedef bool                 U1;
typedef unsigned char       U8;
typedef unsigned short int  U16;
typedef unsigned long       U32;
//typedef unsigned __int64       U64;

#define  ABS(x)	 (x)<0?-(x):(x)
void  DWORDTOBUFF( 	unsigned char *buff, unsigned long vlaue );
unsigned long  BUFFTODWORD( 	unsigned char *buff );
//报账码校验
//获取四位的校验码
//线号	机号	总利润	当前利润	算码次数 	是否PC	机器类别 公式
int GetPWCheckValue4(long LineID, long CilentID, long MaxProfit, long Profit, long CheckCount, BOOL IsPC, char Type, long UserFunc, char *checkString);//
// crc: 校验码 
// pwstring_in: 用户输入的码(8个字节)
// checkstr_out: 要传给加密片的数据(32个字节)
void CreateCheckPWString( long LineID, long CilentID,  long  MaxProfit, long Profit, long CheckCount, long crc,  long pwstring_in, char *checkstr_out );
// recv_buff: 从加密片得到的数据(有效字节12个——去掉指令头和尾部无用字节)
// day4byte: 4个字节的天数(long型)
int  GetCheckPWStringValue( char *recv_buff, char *day4byte, int *altstatus4byte );
#endif
