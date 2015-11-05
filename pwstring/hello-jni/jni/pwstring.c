#include "PWString.h"
#include <stdio.h>
#include <string.h>
#include <math.h>
#include <jni.h>
#include <android/log.h>
#include <sys/time.h>   
//#include "JMD5.h"
//#include "JMD5.c"

#define LOG_TAG "Unity"
#define LOGV(...) __android_log_print(ANDROID_LOG_VERBOSE, LOG_TAG, __VA_ARGS__) 
#define LOGD(...) __android_log_print(ANDROID_LOG_DEBUG, LOG_TAG, __VA_ARGS__)
#define LOGI(...) __android_log_print(ANDROID_LOG_INFO, LOG_TAG, __VA_ARGS__)
#define LOGW(...) __android_log_print(ANDROID_LOG_WARN, LOG_TAG, __VA_ARGS__)
#define LOGE(...) __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

#define   MD5A  0xf2a118d4L
#define   MD5B  0x6be15889L
#define   MD5C  0x5f54dc4eL
#define   MD5D  0x3b0b4233L

#define  BIT(x)              (1UL << (x))
/////////////////////////
// JMD5.c content
U32 Md5_A = MD5A, Md5_B = MD5B, Md5_C = MD5C,  Md5_D = MD5D;
U32 Md5_F(U32 X,U32 Y,U32 Z)          { return (X&Y)|((~X)&Z); }
U32 Md5_G(U32 X,U32 Y,U32 Z)           { return (X&Z)|(Y&(~Z));}
U32 Md5_H(U32 X,U32 Y,U32 Z)        { return (X^Y^Z);}
U32 Md5_I(U32 X,U32 Y,U32 Z)          { return (Y^(X|(~Z)));} 
U32 ROTATE_LEFT(U32 x, U32 n) { return (((x) << (n)) | ((x) >> (32-(n)))); }

U32 Md5_FF(U32 a, U32 b, U32 c, U32 d, U32 x, U32 s, U32 ac) {(a) += Md5_F((b), (c), (d));( a) += (x) + ac;(a) = ROTATE_LEFT ((a), (s));(a) += (b); return a;}
U32 Md5_GG(U32 a, U32 b, U32 c, U32 d, U32 x, U32 s, U32 ac) {(a) += Md5_G((b), (c), (d));a += (x) + ac;(a) = ROTATE_LEFT ((a), (s));  (a) += (b);return a;}
U32 Md5_HH(U32 a, U32 b, U32 c, U32 d, U32 x, U32 s, U32 ac) {(a) += Md5_H((b), (c), (d));(a ) +=  (x) + ac;(a) = ROTATE_LEFT ((a), (s));(a) += (b);return a;}
U32 Md5_II(U32 a, U32 b, U32 c, U32 d, U32 x, U32 s, U32 ac) {(a) += Md5_I((b), (c), (d));(a) += (x) + (ac);(a) = ROTATE_LEFT ((a), (s));(a) += (b);return a;}
//void ModifyMD5Value(U32 numberA, U32 numberB, U32 numberC, U32 numberD)
//{
//	Md5_A = numberA;
//	Md5_B = numberB;
//	Md5_C = numberC;
//	Md5_D = numberD;
//}
void JMD5(U8 *buff , int len, U32 *rA, U32 *rB, U32 *rC, U32 *rD)
{
U32 a = Md5_A, b= Md5_B, c= Md5_C, d= Md5_D;
U32 len1;
U8  Info[128];
U32 Mj[16];
U32 state[4];
int nCount, pos = 0;
U8  i = 0;

	  // len = strlen( buff );	
	   nCount = (len*8)%512;
	   len1 = len;
	   if( nCount < 448 )
	       nCount = (448 - nCount)/8;
	   else
		   nCount = (448  + 512 - nCount)/8;
	  // Info = (U8*)malloc( len + nCount + 8);
	   memset( Info, 0, len + nCount + 8 );
	   memcpy( Info, buff, len);
	  // strcpy( Info,buff );
	   //Моід
	   Info[len] = 0x80;
	   len1 = len*8;
	   len = len +nCount + 8;
	   DWORDTOBUFF( &Info[len -  8], len1);
	   //memcpy( &Info[len -  8], (U8*)&len1, 4 );
	   a = Md5_A; b= Md5_B; c= Md5_C; d= Md5_D;
	   do{
	   state[ 0 ] = a;state[ 1 ] = b;state[ 2 ] = c;state[ 3 ] = d;
	   for( i = 0; i < 16; i ++ )
	   {
	      Mj[i] = BUFFTODWORD( &Info[ pos + i*4 ] );	 
	   }
	   
	   //memcpy( (U8*)Mj, &Info[ pos ], 64 );
	   pos += 64;
		a = Md5_FF(a, b, c, d, Mj[0], 7, 0xd76aa478);
		d = Md5_FF(d, a, b, c, Mj[1], 12, 0xe8c7b756);
		c = Md5_FF(c, d, a, b, Mj[2], 17, 0x242070db);
		b = Md5_FF(b, c, d, a, Mj[3], 22, 0xc1bdceee);

		a = Md5_FF(a, b, c, d, Mj[4], 7, 0xf57c0faf);
		d = Md5_FF(d, a, b, c, Mj[5], 12, 0x4787c62a);
		c = Md5_FF(c, d, a, b, Mj[6], 17, 0xa8304613);
		b = Md5_FF(b, c, d, a, Mj[7], 22, 0xfd469501);

		a = Md5_FF(a, b, c, d, Mj[8], 7, 0x698098d8);
		d = Md5_FF(d, a, b, c, Mj[9], 12, 0x8b44f7af);
		c = Md5_FF(c, d, a, b, Mj[10], 17, 0xffff5bb1);
		b = Md5_FF(b, c, d, a, Mj[11], 22, 0x895cd7be);

		a = Md5_FF(a, b, c, d, Mj[12], 7, 0x6b901122);
		d = Md5_FF(d, a, b, c, Mj[13], 12, 0xfd987193);
		c = Md5_FF(c, d, a, b, Mj[14], 17, 0xa679438e);
		b = Md5_FF(b, c, d, a, Mj[15], 22, 0x49b40821);

		 a = Md5_GG(a, b, c, d, Mj[1], 5, 0xf61e2562);
		 d = Md5_GG(d, a, b, c, Mj[6], 9, 0xc040b340);
		 c = Md5_GG(c, d, a, b, Mj[11], 14, 0x265e5a51);
		 b = Md5_GG(b, c, d, a, Mj[0], 20, 0xe9b6c7aa);

		 a = Md5_GG(a, b, c, d, Mj[5], 5, 0xd62f105d);
		 d = Md5_GG(d, a, b, c, Mj[10], 9, 0x02441453);
		 c = Md5_GG(c, d, a, b, Mj[15], 14, 0xd8a1e681);
		 b = Md5_GG(b, c, d, a, Mj[4], 20, 0xe7d3fbc8);

		 a = Md5_GG(a, b, c, d, Mj[9], 5, 0x21e1cde6);
		 d = Md5_GG(d, a, b, c, Mj[14], 9, 0xc33707d6);
		 c = Md5_GG(c, d, a, b, Mj[3], 14, 0xf4d50d87);
		 b = Md5_GG(b, c, d, a, Mj[8], 20, 0x455a14ed);

		 a = Md5_GG(a, b, c, d, Mj[13], 5, 0xa9e3e905);
		 d = Md5_GG(d, a, b, c, Mj[2], 9, 0xfcefa3f8);
		 c = Md5_GG(c, d, a, b, Mj[7], 14, 0x676f02d9);
		 b = Md5_GG(b, c, d, a, Mj[12], 20, 0x8d2a4c8a);

		a = Md5_HH(a, b, c, d, Mj[5], 4, 0xfffa3942);
		d = Md5_HH(d, a, b, c, Mj[8], 11, 0x8771f681);
		c = Md5_HH(c, d, a, b, Mj[11], 16, 0x6d9d6122);
	    b = Md5_HH(b, c, d, a, Mj[14], 23, 0xfde5380c);

		a = Md5_HH(a, b, c, d, Mj[1], 4, 0xa4beea44);
		d = Md5_HH(d, a, b, c, Mj[4], 11, 0x4bdecfa9);
		c = Md5_HH(c, d, a, b, Mj[7], 16, 0xf6bb4b60);
		b = Md5_HH(b, c, d, a, Mj[10], 23, 0xbebfbc70);

		a = Md5_HH(a, b, c, d, Mj[13], 4, 0x289b7ec6);
		d = Md5_HH(d, a, b, c, Mj[0], 11, 0xeaa127fa);
		c = Md5_HH(c, d, a, b, Mj[3], 16, 0xd4ef3085);
		b = Md5_HH(b, c, d, a, Mj[6], 23, 0x04881d05);

		a = Md5_HH(a, b, c, d, Mj[9], 4, 0xd9d4d039);
		d = Md5_HH(d, a, b, c, Mj[12], 11, 0xe6db99e5);
		c = Md5_HH(c, d, a, b, Mj[15], 16, 0x1fa27cf8);
		b = Md5_HH(b, c, d, a, Mj[2], 23, 0xc4ac5665);

		a = Md5_II(a, b, c, d, Mj[0], 6, 0xf4292244);
		d = Md5_II(d, a, b, c, Mj[7], 10, 0x432aff97);
		c = Md5_II(c, d, a, b, Mj[14], 15, 0xab9423a7);
		b = Md5_II(b, c, d, a, Mj[5], 21, 0xfc93a039);
		a = Md5_II(a, b, c, d, Mj[12], 6, 0x655b59c3);
		d = Md5_II(d, a, b, c, Mj[3], 10, 0x8f0ccc92);
		c = Md5_II(c, d, a, b, Mj[10], 15, 0xffeff47d);
		b = Md5_II(b, c, d, a, Mj[1], 21, 0x85845dd1);
		a = Md5_II(a, b, c, d, Mj[8], 6, 0x6fa87e4f);
		d = Md5_II(d, a, b, c, Mj[15], 10, 0xfe2ce6e0);
		c = Md5_II(c, d, a, b, Mj[6], 15, 0xa3014314);
		b = Md5_II(b, c, d, a, Mj[13], 21, 0x4e0811a1);
		a = Md5_II(a, b, c, d, Mj[4], 6, 0xf7537e82);
		d = Md5_II(d, a, b, c, Mj[11], 10, 0xbd3af235);
		c = Md5_II(c, d, a, b, Mj[2], 15, 0x2ad7d2bb);
		b = Md5_II(b, c, d, a, Mj[9], 21, 0xeb86d391); 

		a = state[0] + a;
		b = state[1] + b;
		c = state[2] + c;
		d = state[3] + d;
	   }while( pos < len );
		//{
		//	goto start;
		//}
		*rA = a;
		*rB = b;
		*rC = c;
		*rD = d;
		//memcpy( value, (U8*)(&a), 4 );  
		//memcpy( value + 4, (U8*)(&b), 4 );  
		//memcpy( value+ 8, (U8*)(&c), 4 );  
		//memcpy( value+12, (U8*)(&d), 4 );  
		//free( Info );
		//Md5_A = MD5A; Md5_B = MD5B; Md5_C = MD5C; Md5_D = MD5D;
		//return value;
}
// JMD5.c end
//////////////////////////////////

void DWORDTOBUFF(unsigned char *buff, unsigned long vlaue)
{
   	 buff[ 0 ] =  (unsigned char )(vlaue);
     buff[ 1 ] =  (unsigned char )(vlaue>>8);
	 buff[ 2 ] =  (unsigned char )(vlaue>>16);
	 buff[ 3 ] =  (unsigned char )(vlaue>>24);
}

unsigned long  BUFFTODWORD(unsigned char *buff)
{
unsigned long temp;

	temp = buff[3];
	temp = temp<<8;
	temp |= buff[2];
	temp = temp<<8;
	temp |= buff[1];
	temp = temp<<8;
	temp |= buff[0];
	return  temp;
}

jstring Java_com_zxproduct_www_UnityPlayerActivity_GetPWCheckValue4(JNIEnv* env, jobject thiz, 
jlong LineID, jlong CilentID, jlong  MaxProfit, jlong Profit, jlong CheckCount)
{	
	//char checkString[] = {"1234567890"};
	char *checkString = (char*)malloc(10);
	strcpy(checkString, "1234567890");
	GetPWCheckValue4((long)LineID, (long)CilentID,  (long)MaxProfit, (long)Profit, (long)CheckCount, 1, 2, 12345678, checkString);
	jstring jstrBuf = (*env)->NewStringUTF(env, checkString);
	free(checkString);
	return jstrBuf;
}

//               线号          机号          总利润              当前利润            算码次数        是否PC  机器类别	 公式
// ispc = 1, type = 2, userfunc = 12345678
int GetPWCheckValue4( long LineID, long CilentID,  long  MaxProfit, long Profit, long CheckCount, BOOL IsPC, char Type,  long UserFunc, char *checkString )
{
unsigned char   Buff[128];
unsigned long  Crc = 0, PC = 0;
unsigned long  result_A = 0, result_B = 0, result_C = 0, result_D = 0;
unsigned long  XorKey = 0x4789fec5;
char  tmpBuff[20];

	DWORDTOBUFF( Buff, Profit);
    DWORDTOBUFF( &Buff[4], CilentID);
	DWORDTOBUFF( &Buff[8], LineID);
    DWORDTOBUFF( &Buff[12], CheckCount);
	DWORDTOBUFF( &Buff[16], MaxProfit);
	
	Md5_A = 0x6bef18d4L; Md5_B = 0x15889fdaL; Md5_C = 0x23a4dc4eL;  Md5_D = 0x3b0bf5feL;
	JMD5(  Buff, 20,&result_A,&result_B,&result_C,&result_D);
	Md5_A = MD5A; Md5_B = MD5B; Md5_C = MD5C;  Md5_D = MD5D;
   	Crc = result_A^result_B^result_C;
	Crc ^=UserFunc;
	Crc ^= Profit;
	Crc ^= CheckCount;
	Crc ^= CilentID;
	Crc ^= LineID;
	Crc ^= XorKey;
	Crc =Crc>>3;
	PC = IsPC;
	Crc |= PC<<31;
	PC = Type;
    Crc |= PC<<30;
	sprintf( tmpBuff, "%08lu", Crc );
	memcpy( checkString, &tmpBuff, 4 );
	checkString[4] = 0;
    return 1;
}

jbyteArray Java_com_zxproduct_www_UnityPlayerActivity_CreateCheckPWString(JNIEnv* env, jobject thiz, jlong LineID, jlong CilentID, jlong MaxProfit, jlong Profit, jlong CheckCount, jlong crc, jlong pwstring_in)
{
	int size = 32;
	char *checkstr_out = (char*)malloc(size);
	CreateCheckPWString((long)LineID, (long)CilentID,  (long)MaxProfit, (long)Profit, (long)CheckCount, (long)crc, (long)pwstring_in, checkstr_out);
	jbyteArray result = (*env)->NewByteArray(env, size);  
	jbyte *bytes = (*env)->GetByteArrayElements(env, result, 0);  
    memcpy(bytes, checkstr_out, size * sizeof(jbyte));  
    (*env)->SetByteArrayRegion(env, result, 0, size, bytes);
	free(checkstr_out);
	return result;
}

void CreateCheckPWString( long LineID, long CilentID,  long  MaxProfit, long Profit, long CheckCount, long crc,  long pwstring_in, char *checkstr_out )
{
		sprintf( checkstr_out, "%ld", pwstring_in );
		DWORDTOBUFF( (U8*)&checkstr_out[8],LineID);
		DWORDTOBUFF( (U8*)&checkstr_out[12],CilentID);
		DWORDTOBUFF( (U8*)&checkstr_out[16],MaxProfit);
		DWORDTOBUFF( (U8*)&checkstr_out[20],Profit);
		DWORDTOBUFF( (U8*)&checkstr_out[24],CheckCount);
		DWORDTOBUFF( (U8*)&checkstr_out[28],crc );
}

jstring Java_com_zxproduct_www_UnityPlayerActivity_GetCheckPWStringValue(JNIEnv* env, jobject thiz, jbyteArray recv_buff)
{
	int size = (*env)->GetArrayLength(env, recv_buff);
	jbyte* point = (*env)->GetByteArrayElements(env, recv_buff, 0);
	char *buff = (char*)malloc(size);
	memcpy(buff, point, size);
	
	char* day4byte = (char*)malloc(10);
	char* altstatus4byte = (char*)malloc(10);
	int flag = GetCheckPWStringValue(buff, day4byte, altstatus4byte);	// 0:错误 1:正确
	int *day = (int*)malloc(4);
	memcpy(day, day4byte, 4);
	
	char* result = (char*)malloc(32);
	char* strDay = (char*)malloc(10);
	sprintf(result, "%d", flag);
	sprintf(strDay, "%d", *day);
	strcat(result, ":");
	strcat(result, strDay);
	
	jstring strResult = (*env)->NewStringUTF(env, result);
	free(buff);
	free(day4byte);
	free(altstatus4byte);
	free(day);
	free(strDay);
	free(result);
	(*env)->ReleaseByteArrayElements(env, recv_buff, point, 0);
	return strResult;
}

 unsigned short int   ShiftByte( unsigned short int Number )
{
	unsigned short int  temp = 0;
	temp = (Number&BIT( 15 ))?1:0;
	Number = Number<<1;
	Number |= temp;
	return  Number;
}
unsigned short int   ShiftByte1( unsigned short int Number )
{
	unsigned short int  temp = 0;
	temp = (Number&BIT( 0 ))?1:0;
	temp = temp<<15;
	Number = Number>>1;
	Number |= temp;
	return  Number;
}
// 解密从加密芯片传回来的数据
int   DecryptIODataFromChip( unsigned char *Input, unsigned short int in_len, unsigned char *Output,  unsigned short int  *EnCryptKey )
{
	U16  Temp = 0;
	U16  leng = 0;
	U16  KeyConst = 0x5975;
	U8 XorKey[] = {0x3F,0x2E,0x8A,0xA4,0x5F,0x80,0x17,0xD7,0xDE,0x5B,0x91,0xEF,0x29,0x94,0x77,0xA2,0xAB,0x13,0x1B,0x7A,0x78,0xCF,0x37,0x39,
		                   0x9E,0x4F,0xA1,0x34,0x71,0xDC,0xBE,0x10,0x96,0xEF,0x1F,0x52,0x70,0x63,0x33,0x1B,0xF3,0xDB,0x9E,0x84,0x49,0xB4,0x8C,0xB8,
						   0xE9,0xA4,0x18,0x68,0x4B,0xFB,0x76,0xC5,0x8C,0x5B,0x65,0xBD,0x8E,0xB9,0xA3,0x4E};
	U8    Key1 = 0, Key2 = 0, data = 0;
	int    Count = 2, i = 0;//, j = 0;
//	KeyConst ^= (U16)result_A;
//	KeyConst ^= (U16)result_B;
//	KeyConst ^= (U16)result_C;
//	KeyConst ^= (U16)result_D;
	for(  i = 0; i < in_len; i ++ )
	{
		Input[i] ^= XorKey[i];
	}

	Key1 = (U8)(KeyConst&0x00FF);
	Key2 = (U8)((KeyConst>>8)&0x00FF);
	for(  i = 0; i < in_len; i ++ )
	{
		data = Input[ i ];
		data = data^Key2;
		data = data^Key1;
		Input[i] = data;
	}

    Temp = ((U16)Input[in_len - 1]<<8)|((U16)Input[in_len - 2]);
	*EnCryptKey = Temp;
	do
	{
		*EnCryptKey= ShiftByte1( *EnCryptKey );
		Key1 = (U8)(*EnCryptKey&0x00FF);
		Key2 = (U8)((*EnCryptKey>>8)&0x00FF);
		for(  i = 0; i < in_len - 2; i ++ )
		{
			data = Input[ i ];
			data = data^Key2;
			data = data^Key1;
			Input[i] = data;
		}
		Count --;
	}while( Count > 0 );

	leng = in_len - 2;
	for(  i = 0; i < leng; i ++ )
	{
		Output[i] = Input[ i ];
	}
	return leng;
}

int  GetCheckPWStringValue( char *pwstring_in, char *day4byte, char *altstatus4byte )
{
	//int i;
	char  decrypt_buff[20];
	long  day = 0, bom = 0, flag = 0;
	unsigned short int  Encrypt = 0;
	//for (i = 0; i < 14; ++i)
	//	LOGD("pwstring_in[%d]=%02X", i, pwstring_in[i]);
	DecryptIODataFromChip( (U8*)pwstring_in, 14, decrypt_buff,  &Encrypt );
	//for (i = 0; i < 20; ++i)
	//	LOGD("decrypt_buff[%d]=%02X", i, decrypt_buff[i]);
	day = BUFFTODWORD(  (U8*)&decrypt_buff[0]);
	bom = BUFFTODWORD(  (U8*)&decrypt_buff[4] );
	flag =   BUFFTODWORD(  (U8*)&decrypt_buff[8] );
	memcpy( day4byte, &day, 4 );
	memcpy( altstatus4byte, &bom, 4 );
	return flag;
}

// 传给金手指的数据，先通过它来加密。
// Input: 62个字节数组
// Output: 64个字节数组
int EncryptIOData( U8 *Input, U16  in_len, U8 *Output ,U16 EnCryptKey)
{
	U16  Temp = 0;
	U16  leng = 0;
	U16  KeyConst = 0x5975;
	Temp = EnCryptKey;
	U8    Key1 = 0, Key2 = 0, data = 0;
	int    Count = 2, i = 0, j = 0;
	unsigned char XorKey[] = {0x3F,0x2E,0x8A,0xA4,0x5F,0x80,0x17,0xD7,0xDE,0x5B,0x91,0xEF,0x29,0x94,0x77,0xA2,0xAB,0x13,0x1B,0x7A,0x78,0xCF,0x37,0x39,
		                   0x9E,0x4F,0xA1,0x34,0x71,0xDC,0xBE,0x10,0x96,0xEF,0x1F,0x52,0x70,0x63,0x33,0x1B,0xF3,0xDB,0x9E,0x84,0x49,0xB4,0x8C,0xB8,
						   0xE9,0xA4,0x18,0x68,0x4B,0xFB,0x76,0xC5,0x8C,0x5B,0x65,0xBD,0x8E,0xB9,0xA3,0x4E};
	do
	{
		Key1 = (U8)(EnCryptKey&0x00FF);
		Key2 = (U8)((EnCryptKey>>8)&0x00FF);
		for(  i = 0; i < in_len; i ++ )
		{
			data = Input[ i ];
			data = data^Key1;
			data = data^Key2;
			Input[i] = data;
		}
		EnCryptKey = ShiftByte( EnCryptKey );
		Count --;
	}while( Count > 0 );

	Key1 = (U8)(EnCryptKey&0x00FF);
	Key2 = (U8)((EnCryptKey>>8)&0x00FF);
	for(  i = 0; i < in_len; i ++ )
	{
		Output[i] = Input[ i ];
	}
    Output[ in_len ] = Key1;
    Output[ in_len + 1 ] = Key2; 
	leng = in_len + 2;
	Key1 = (U8)(KeyConst&0x00FF);
	Key2 = (U8)((KeyConst>>8)&0x00FF);
	for(  i = 0; i < leng; i ++ )
	{
		data = Output[ i ];
		data = data^Key1;
		data = data^Key2;
		Output[i] = data;
	}
	for(  i = 0; i < leng; i ++ )
	{
		Output[i] ^= XorKey[i];
	}

	return leng;
}

long Random_Seed()
{
	struct timeval tv;    
   	gettimeofday(&tv,NULL);    
  	return tv.tv_sec * 1000L + tv.tv_usec / 1000L;    
}

long Random_Int(int min, int max, long g_seed)
{
	g_seed=214013*g_seed+2531011;
	return min+(g_seed ^ g_seed>>15)%(max-min+1);
}

JNIEXPORT jbyteArray JNICALL Java_com_zxproduct_www_UnityPlayerActivity_EncryptIOData(JNIEnv* env, jobject thiz, jbyteArray inputArray)
{
	U16 EnCryptKey = (U16)Random_Int(0, 0xffff, Random_Seed());
	U8* input = (U8*)(*env)->GetByteArrayElements(env, inputArray, 0);
	U16 input_len = (U16)(*env)->GetArrayLength(env, inputArray);
	U16 output_len = input_len + 2;
	U8 *output = (U8*)malloc(output_len);
	memset(output, 0, output_len);
	EncryptIOData(input, input_len, output, EnCryptKey);

	jbyteArray outputArray = (*env)->NewByteArray(env, output_len);
  	(*env)->SetByteArrayRegion(env, outputArray, 0, output_len, (jbyte*)output);

	(*env)->ReleaseByteArrayElements(env, inputArray, input, 0);
	free(output);

	return outputArray;
}