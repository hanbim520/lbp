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
// ��ȡ��λ�ı�����У����.
// �ߺ�	����		������	��ǰ����		�������
char* ReturnCheckCode(int LineID, int CilentID, int MaxProfit, int Profit, int CheckCount);
// ���ɱ�������, ��������Ƭ.
// crc: У����
// pwstring_in: �û��������(8���ֽ�)
// return: Ҫ��������Ƭ������(32���ֽ�)
char* CreateReportBytes(int LineID, int CilentID, int  MaxProfit, int Profit, int CheckCount, int crc, int pwstring_in);
// ��������Ƭ���صĴ�����
// recv_buff: �Ӽ���Ƭ�õ�������(��Ч�ֽ�12������ȥ��ָ��ͷ��β�������ֽ�)
char* ParserCheckData(char *recv_buff);
// ���ܴӽ���ָ������������
// Input: 61���ֽ�����
// Output: 59���ֽ�����
int DecryptIOData(unsigned char *Input, unsigned short int in_len, unsigned char **Output);
// ��������ָ�����ݣ���ͨ���������ܡ�
// Input: 61���ֽ�����
// Output: 63���ֽ�����
int EncryptIOData(U8 *Input, U16 in_len, U8 **Output);
// �ͷ�byte����ָ��
void FreeByteArray(unsigned char *ptr);
}


#endif
