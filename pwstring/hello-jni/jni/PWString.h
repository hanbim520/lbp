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
//������У��
//��ȡ��λ��У����
//�ߺ�	����	������	��ǰ����	������� 	�Ƿ�PC	������� ��ʽ
int GetPWCheckValue4(long LineID, long CilentID, long MaxProfit, long Profit, long CheckCount, BOOL IsPC, char Type, long UserFunc, char *checkString);//
// crc: У���� 
// pwstring_in: �û��������(8���ֽ�)
// checkstr_out: Ҫ��������Ƭ������(32���ֽ�)
void CreateCheckPWString( long LineID, long CilentID,  long  MaxProfit, long Profit, long CheckCount, long crc,  long pwstring_in, char *checkstr_out );
// recv_buff: �Ӽ���Ƭ�õ�������(��Ч�ֽ�12������ȥ��ָ��ͷ��β�������ֽ�)
// day4byte: 4���ֽڵ�����(long��)
int  GetCheckPWStringValue( char *recv_buff, char *day4byte, int *altstatus4byte );
#endif
