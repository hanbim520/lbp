#ifndef _ENCRYCHIP_H
#define _ENCRYCHIP_H

//typedef unsigned short int			BOOL;
typedef unsigned char       U8;
typedef unsigned short int  U16;
typedef unsigned long       U32;

#define  ABS(x)	 (x)<0?-(x):(x)
void  DWORDTOBUFF(unsigned char *buff, unsigned long vlaue);
unsigned long  BUFFTODWORD(unsigned char *buff);
//������У��, ��ȡ��λ��У����.
//�ߺ�	����		������	��ǰ����		�������		�Ƿ�PC	�������		��ʽ
extern "C" _declspec(dllexport) int GetPWCheckValue4(long LineID, long CilentID, long MaxProfit, long Profit, long CheckCount, BOOL IsPC, char Type, long UserFunc, char *checkString);
// crc: У���� 
// pwstring_in: �û��������(8���ֽ�)
// checkstr_out: Ҫ��������Ƭ������(32���ֽ�)
extern "C" _declspec(dllexport) void CreateCheckPWString(long LineID, long CilentID, long  MaxProfit, long Profit, long CheckCount, long crc, long pwstring_in, char *checkstr_out);
// recv_buff: �Ӽ���Ƭ�õ�������(��Ч�ֽ�12������ȥ��ָ��ͷ��β�������ֽ�)
// day4byte: 4���ֽڵ�����(long��)
extern "C" _declspec(dllexport) int GetCheckPWStringValue(char *recv_buff, char *day4byte, char *altstatus4byte);
// ���ܴӽ���ָ������������
// Input: 61���ֽ�����
// Output: 59���ֽ�����
extern "C" _declspec(dllexport) int DecryptIOData(unsigned char *Input, unsigned short int in_len, unsigned char **Output);
// ��������ָ�����ݣ���ͨ���������ܡ�
// Input: 61���ֽ�����
// Output: 63���ֽ�����
extern "C" _declspec(dllexport) int EncryptIOData(U8 *Input, U16 in_len, U8 **Output);
// �ͷ�byte����ָ��
extern "C" _declspec(dllexport) void FreeByteArray(unsigned char *ptr);

#endif