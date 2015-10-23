#include "JMD5.h"

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




